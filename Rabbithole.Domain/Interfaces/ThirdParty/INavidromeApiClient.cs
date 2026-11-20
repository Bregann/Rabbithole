using Rabbithole.Domain.DTOs.ThirdParty.Navidrome;

namespace Rabbithole.Domain.Interfaces.ThirdParty
{
    public interface INavidromeApiClient
    {
        Task<bool> Ping(NavidromeCredentials credentials);
        Task<SubsonicPlaylist[]> GetPlaylists(NavidromeCredentials credentials);
        Task<SubsonicPlaylist?> GetPlaylist(NavidromeCredentials credentials, string playlistId);
        Task<SubsonicPlaylist> CreatePlaylist(NavidromeCredentials credentials, string name, string[] songIds);
        Task<bool> ReplacePlaylistSongs(NavidromeCredentials credentials, string playlistId, string[] songIds);
        Task UpdatePlaylist(NavidromeCredentials credentials, string playlistId, string[] songIdsToAdd, int[] songIndexesToRemove, bool? isPublic = null);
        Task<SubsonicSearchResult> Search(NavidromeCredentials credentials, string query, int artistCount, int albumCount, int songCount);
        Task<SubsonicArtist[]> GetArtists(NavidromeCredentials credentials);
        Task<SubsonicAlbum[]> GetFrequentAlbums(NavidromeCredentials credentials, int size);
        Task<NavidromeStream?> Stream(NavidromeCredentials credentials, string songId, string? rangeHeader);
        Task<NavidromeStream?> GetCoverArt(NavidromeCredentials credentials, string coverArtId, int size);
    }
}
