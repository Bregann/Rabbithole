using Rabbithole.Domain.DTOs.Discover;
using Rabbithole.Domain.DTOs.Library;

namespace Rabbithole.Domain.Interfaces.Api.Discover
{
    public interface IRelatedArtistsService
    {
        Task<RelatedArtistResult[]> GetRelatedArtists(string[] seedArtistNames, LibrarySnapshot snapshot, int perSeed, int take, bool includeLibraryArtists);
    }
}
