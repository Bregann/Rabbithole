using Rabbithole.Domain.Enums;

namespace Rabbithole.Domain.DTOs.Shared
{
    public class SuggestedTrackItem
    {
        public required TrackItem Track { get; set; }
        public required LibraryState LibraryState { get; set; }
        public required string[] BecauseOf { get; set; } = [];
    }
}
