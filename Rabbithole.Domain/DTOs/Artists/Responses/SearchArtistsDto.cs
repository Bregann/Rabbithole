using Rabbithole.Domain.DTOs.Shared;

namespace Rabbithole.Domain.DTOs.Artists.Responses
{
    public class SearchArtistsDto
    {
        public required ArtistSummaryItem[] Artists { get; set; } = [];
    }
}
