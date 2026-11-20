using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rabbithole.Domain.DTOs.Status.Responses;
using Rabbithole.Domain.Interfaces.Api.Status;

namespace Rabbithole.Core.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class StatusController(IStatusService statusService) : ControllerBase
    {
        [HttpGet]
        [AllowAnonymous]
        public ActionResult<GetStatusDto> GetStatus()
        {
            return Ok(statusService.GetStatus());
        }
    }
}
