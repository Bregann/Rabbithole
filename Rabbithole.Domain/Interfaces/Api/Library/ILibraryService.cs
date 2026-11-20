using Rabbithole.Domain.DTOs.Library.Responses;
using Rabbithole.Domain.Results;

namespace Rabbithole.Domain.Interfaces.Api.Library
{
    public interface ILibraryService
    {
        Task<Result<SearchLibraryDto>> SearchSongs(string query);
    }
}
