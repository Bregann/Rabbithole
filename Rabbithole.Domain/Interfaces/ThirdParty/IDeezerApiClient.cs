using Rabbithole.Domain.DTOs.ThirdParty.Deezer;

namespace Rabbithole.Domain.Interfaces.ThirdParty
{
    public interface IDeezerApiClient
    {
        Task<DeezerArtist[]> SearchArtists(string query, int limit);
        Task<DeezerArtist?> GetArtist(long artistId);
        Task<DeezerArtist[]> GetRelatedArtists(long artistId, int limit);
        Task<DeezerTrack[]> GetTopTracks(long artistId, int limit);
        Task<DeezerAlbum[]> GetArtistAlbums(long artistId, int limit);
        Task<DeezerAlbum?> GetAlbum(long albumId);
        Task<DeezerTrack?> GetTrack(long trackId);
        Task<DeezerTrack[]> SearchTracks(string artistName, string trackTitle, int limit);
    }
}
