using Rabbithole.Domain.DTOs.Shared;

namespace Rabbithole.Domain.DTOs.Artists.Responses
{
    public class GetSimilarArtistsDto
    {
        public required ArtistSummaryItem Artist { get; set; }
        public required ArtistSummaryItem[] SimilarArtists { get; set; } = [];
    }
}
