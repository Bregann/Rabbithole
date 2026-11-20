using Rabbithole.Domain.Enums;

namespace Rabbithole.Domain.DTOs.Shared
{
    public class ArtistSummaryItem
    {
        public required long DeezerArtistId { get; set; }
        public required string Name { get; set; }
        public string? ImageUrl { get; set; }
        public int? FanCount { get; set; }
        public required LibraryState LibraryState { get; set; }
        public required bool IsBlocked { get; set; }
    }
}
