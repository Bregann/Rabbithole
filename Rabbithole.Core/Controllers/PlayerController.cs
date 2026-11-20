using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rabbithole.Core.Extensions;
using Rabbithole.Domain.DTOs.Player.Responses;
using Rabbithole.Domain.Interfaces.Api.Player;

namespace Rabbithole.Core.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class PlayerController(IPlayerService playerService) : ControllerBase
    {
        // Proxies Navidrome's stream, passing Range through so seeking works in the browser
        [HttpGet]
        public async Task<ActionResult> Stream([FromQuery] string songId)
        {
            var result = await playerService.Stream(songId, Request.Headers.Range.ToString());

            if (!result.Success)
            {
                return result.ToErrorResponse();
            }

            var stream = result.Value;

            await using (stream.Content)
            {
                Response.StatusCode = stream.StatusCode;
                Response.ContentType = stream.ContentType;
                Response.ContentLength = stream.ContentLength;

                if (stream.ContentRange != null)
                {
                    Response.Headers.ContentRange = stream.ContentRange;
                }

                Response.Headers.AcceptRanges = stream.AcceptRanges ?? "bytes";

                await stream.Content.CopyToAsync(Response.Body, HttpContext.RequestAborted);
            }

            return new EmptyResult();
        }

        [HttpGet]
        [ResponseCache(Duration = 86400, Location = ResponseCacheLocation.Client)]
        public async Task<ActionResult> CoverArt([FromQuery] string coverArtId, [FromQuery] int size = 300)
        {
            var result = await playerService.GetCoverArt(coverArtId, size);

            return result.Success
                ? File(result.Value.Content, result.Value.ContentType)
                : result.ToErrorResponse();
        }

        [HttpGet]
        public async Task<ActionResult<GetPreviewDto>> GetPreview([FromQuery] long deezerTrackId)
        {
            var result = await playerService.GetPreview(deezerTrackId);
            return result.ToActionResult();
        }
    }
}
