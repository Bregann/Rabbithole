using Rabbithole.Domain.DTOs.Shared;
using Rabbithole.Domain.Enums;

namespace Rabbithole.Domain.DTOs.Artists.Responses
{
    public class GetArtistDto
    {
        public required ArtistSummaryItem Artist { get; set; }
        public required TrackItem[] TopTracks { get; set; } = [];
        public required AlbumItem[] Albums { get; set; } = [];
        public required ArtistSummaryItem[] SimilarArtists { get; set; } = [];
    }

    public class AlbumItem
    {
        public required long DeezerAlbumId { get; set; }
        public required string Title { get; set; }
        public string? CoverUrl { get; set; }
        public int? ReleaseYear { get; set; }
        public string? RecordType { get; set; }
        public RequestStatus? RequestStatus { get; set; }
    }
}
