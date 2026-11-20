using System.Text.Json.Serialization;

namespace Rabbithole.Domain.DTOs.ThirdParty.ListenBrainz
{
    public class ListenBrainzArtistStatsResponse
    {
        public ListenBrainzArtistStatsPayload? Payload { get; set; }
    }

    public class ListenBrainzArtistStatsPayload
    {
        public ListenBrainzArtistStat[]? Artists { get; set; }
    }

    public class ListenBrainzArtistStat
    {
        [JsonPropertyName("artist_name")]
        public string ArtistName { get; set; } = string.Empty;

        [JsonPropertyName("listen_count")]
        public int ListenCount { get; set; }
    }

    public class ListenBrainzCreatedForResponse
    {
        public ListenBrainzPlaylistWrapper[]? Playlists { get; set; }
    }

    public class ListenBrainzPlaylistWrapper
    {
        public ListenBrainzPlaylist? Playlist { get; set; }
    }

    public class ListenBrainzPlaylist
    {
        public string? Identifier { get; set; }
        public string? Title { get; set; }
        public ListenBrainzPlaylistTrack[]? Track { get; set; }
    }

    public class ListenBrainzPlaylistTrack
    {
        public string? Title { get; set; }
        public string? Creator { get; set; }
        public string? Album { get; set; }
    }
}
