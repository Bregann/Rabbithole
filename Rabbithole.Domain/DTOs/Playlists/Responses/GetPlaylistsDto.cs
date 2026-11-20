namespace Rabbithole.Domain.DTOs.Playlists.Responses
{
    public class GetPlaylistsDto
    {
        public required PlaylistItem[] Playlists { get; set; } = [];
    }

    public class PlaylistItem
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public required int SongCount { get; set; }
        public required int DurationSeconds { get; set; }
        public string? CoverArtId { get; set; }
        public required bool IsPublic { get; set; }
        public string? Owner { get; set; }
        public required bool IsOwnedByMe { get; set; }
        public required bool IsRabbitholeWeekly { get; set; }
    }
}
