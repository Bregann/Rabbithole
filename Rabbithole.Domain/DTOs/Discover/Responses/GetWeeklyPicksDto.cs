using Rabbithole.Domain.DTOs.Shared;
using Rabbithole.Domain.Enums;

namespace Rabbithole.Domain.DTOs.Discover.Responses
{
    public class GetWeeklyPicksDto
    {
        public required bool IsListenBrainzLinked { get; set; }
        public string? PlaylistTitle { get; set; }
        public required WeeklyPickItem[] Picks { get; set; } = [];
    }

    public class WeeklyPickItem
    {
        public required string Title { get; set; }
        public required string ArtistName { get; set; }
        public required LibraryState LibraryState { get; set; }
        public TrackItem? Track { get; set; }
    }
}
