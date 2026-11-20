using Microsoft.EntityFrameworkCore;
using Rabbithole.Domain.Database.Context;
using Rabbithole.Domain.DTOs.Playlists.Requests;
using Rabbithole.Domain.DTOs.Playlists.Responses;
using Rabbithole.Domain.DTOs.ThirdParty.Navidrome;
using Rabbithole.Domain.Extensions;
using Rabbithole.Domain.Interfaces.Api.Playlists;
using Rabbithole.Domain.Interfaces.Helpers;
using Rabbithole.Domain.Interfaces.ThirdParty;
using Rabbithole.Domain.Results;

namespace Rabbithole.Domain.Services.Playlists
{
    // The user's own Navidrome playlists, read and changed with their own Navidrome account
    public class PlaylistsService(
        AppDbContext context,
        ICurrentUserHelper currentUserHelper,
        INavidromeApiClient navidromeApiClient) : IPlaylistsService
    {
        public async Task<GetPlaylistsDto> GetPlaylists()
        {
            var credentials = await currentUserHelper.GetCredentials();
            var weeklyPlaylistId = await GetWeeklyPlaylistId();
            var playlists = await navidromeApiClient.GetPlaylists(credentials);

            return new GetPlaylistsDto
            {
                Playlists = playlists
                    .Select(p => p.ToPlaylistItem(credentials.Username, weeklyPlaylistId))
                    .OrderByDescending(p => p.IsRabbitholeWeekly)
                    .ThenByDescending(p => p.IsOwnedByMe)
                    .ThenBy(p => p.Name)
                    .ToArray()
            };
        }

        public async Task<Result<GetPlaylistDto>> GetPlaylist(string playlistId)
        {
            var credentials = await currentUserHelper.GetCredentials();
            var playlist = await navidromeApiClient.GetPlaylist(credentials, playlistId);

            if (playlist == null)
            {
                return Result.NotFoundFail<GetPlaylistDto>("That playlist doesn't exist.");
            }

            return Result.Ok(new GetPlaylistDto
            {
                Playlist = playlist.ToPlaylistItem(credentials.Username, await GetWeeklyPlaylistId()),
                Songs = (playlist.Entry ?? []).Select(s => s.ToLibrarySongItem()).ToArray()
            });
        }

        public async Task<Result<string>> CreatePlaylist(CreatePlaylistRequest request)
        {
            var name = request.Name.Trim();

            if (name.Length == 0)
            {
                return Result.Fail<string>("Give the playlist a name.");
            }

            var credentials = await currentUserHelper.GetCredentials();
            var playlist = await navidromeApiClient.CreatePlaylist(credentials, name, []);

            return Result.Ok(playlist.Id);
        }

        public async Task<Result> AddSongsToPlaylist(AddSongsToPlaylistRequest request)
        {
            if (request.SongIds.Length == 0)
            {
                return Result.Fail("Pick at least one song to add.");
            }

            var credentials = await currentUserHelper.GetCredentials();
            var ownership = await GetOwnPlaylist(credentials, request.PlaylistId);

            if (!ownership.Success)
            {
                return ownership;
            }

            await navidromeApiClient.UpdatePlaylist(credentials, request.PlaylistId, request.SongIds, []);

            return Result.Ok();
        }

        public async Task<Result> RemoveSongFromPlaylist(RemoveSongFromPlaylistRequest request)
        {
            var credentials = await currentUserHelper.GetCredentials();
            var ownership = await GetOwnPlaylist(credentials, request.PlaylistId);

            if (!ownership.Success)
            {
                return ownership;
            }

            if (request.SongIndex < 0 || request.SongIndex >= ownership.Value.SongCount)
            {
                return Result.Fail("That song isn't in the playlist any more. Refresh and try again.");
            }

            await navidromeApiClient.UpdatePlaylist(credentials, request.PlaylistId, [], [request.SongIndex]);

            return Result.Ok();
        }

        private async Task<Result<SubsonicPlaylist>> GetOwnPlaylist(NavidromeCredentials credentials, string playlistId)
        {
            var playlist = await navidromeApiClient.GetPlaylist(credentials, playlistId);

            if (playlist == null)
            {
                return Result.NotFoundFail<SubsonicPlaylist>("That playlist doesn't exist.");
            }

            if (!string.Equals(playlist.Owner, credentials.Username, StringComparison.OrdinalIgnoreCase))
            {
                return Result.Fail<SubsonicPlaylist>("You can only change your own playlists.");
            }

            return Result.Ok(playlist);
        }

        private async Task<string?> GetWeeklyPlaylistId()
        {
            var userId = currentUserHelper.GetUserId();

            return await context.Users
                .Where(u => u.Id == userId)
                .Select(u => u.DiscoverPlaylistId)
                .FirstOrDefaultAsync();
        }
    }
}
