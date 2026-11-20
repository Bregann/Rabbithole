using Rabbithole.Domain.DTOs.Playlists.Responses;
using Rabbithole.Domain.Results;

namespace Rabbithole.Domain.Interfaces.Api.Playlists
{
    public interface IPlaylistSuggestionsService
    {
        Task<Result<GetPlaylistSuggestionsDto>> GetPlaylistSuggestions(string playlistId);
    }
}
