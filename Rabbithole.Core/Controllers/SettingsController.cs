using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rabbithole.Core.Extensions;
using Rabbithole.Domain.DTOs.Feedback.Responses;
using Rabbithole.Domain.DTOs.Settings.Requests;
using Rabbithole.Domain.DTOs.Settings.Responses;
using Rabbithole.Domain.Helpers;
using Rabbithole.Domain.Interfaces.Api.Feedback;
using Rabbithole.Domain.Interfaces.Api.Settings;

namespace Rabbithole.Core.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class SettingsController(ISettingsService settingsService, IFeedbackService feedbackService) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<GetMySettingsDto>> GetMySettings()
        {
            var result = await settingsService.GetMySettings();
            return result.ToActionResult();
        }

        [HttpPut]
        public async Task<ActionResult> UpdateMySettings([FromBody] UpdateMySettingsRequest request)
        {
            var result = await settingsService.UpdateMySettings(request);
            return result.ToActionResult();
        }

        [HttpGet]
        public async Task<ActionResult<GetBlockedArtistsDto>> GetBlockedArtists()
        {
            var blockedArtists = await feedbackService.GetBlockedArtists();
            return Ok(blockedArtists);
        }

        [HttpGet]
        [Authorize(Roles = UserRoles.Admin)]
        public ActionResult<GetIntegrationSettingsDto> GetIntegrationSettings()
        {
            return Ok(settingsService.GetIntegrationSettings());
        }

        [HttpPut]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult> UpdateIntegrationSettings([FromBody] UpdateIntegrationSettingsRequest request)
        {
            var result = await settingsService.UpdateIntegrationSettings(request);
            return result.ToActionResult();
        }
    }
}
