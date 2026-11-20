using Microsoft.Extensions.Caching.Memory;
using Rabbithole.Domain.DTOs.Discover.Responses;
using Rabbithole.Domain.Interfaces.Api.Discover;
using Rabbithole.Domain.Interfaces.Helpers;

namespace Rabbithole.Domain.Services.Discover
{
    public class DiscoverFeedService(
        ICurrentUserHelper currentUserHelper,
        ISeedArtistsService seedArtistsService,
        IRelatedArtistsService relatedArtistsService,
        IMemoryCache memoryCache) : IDiscoverFeedService
    {
        private const int SeedCount = 8;
        private const int RelatedPerSeed = 15;
        private const int FeedSize = 36;
        private static readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(30);

        public static string GetCacheKey(int userId)
        {
            return $"discover:feed:{userId}";
        }

        public async Task<GetDiscoverFeedDto> GetFeed(bool refresh)
        {
            var userId = currentUserHelper.GetUserId();
            var cacheKey = GetCacheKey(userId);

            if (!refresh && memoryCache.TryGetValue(cacheKey, out GetDiscoverFeedDto? cached) && cached != null)
            {
                return cached;
            }

            var credentials = await currentUserHelper.GetCredentials();
            var snapshot = await currentUserHelper.GetLibrarySnapshot();
            var seeds = await seedArtistsService.GetSeedArtists(userId, credentials, SeedCount);

            var related = await relatedArtistsService.GetRelatedArtists(seeds.Names, snapshot, RelatedPerSeed, FeedSize, includeLibraryArtists: false);

            var feed = new GetDiscoverFeedDto
            {
                SeedSource = seeds.Source,
                Seeds = seeds.Names,
                Artists = related
                    .Select(r => new DiscoverArtistItem { Artist = r.Artist, BecauseOf = r.BecauseOf })
                    .ToArray()
            };

            memoryCache.Set(cacheKey, feed, _cacheDuration);

            return feed;
        }
    }
}
