using Rabbithole.Domain.DTOs.Shared;

namespace Rabbithole.Domain.DTOs.Discover
{
    public class RelatedArtistResult
    {
        public required ArtistSummaryItem Artist { get; set; }
        public required string[] BecauseOf { get; set; } = [];
    }
}
