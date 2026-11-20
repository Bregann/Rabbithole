using System.Text.Json.Serialization;

namespace Rabbithole.Domain.DTOs.ThirdParty.Deezer
{
    // Shapes returned by the public Deezer API (no key needed). Errors come back as 200 with an "error" object.
    public class DeezerList<T>
    {
        public T[]? Data { get; set; }
        public DeezerError? Error { get; set; }
    }

    public class DeezerError
    {
        public string? Type { get; set; }
        public string? Message { get; set; }
        public int Code { get; set; }
    }

    public class DeezerArtist
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("picture_medium")]
        public string? PictureMedium { get; set; }

        [JsonPropertyName("picture_xl")]
        public string? PictureXl { get; set; }

        [JsonPropertyName("nb_fan")]
        public int? FanCount { get; set; }

        public DeezerError? Error { get; set; }
    }

    public class DeezerAlbum
    {
        public long Id { get; set; }
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("cover_medium")]
        public string? CoverMedium { get; set; }

        [JsonPropertyName("cover_xl")]
        public string? CoverXl { get; set; }

        [JsonPropertyName("release_date")]
        public string? ReleaseDate { get; set; }

        [JsonPropertyName("record_type")]
        public string? RecordType { get; set; }

        public DeezerArtist? Artist { get; set; }
        public DeezerError? Error { get; set; }
    }

    public class DeezerTrack
    {
        public long Id { get; set; }
        public string Title { get; set; } = string.Empty;

        // 30-second MP3. Signed URL that expires, so never store it.
        public string? Preview { get; set; }

        public int Duration { get; set; }
        public DeezerArtist? Artist { get; set; }
        public DeezerAlbum? Album { get; set; }
        public DeezerError? Error { get; set; }
    }
}
