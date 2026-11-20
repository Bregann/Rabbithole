using System.Text.Json.Serialization;

namespace Rabbithole.Domain.DTOs.ThirdParty.Navidrome
{
    // Shapes returned by Navidrome's Subsonic API (f=json). Everything is nullable: never trust upstream data.
    public class SubsonicEnvelope
    {
        [JsonPropertyName("subsonic-response")]
        public SubsonicResponse? Response { get; set; }
    }

    public class SubsonicResponse
    {
        public string? Status { get; set; }
        public SubsonicError? Error { get; set; }
        public SubsonicPlaylists? Playlists { get; set; }
        public SubsonicPlaylist? Playlist { get; set; }
        public SubsonicSearchResult? SearchResult3 { get; set; }
        public SubsonicArtistsIndex? Artists { get; set; }
        public SubsonicAlbumList? AlbumList2 { get; set; }
    }

    public class SubsonicError
    {
        public int Code { get; set; }
        public string? Message { get; set; }
    }

    public class SubsonicPlaylists
    {
        public SubsonicPlaylist[]? Playlist { get; set; }
    }

    public class SubsonicPlaylist
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Comment { get; set; }
        public string? Owner { get; set; }
        public bool Public { get; set; }
        public int SongCount { get; set; }
        public int Duration { get; set; }
        public string? CoverArt { get; set; }
        public SubsonicSong[]? Entry { get; set; }
    }

    public class SubsonicSong
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Artist { get; set; }
        public string? ArtistId { get; set; }
        public string? Album { get; set; }
        public string? AlbumId { get; set; }
        public int? Duration { get; set; }
        public string? CoverArt { get; set; }
    }

    public class SubsonicSearchResult
    {
        public SubsonicArtist[]? Artist { get; set; }
        public SubsonicAlbum[]? Album { get; set; }
        public SubsonicSong[]? Song { get; set; }
    }

    public class SubsonicArtistsIndex
    {
        public SubsonicArtistIndexEntry[]? Index { get; set; }
    }

    public class SubsonicArtistIndexEntry
    {
        public string? Name { get; set; }
        public SubsonicArtist[]? Artist { get; set; }
    }

    public class SubsonicArtist
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? MusicBrainzId { get; set; }
    }

    public class SubsonicAlbumList
    {
        public SubsonicAlbum[]? Album { get; set; }
    }

    public class SubsonicAlbum
    {
        public string Id { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string? Artist { get; set; }
        public string? ArtistId { get; set; }
        public string? CoverArt { get; set; }
    }
}
