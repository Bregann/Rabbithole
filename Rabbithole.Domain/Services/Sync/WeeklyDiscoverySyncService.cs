using Microsoft.EntityFrameworkCore;
using Rabbithole.Domain.Database.Context;
using Rabbithole.Domain.DTOs.Discover.Responses;
using Rabbithole.Domain.DTOs.ThirdParty.Navidrome;
using Rabbithole.Domain.Extensions;
using Rabbithole.Domain.Interfaces.Api.Discover;
using Rabbithole.Domain.Interfaces.Api.Sync;
using Rabbithole.Domain.Interfaces.Helpers;
using Rabbithole.Domain.Interfaces.ThirdParty;
using Rabbithole.Domain.Results;
using Serilog;

namespace Rabbithole.Domain.Services.Sync
{
    // Every week: take each user's ListenBrainz "Weekly Exploration", keep the songs already in the library,
    // and rebuild their private "Rabbithole Weekly" playlist in Navidrome. Songs they don't own appear
    // in the app's Weekly picks instead, where they can be previewed and requested.
    public class WeeklyDiscoverySyncService(
        AppDbContext context,
        INavidromeCredentialHelper navidromeCredentialHelper,
        INavidromeApiClient navidromeApiClient,
        IWeeklyExplorationService weeklyExplorationService) : IWeeklyDiscoverySyncService
    {
        public const string PlaylistName = "Rabbithole Weekly";

        public async Task RunForAllUsers()
        {
            var userIds = await context.Users
                .Where(u => u.ListenBrainzUsername != null)
                .Select(u => u.Id)
                .ToArrayAsync();

            foreach (var userId in userIds)
            {
                try
                {
                    var result = await RunForUser(userId);

                    if (result.Success)
                    {
                        Log.Information("[Sync] Weekly playlist for user {UserId}: {Matched}/{Total} songs", userId, result.Value.MatchedSongs, result.Value.TotalTracks);
                    }
                    else
                    {
                        Log.Warning("[Sync] Weekly playlist for user {UserId} skipped: {Error}", userId, result.Error);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "[Sync] Weekly playlist for user {UserId} failed", userId);
                }
            }
        }

        public async Task<Result<SyncWeeklyPlaylistDto>> RunForUser(int userId)
        {
            var user = await context.Users.FindAsync(userId);

            if (user == null)
            {
                return Result.NotFoundFail<SyncWeeklyPlaylistDto>("Couldn't find that user.");
            }

            if (user.ListenBrainzUsername == null)
            {
                return Result.Fail<SyncWeeklyPlaylistDto>("Link your ListenBrainz account in Settings first.");
            }

            var credentials = await navidromeCredentialHelper.GetCredentials(userId);
            var tracks = await weeklyExplorationService.GetWeeklyExploration(user.ListenBrainzUsername);
            var songIds = new List<string>();

            foreach (var track in tracks)
            {
                var normalisedTitle = track.Title.NormaliseTitle();
                var normalisedArtist = track.ArtistName.NormaliseArtistName();

                var results = await navidromeApiClient.Search(credentials, $"{track.ArtistName} {track.Title}", 0, 0, 5);
                var song = (results.Song ?? []).FirstOrDefault(s =>
                    s.Title.NormaliseTitle() == normalisedTitle && s.Artist.NormaliseArtistName() == normalisedArtist);

                if (song != null && !songIds.Contains(song.Id))
                {
                    songIds.Add(song.Id);
                }
            }

            if (songIds.Count == 0)
            {
                return Result.Ok(new SyncWeeklyPlaylistDto { PlaylistId = user.DiscoverPlaylistId, MatchedSongs = 0, TotalTracks = tracks.Length });
            }

            user.DiscoverPlaylistId = await WritePlaylist(credentials, user.DiscoverPlaylistId, songIds.ToArray());
            await context.SaveChangesAsync();

            return Result.Ok(new SyncWeeklyPlaylistDto
            {
                PlaylistId = user.DiscoverPlaylistId,
                MatchedSongs = songIds.Count,
                TotalTracks = tracks.Length
            });
        }

        private async Task<string> WritePlaylist(NavidromeCredentials credentials, string? existingPlaylistId, string[] songIds)
        {
            // If they deleted it in Navidrome, ReplacePlaylistSongs returns false and a new one is made
            if (existingPlaylistId != null && await navidromeApiClient.ReplacePlaylistSongs(credentials, existingPlaylistId, songIds))
            {
                return existingPlaylistId;
            }

            var playlist = await navidromeApiClient.CreatePlaylist(credentials, PlaylistName, songIds);
            await navidromeApiClient.UpdatePlaylist(credentials, playlist.Id, [], [], isPublic: false);

            return playlist.Id;
        }
    }
}
