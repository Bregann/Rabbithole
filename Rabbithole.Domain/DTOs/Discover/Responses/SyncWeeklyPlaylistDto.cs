namespace Rabbithole.Domain.DTOs.Discover.Responses
{
    public class SyncWeeklyPlaylistDto
    {
        public string? PlaylistId { get; set; }
        public required int MatchedSongs { get; set; }
        public required int TotalTracks { get; set; }
    }
}
