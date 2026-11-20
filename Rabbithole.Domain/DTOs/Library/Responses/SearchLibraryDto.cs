using Rabbithole.Domain.DTOs.Shared;

namespace Rabbithole.Domain.DTOs.Library.Responses
{
    public class SearchLibraryDto
    {
        public required LibrarySongItem[] Songs { get; set; } = [];
    }
}
