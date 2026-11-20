using Rabbithole.Domain.DTOs.Library.Responses;
using Rabbithole.Domain.Extensions;
using Rabbithole.Domain.Interfaces.Api.Library;
using Rabbithole.Domain.Interfaces.Helpers;
using Rabbithole.Domain.Interfaces.ThirdParty;
using Rabbithole.Domain.Results;

namespace Rabbithole.Domain.Services.Library
{
    public class LibraryService(ICurrentUserHelper currentUserHelper, INavidromeApiClient navidromeApiClient) : ILibraryService
    {
        private const int MaxSongs = 40;

        public async Task<Result<SearchLibraryDto>> SearchSongs(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Result.Fail<SearchLibraryDto>("Type something to search your library.");
            }

            var credentials = await currentUserHelper.GetCredentials();
            var results = await navidromeApiClient.Search(credentials, query.Trim(), 0, 0, MaxSongs);

            return Result.Ok(new SearchLibraryDto
            {
                Songs = (results.Song ?? []).Select(s => s.ToLibrarySongItem()).ToArray()
            });
        }
    }
}
