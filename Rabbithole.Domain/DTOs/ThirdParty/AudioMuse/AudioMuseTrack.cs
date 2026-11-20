using System.Text.Json.Serialization;

namespace Rabbithole.Domain.DTOs.ThirdParty.AudioMuse
{
    // item_id is the media server's own song id (Navidrome), so results can go straight into playlists
    public class AudioMuseTrack
    {
        [JsonPropertyName("item_id")]
        public string ItemId { get; set; } = string.Empty;

        public string? Title { get; set; }
        public string? Author { get; set; }
        public string? Album { get; set; }
        public double? Distance { get; set; }
    }
}
