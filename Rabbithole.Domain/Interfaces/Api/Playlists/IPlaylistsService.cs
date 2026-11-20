using Rabbithole.Domain.DTOs.Playlists.Requests;
using Rabbithole.Domain.DTOs.Playlists.Responses;
using Rabbithole.Domain.Results;

namespace Rabbithole.Domain.Interfaces.Api.Playlists
{
    public interface IPlaylistsService
    {
        Task<GetPlaylistsDto> GetPlaylists();
        Task<Result<GetPlaylistDto>> GetPlaylist(string playlistId);
        Task<Result<string>> CreatePlaylist(CreatePlaylistRequest request);
        Task<Result> AddSongsToPlaylist(AddSongsToPlaylistRequest request);
        Task<Result> RemoveSongFromPlaylist(RemoveSongFromPlaylistRequest request);
    }
}
