using Rabbithole.Domain.DTOs.Discover;
using Rabbithole.Domain.DTOs.ThirdParty.Navidrome;

namespace Rabbithole.Domain.Interfaces.Api.Discover
{
    public interface ISeedArtistsService
    {
        Task<SeedArtistsResult> GetSeedArtists(int userId, NavidromeCredentials credentials, int count);
    }
}
