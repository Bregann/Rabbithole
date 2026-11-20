using Rabbithole.Domain.DTOs.Artists.Responses;
using Rabbithole.Domain.Results;

namespace Rabbithole.Domain.Interfaces.Api.Artists
{
    public interface IArtistsService
    {
        Task<Result<SearchArtistsDto>> SearchArtists(string query);
        Task<Result<GetArtistDto>> GetArtist(long deezerArtistId);
        Task<Result<GetSimilarArtistsDto>> GetSimilarArtists(long deezerArtistId);
    }
}
