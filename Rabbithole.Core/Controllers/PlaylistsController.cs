using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rabbithole.Core.Extensions;
using Rabbithole.Domain.DTOs.Playlists.Requests;
using Rabbithole.Domain.DTOs.Playlists.Responses;
using Rabbithole.Domain.Interfaces.Api.Playlists;

namespace Rabbithole.Core.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class PlaylistsController(IPlaylistsService playlistsService, IPlaylistSuggestionsService playlistSuggestionsService) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<GetPlaylistsDto>> GetPlaylists()
        {
            var playlists = await playlistsService.GetPlaylists();
            return Ok(playlists);
        }

        [HttpGet]
        public async Task<ActionResult<GetPlaylistDto>> GetPlaylist([FromQuery] string playlistId)
        {
            var result = await playlistsService.GetPlaylist(playlistId);
            return result.ToActionResult();
        }

        [HttpGet]
        public async Task<ActionResult<GetPlaylistSuggestionsDto>> GetPlaylistSuggestions([FromQuery] string playlistId)
        {
            var result = await playlistSuggestionsService.GetPlaylistSuggestions(playlistId);
            return result.ToActionResult();
        }

        [HttpPost]
        public async Task<ActionResult<string>> CreatePlaylist([FromBody] CreatePlaylistRequest request)
        {
            var result = await playlistsService.CreatePlaylist(request);
            return result.ToActionResult();
        }

        [HttpPost]
        public async Task<ActionResult> AddSongsToPlaylist([FromBody] AddSongsToPlaylistRequest request)
        {
            var result = await playlistsService.AddSongsToPlaylist(request);
            return result.ToActionResult();
        }

        [HttpPost]
        public async Task<ActionResult> RemoveSongFromPlaylist([FromBody] RemoveSongFromPlaylistRequest request)
        {
            var result = await playlistsService.RemoveSongFromPlaylist(request);
            return result.ToActionResult();
        }
    }
}
