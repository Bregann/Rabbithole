using Rabbithole.Domain.DTOs.ThirdParty.Lidarr;

namespace Rabbithole.Domain.Interfaces.ThirdParty
{
    public interface ILidarrApiClient
    {
        bool IsConfigured();
        Task<LidarrArtist[]> GetArtists();
        Task<LidarrArtistLookup[]> LookupArtists(string term);
        Task<LidarrArtist> AddArtist(LidarrArtistLookup lookup, int qualityProfileId, int metadataProfileId, string rootFolderPath);
        Task<LidarrProfile[]> GetQualityProfiles();
        Task<LidarrProfile[]> GetMetadataProfiles();
        Task<LidarrRootFolder[]> GetRootFolders();
        Task<LidarrAlbum[]> GetAlbums(int artistId);
        Task<LidarrAlbum?> GetAlbum(int albumId);
        Task MonitorAlbums(int[] albumIds);
        Task SearchAlbums(int[] albumIds);
    }
}
