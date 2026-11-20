using Rabbithole.Domain.Enums;

namespace Rabbithole.Domain.DTOs.Requests.Responses
{
    public class GetRequestsDto
    {
        public required RequestItem[] Requests { get; set; } = [];
        public int? WeeklyLimit { get; set; }
        public int? RemainingThisWeek { get; set; }
    }

    public class RequestItem
    {
        public required int Id { get; set; }
        public required string ArtistName { get; set; }
        public required string AlbumTitle { get; set; }
        public string? CoverUrl { get; set; }
        public required RequestStatus Status { get; set; }
        public string? StatusMessage { get; set; }
        public required DateTime RequestedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public required int PendingPlaylistTracks { get; set; }
    }
}
