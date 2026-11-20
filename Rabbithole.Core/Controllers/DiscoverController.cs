using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rabbithole.Core.Extensions;
using Rabbithole.Domain.DTOs.Discover.Responses;
using Rabbithole.Domain.Interfaces.Api.Discover;
using Rabbithole.Domain.Interfaces.Api.Sync;
using Rabbithole.Domain.Interfaces.Helpers;

namespace Rabbithole.Core.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class DiscoverController(
        IDiscoverFeedService discoverFeedService,
        IWeeklyPicksService weeklyPicksService,
        IWeeklyDiscoverySyncService weeklyDiscoverySyncService,
        IUserContextHelper userContextHelper) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<GetDiscoverFeedDto>> GetFeed([FromQuery] bool refresh = false)
        {
            var feed = await discoverFeedService.GetFeed(refresh);
            return Ok(feed);
        }

        [HttpGet]
        public async Task<ActionResult<GetWeeklyPicksDto>> GetWeeklyPicks()
        {
            var picks = await weeklyPicksService.GetWeeklyPicks();
            return Ok(picks);
        }

        [HttpPost]
        public async Task<ActionResult<SyncWeeklyPlaylistDto>> SyncMyWeeklyPlaylist()
        {
            var userId = userContextHelper.GetUserId();
            var result = await weeklyDiscoverySyncService.RunForUser(userId);

            return result.ToActionResult();
        }
    }
}
