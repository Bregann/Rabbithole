using Microsoft.EntityFrameworkCore;
using Rabbithole.Domain.DTOs.Discover;
using Rabbithole.Domain.DTOs.ThirdParty.Navidrome;
using Rabbithole.Domain.Database.Context;
using Rabbithole.Domain.Exceptions;
using Rabbithole.Domain.Interfaces.Api.Discover;
using Rabbithole.Domain.Interfaces.ThirdParty;
using Serilog;

namespace Rabbithole.Domain.Services.Discover
{
    // Who does this user actually listen to? ListenBrainz stats first, Navidrome's most played albums as a fallback.
    public class SeedArtistsService(
        AppDbContext context,
        IListenBrainzApiClient listenBrainzApiClient,
        INavidromeApiClient navidromeApiClient) : ISeedArtistsService
    {
        public const string ListenBrainzSource = "ListenBrainz";
        public const string NavidromeSource = "Navidrome";

        public async Task<SeedArtistsResult> GetSeedArtists(int userId, NavidromeCredentials credentials, int count)
        {
            var listenBrainzUsername = await context.Users
                .Where(u => u.Id == userId)
                .Select(u => u.ListenBrainzUsername)
                .FirstOrDefaultAsync();

            if (listenBrainzUsername != null)
            {
                try
                {
                    var topArtists = await listenBrainzApiClient.GetTopArtists(listenBrainzUsername, count);

                    if (topArtists.Length > 0)
                    {
                        return new SeedArtistsResult
                        {
                            Source = ListenBrainzSource,
                            Names = topArtists.Select(a => a.ArtistName).ToArray()
                        };
                    }
                }
                catch (ExternalServiceException ex)
                {
                    Log.Warning(ex, "[Discover] ListenBrainz stats unavailable for {Username}, falling back to Navidrome", listenBrainzUsername);
                }
            }

            var frequentAlbums = await navidromeApiClient.GetFrequentAlbums(credentials, count * 3);

            return new SeedArtistsResult
            {
                Source = NavidromeSource,
                Names = frequentAlbums
                    .Select(a => a.Artist)
                    .OfType<string>()
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Take(count)
                    .ToArray()
            };
        }
    }
}
