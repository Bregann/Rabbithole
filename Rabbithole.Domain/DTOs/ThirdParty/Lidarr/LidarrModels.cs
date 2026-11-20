using System.Text.Json.Nodes;

namespace Rabbithole.Domain.DTOs.ThirdParty.Lidarr
{
    public class LidarrArtist
    {
        public int Id { get; set; }
        public string ArtistName { get; set; } = string.Empty;
        public string? ForeignArtistId { get; set; }
        public bool Monitored { get; set; }
        public LidarrStatistics? Statistics { get; set; }
    }

    // Lidarr wants the full lookup object posted back when adding an artist, so keep the raw JSON alongside
    public class LidarrArtistLookup
    {
        public required LidarrArtist Artist { get; set; }
        public required JsonObject Raw { get; set; }
    }

    public class LidarrAlbum
    {
        public int Id { get; set; }
        public int ArtistId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? ForeignAlbumId { get; set; }
        public bool Monitored { get; set; }
        public LidarrStatistics? Statistics { get; set; }
    }

    public class LidarrStatistics
    {
        public int TrackFileCount { get; set; }
        public int TrackCount { get; set; }
        public double PercentOfTracks { get; set; }
    }

    public class LidarrProfile
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }

    public class LidarrRootFolder
    {
        public int Id { get; set; }
        public string Path { get; set; } = string.Empty;
    }
}
