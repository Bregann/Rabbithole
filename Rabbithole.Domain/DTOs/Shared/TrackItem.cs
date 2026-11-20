namespace Rabbithole.Domain.DTOs.Shared
{
    public class TrackItem
    {
        public required long DeezerTrackId { get; set; }
        public required string Title { get; set; }
        public required string ArtistName { get; set; }
        public long? DeezerArtistId { get; set; }
        public string? AlbumTitle { get; set; }
        public long? DeezerAlbumId { get; set; }
        public string? CoverUrl { get; set; }
        public required int DurationSeconds { get; set; }
        public required bool HasPreview { get; set; }
    }
}
