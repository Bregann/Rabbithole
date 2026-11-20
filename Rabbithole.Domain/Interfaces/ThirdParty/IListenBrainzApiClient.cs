using Rabbithole.Domain.DTOs.ThirdParty.ListenBrainz;

namespace Rabbithole.Domain.Interfaces.ThirdParty
{
    public interface IListenBrainzApiClient
    {
        Task<bool> UserExists(string username);
        Task<ListenBrainzArtistStat[]> GetTopArtists(string username, int count);
        Task<ListenBrainzPlaylist[]> GetCreatedForPlaylists(string username);
        Task<ListenBrainzPlaylist?> GetPlaylist(string playlistMbid);
    }
}
