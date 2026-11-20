namespace Rabbithole.Domain.DTOs.Shared
{
    public class LibrarySongItem
    {
        public required string SongId { get; set; }
        public required string Title { get; set; }
        public string? ArtistName { get; set; }
        public string? AlbumTitle { get; set; }
        public int? DurationSeconds { get; set; }
        public string? CoverArtId { get; set; }
    }
}
