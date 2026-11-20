using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rabbithole.Core.Extensions;
using Rabbithole.Domain.DTOs.Artists.Responses;
using Rabbithole.Domain.Interfaces.Api.Artists;

namespace Rabbithole.Core.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class ArtistsController(IArtistsService artistsService) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<SearchArtistsDto>> SearchArtists([FromQuery] string query)
        {
            var result = await artistsService.SearchArtists(query);
            return result.ToActionResult();
        }

        [HttpGet]
        public async Task<ActionResult<GetArtistDto>> GetArtist([FromQuery] long deezerArtistId)
        {
            var result = await artistsService.GetArtist(deezerArtistId);
            return result.ToActionResult();
        }

        [HttpGet]
        public async Task<ActionResult<GetSimilarArtistsDto>> GetSimilarArtists([FromQuery] long deezerArtistId)
        {
            var result = await artistsService.GetSimilarArtists(deezerArtistId);
            return result.ToActionResult();
        }
    }
}
