using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Rabbithole.Domain.DTOs.Library;
using Rabbithole.Domain.DTOs.ThirdParty.Navidrome;
using Rabbithole.Domain.Database.Context;
using Rabbithole.Domain.Exceptions;
using Rabbithole.Domain.Extensions;
using Rabbithole.Domain.Interfaces.Helpers;
using Rabbithole.Domain.Interfaces.ThirdParty;
using Serilog;

namespace Rabbithole.Domain.Helpers
{
    public class LibraryStateHelper(
        AppDbContext context,
        INavidromeApiClient navidromeApiClient,
        ILidarrApiClient lidarrApiClient,
        IMemoryCache memoryCache) : ILibraryStateHelper
    {
        private const string NavidromeArtistsCacheKey = "library:navidrome-artists";
        private const string LidarrArtistsCacheKey = "library:lidarr-artists";
        private static readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(10);

        public async Task<LibrarySnapshot> GetSnapshot(int userId, NavidromeCredentials credentials)
        {
            var libraryArtists = await memoryCache.GetOrCreateAsync(NavidromeArtistsCacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = _cacheDuration;
                var artists = await navidromeApiClient.GetArtists(credentials);
                return ToNameSet(artists.Select(a => a.Name));
            }) ?? [];

            var lidarrArtists = await GetLidarrArtistNames();

            var blockedArtists = await context.BlockedArtists
                .Where(b => b.UserId == userId)
                .Select(b => b.NormalisedName)
                .ToArrayAsync();

            return new LibrarySnapshot(libraryArtists, lidarrArtists, blockedArtists.ToHashSet());
        }

        public void ClearLidarrCache()
        {
            memoryCache.Remove(LidarrArtistsCacheKey);
        }

        private async Task<HashSet<string>> GetLidarrArtistNames()
        {
            if (!lidarrApiClient.IsConfigured())
            {
                return [];
            }

            try
            {
                return await memoryCache.GetOrCreateAsync(LidarrArtistsCacheKey, async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = _cacheDuration;
                    var artists = await lidarrApiClient.GetArtists();
                    return ToNameSet(artists.Select(a => a.ArtistName));
                }) ?? [];
            }
            catch (ExternalServiceException ex)
            {
                // Lidarr being down shouldn't break discovery; artists just won't show "In Lidarr"
                Log.Warning(ex, "[Library] Couldn't load Lidarr artists");
                return [];
            }
        }

        private static HashSet<string> ToNameSet(IEnumerable<string> names)
        {
            return names
                .Select(n => n.NormaliseArtistName())
                .Where(n => n.Length > 0)
                .ToHashSet();
        }
    }
}
