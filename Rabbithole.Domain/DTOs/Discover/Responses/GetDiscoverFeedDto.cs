using Rabbithole.Domain.DTOs.Shared;

namespace Rabbithole.Domain.DTOs.Discover.Responses
{
    public class GetDiscoverFeedDto
    {
        public required string SeedSource { get; set; }
        public required string[] Seeds { get; set; } = [];
        public required DiscoverArtistItem[] Artists { get; set; } = [];
    }

    public class DiscoverArtistItem
    {
        public required ArtistSummaryItem Artist { get; set; }
        public required string[] BecauseOf { get; set; } = [];
    }
}
