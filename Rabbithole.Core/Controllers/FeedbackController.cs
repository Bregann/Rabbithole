using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rabbithole.Core.Extensions;
using Rabbithole.Domain.DTOs.Feedback.Requests;
using Rabbithole.Domain.Interfaces.Api.Feedback;

namespace Rabbithole.Core.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class FeedbackController(IFeedbackService feedbackService) : ControllerBase
    {
        [HttpPost]
        public async Task<ActionResult<int>> BlockArtist([FromBody] BlockArtistRequest request)
        {
            var result = await feedbackService.BlockArtist(request);
            return result.ToActionResult();
        }

        [HttpDelete]
        public async Task<ActionResult> UnblockArtist([FromQuery] int blockedArtistId)
        {
            var result = await feedbackService.UnblockArtist(blockedArtistId);
            return result.ToActionResult();
        }
    }
}
