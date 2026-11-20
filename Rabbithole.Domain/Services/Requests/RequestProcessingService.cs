using Microsoft.EntityFrameworkCore;
using Rabbithole.Domain.Database.Context;
using Rabbithole.Domain.Database.Models;
using Rabbithole.Domain.DTOs.ThirdParty.Lidarr;
using Rabbithole.Domain.Enums;
using Rabbithole.Domain.Extensions;
using Rabbithole.Domain.Interfaces.Api.Requests;
using Rabbithole.Domain.Interfaces.Helpers;
using Rabbithole.Domain.Interfaces.ThirdParty;
using Rabbithole.Domain.Results;
using Serilog;

namespace Rabbithole.Domain.Services.Requests
{
    // Moves album requests through Lidarr:
    //   Pending          → make sure the artist is in Lidarr (added unmonitored-by-default)
    //   WaitingForLidarr → find the album once Lidarr has loaded the discography, monitor it, search
    //   Searching        → wait for files to appear
    //   Available        → add any songs people queued for playlists
    // Runs from the scheduled job and straight after a request is made. Safe to run repeatedly.
    public class RequestProcessingService(
        AppDbContext context,
        ILidarrApiClient lidarrApiClient,
        INavidromeApiClient navidromeApiClient,
        INavidromeCredentialHelper navidromeCredentialHelper,
        ILibraryStateHelper libraryStateHelper) : IRequestProcessingService
    {
        private static readonly TimeSpan _albumLookupTimeout = TimeSpan.FromDays(1);
        private static readonly TimeSpan _playlistAddTimeout = TimeSpan.FromDays(3);

        public async Task ProcessAllRequests()
        {
            var activeRequestIds = await context.AlbumRequests
                .Where(r => r.Status == RequestStatus.Pending || r.Status == RequestStatus.WaitingForLidarr || r.Status == RequestStatus.Searching
                    || (r.Status == RequestStatus.Available && r.PendingPlaylistTracks.Any(p => p.AddedAt == null && p.GaveUpAt == null)))
                .Select(r => r.Id)
                .ToArrayAsync();

            foreach (var requestId in activeRequestIds)
            {
                try
                {
                    var result = await ProcessRequest(requestId);

                    if (!result.Success)
                    {
                        Log.Warning("[Requests] Request {RequestId} skipped: {Error}", requestId, result.Error);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "[Requests] Processing request {RequestId} failed", requestId);
                }
            }
        }

        public async Task<Result> ProcessRequest(int albumRequestId)
        {
            var request = await context.AlbumRequests.FindAsync(albumRequestId);

            if (request == null)
            {
                return Result.NotFoundFail("That request doesn't exist.");
            }

            if (!lidarrApiClient.IsConfigured())
            {
                SetStatus(request, request.Status, "Waiting for an admin to set up Lidarr.");
                await context.SaveChangesAsync();
                return Result.Ok();
            }

            if (request.Status == RequestStatus.Pending)
            {
                await EnsureArtistInLidarr(request);
            }

            if (request.Status == RequestStatus.WaitingForLidarr)
            {
                await MonitorAndSearchAlbum(request);
            }

            if (request.Status == RequestStatus.Searching)
            {
                await CheckDownloaded(request);
            }

            if (request.Status == RequestStatus.Available)
            {
                await AddPendingPlaylistTracks(request);
            }

            await context.SaveChangesAsync();

            return Result.Ok();
        }

        private async Task EnsureArtistInLidarr(AlbumRequest request)
        {
            var normalisedName = request.ArtistName.NormaliseArtistName();
            var existing = (await lidarrApiClient.GetArtists()).FirstOrDefault(a => a.ArtistName.NormaliseArtistName() == normalisedName);

            if (existing != null)
            {
                request.LidarrArtistId = existing.Id;
                SetStatus(request, RequestStatus.WaitingForLidarr, "Artist is in Lidarr, looking for the album.");
                return;
            }

            var lookups = await lidarrApiClient.LookupArtists(request.ArtistName);
            var match = lookups.FirstOrDefault(l => l.Artist.ArtistName.NormaliseArtistName() == normalisedName) ?? lookups.FirstOrDefault();

            if (match == null)
            {
                SetStatus(request, RequestStatus.Failed, "Lidarr couldn't find this artist on MusicBrainz.");
                return;
            }

            var defaults = await GetLidarrDefaults();

            if (defaults == null)
            {
                SetStatus(request, RequestStatus.Failed, "Lidarr needs a quality profile, metadata profile and root folder set up first.");
                return;
            }

            var added = await lidarrApiClient.AddArtist(match, defaults.Value.QualityProfileId, defaults.Value.MetadataProfileId, defaults.Value.RootFolderPath);
            libraryStateHelper.ClearLidarrCache();

            request.LidarrArtistId = added.Id;
            SetStatus(request, RequestStatus.WaitingForLidarr, "Added the artist to Lidarr, waiting for it to load their albums.");

            Log.Information("[Requests] Added {Artist} to Lidarr for request {RequestId}", request.ArtistName, request.Id);
        }

        private async Task MonitorAndSearchAlbum(AlbumRequest request)
        {
            var albums = await lidarrApiClient.GetAlbums(request.LidarrArtistId!.Value);
            var album = FindAlbum(albums, request.AlbumTitle);

            if (album == null)
            {
                // Freshly added artists take a few minutes to load their discography
                if (DateTime.UtcNow - request.RequestedAt > _albumLookupTimeout)
                {
                    SetStatus(request, RequestStatus.Failed, "Lidarr doesn't list this album. It may be missing from MusicBrainz.");
                }

                return;
            }

            request.LidarrAlbumId = album.Id;

            if (album.Statistics?.TrackFileCount > 0)
            {
                MarkAvailable(request);
                return;
            }

            await lidarrApiClient.MonitorAlbums([album.Id]);
            await lidarrApiClient.SearchAlbums([album.Id]);

            SetStatus(request, RequestStatus.Searching, "Lidarr is searching for the album.");
        }

        private async Task CheckDownloaded(AlbumRequest request)
        {
            var album = await lidarrApiClient.GetAlbum(request.LidarrAlbumId!.Value);

            if (album == null)
            {
                // Removed from Lidarr after we asked for it
                SetStatus(request, RequestStatus.Failed, "The album was removed from Lidarr.");
                return;
            }

            if (album.Statistics?.TrackFileCount > 0)
            {
                MarkAvailable(request);
            }
        }

        private async Task AddPendingPlaylistTracks(AlbumRequest request)
        {
            var pendingTracks = request.PendingPlaylistTracks
                .Where(p => p.AddedAt == null && p.GaveUpAt == null)
                .ToArray();

            if (pendingTracks.Length == 0)
            {
                return;
            }

            var credentials = await navidromeCredentialHelper.GetCredentials(request.UserId);

            foreach (var pending in pendingTracks)
            {
                var normalisedTitle = pending.TrackTitle.NormaliseTitle();
                var normalisedArtist = pending.ArtistName.NormaliseArtistName();

                var results = await navidromeApiClient.Search(credentials, $"{pending.ArtistName} {pending.TrackTitle}", 0, 0, 10);
                var song = (results.Song ?? []).FirstOrDefault(s =>
                    s.Title.NormaliseTitle() == normalisedTitle && s.Artist.NormaliseArtistName() == normalisedArtist);

                if (song != null)
                {
                    await navidromeApiClient.UpdatePlaylist(credentials, pending.NavidromePlaylistId, [song.Id], []);
                    pending.AddedAt = DateTime.UtcNow;
                    Log.Information("[Requests] Added {Track} to playlist {PlaylistId}", pending.TrackTitle, pending.NavidromePlaylistId);
                    continue;
                }

                // Navidrome may not have scanned the new files yet; give it a few days
                if (request.CompletedAt != null && DateTime.UtcNow - request.CompletedAt > _playlistAddTimeout)
                {
                    pending.GaveUpAt = DateTime.UtcNow;
                }
            }
        }

        private async Task<(int QualityProfileId, int MetadataProfileId, string RootFolderPath)?> GetLidarrDefaults()
        {
            var qualityProfile = (await lidarrApiClient.GetQualityProfiles()).FirstOrDefault();
            var metadataProfile = (await lidarrApiClient.GetMetadataProfiles()).FirstOrDefault();
            var rootFolder = (await lidarrApiClient.GetRootFolders()).FirstOrDefault();

            if (qualityProfile == null || metadataProfile == null || rootFolder == null)
            {
                return null;
            }

            return (qualityProfile.Id, metadataProfile.Id, rootFolder.Path);
        }

        private static LidarrAlbum? FindAlbum(LidarrAlbum[] albums, string albumTitle)
        {
            var normalisedTitle = albumTitle.NormaliseTitle();

            return albums.FirstOrDefault(a => a.Title.NormaliseTitle() == normalisedTitle)
                ?? albums.FirstOrDefault(a =>
                {
                    var candidate = a.Title.NormaliseTitle();
                    return candidate.Length > 0 && normalisedTitle.Length > 0
                        && (candidate.Contains(normalisedTitle) || normalisedTitle.Contains(candidate));
                });
        }

        private static void MarkAvailable(AlbumRequest request)
        {
            request.CompletedAt = DateTime.UtcNow;
            SetStatus(request, RequestStatus.Available, "Downloaded. It'll show in Navidrome after its next scan.");
        }

        private static void SetStatus(AlbumRequest request, RequestStatus status, string? message)
        {
            request.Status = status;
            request.StatusMessage = message;
            request.UpdatedAt = DateTime.UtcNow;
        }
    }
}
