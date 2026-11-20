using Microsoft.Extensions.Caching.Memory;
using Rabbithole.Domain.DTOs.Discover;
using Rabbithole.Domain.Interfaces.Api.Discover;
using Rabbithole.Domain.Interfaces.ThirdParty;

namespace Rabbithole.Domain.Services.Discover
{
    // ListenBrainz builds a "Weekly Exploration" playlist for every user each Monday from their listening history
    public class WeeklyExplorationService(IListenBrainzApiClient listenBrainzApiClient, IMemoryCache memoryCache) : IWeeklyExplorationService
    {
        private const string WeeklyExplorationTitle = "Weekly Exploration";
        private static readonly TimeSpan _cacheDuration = TimeSpan.FromHours(6);

        public async Task<WeeklyExplorationTrack[]> GetWeeklyExploration(string listenBrainzUsername)
        {
            return await memoryCache.GetOrCreateAsync($"listenbrainz:weekly:{listenBrainzUsername.ToLowerInvariant()}", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = _cacheDuration;

                var playlists = await listenBrainzApiClient.GetCreatedForPlaylists(listenBrainzUsername);

                // Newest first, so the first match is this week's
                var weekly = playlists.FirstOrDefault(p => p.Title?.Contains(WeeklyExplorationTitle, StringComparison.OrdinalIgnoreCase) == true);
                var playlistMbid = weekly?.Identifier?.TrimEnd('/').Split('/').LastOrDefault();

                if (weekly == null || string.IsNullOrWhiteSpace(playlistMbid))
                {
                    return Array.Empty<WeeklyExplorationTrack>();
                }

                var playlist = await listenBrainzApiClient.GetPlaylist(playlistMbid);

                if (playlist == null)
                {
                    return Array.Empty<WeeklyExplorationTrack>();
                }

                return (playlist.Track ?? [])
                    .Where(t => !string.IsNullOrWhiteSpace(t.Title) && !string.IsNullOrWhiteSpace(t.Creator))
                    .Select(t => new WeeklyExplorationTrack
                    {
                        Title = t.Title!,
                        ArtistName = t.Creator!,
                        PlaylistTitle = playlist.Title ?? weekly.Title ?? WeeklyExplorationTitle
                    })
                    .ToArray();
            }) ?? [];
        }
    }
}
