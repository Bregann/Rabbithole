using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rabbithole.Core.Extensions;
using Rabbithole.Domain.DTOs.Requests.Requests;
using Rabbithole.Domain.DTOs.Requests.Responses;
using Rabbithole.Domain.Interfaces.Api.Requests;

namespace Rabbithole.Core.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class RequestsController(IRequestsService requestsService) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<GetRequestsDto>> GetMyRequests()
        {
            var requests = await requestsService.GetMyRequests();
            return Ok(requests);
        }

        [HttpPost]
        public async Task<ActionResult<CreateRequestDto>> RequestAlbum([FromBody] RequestAlbumRequest request)
        {
            var result = await requestsService.RequestAlbum(request);
            return result.ToActionResult();
        }

        [HttpPost]
        public async Task<ActionResult<CreateRequestDto>> RequestTrack([FromBody] RequestTrackRequest request)
        {
            var result = await requestsService.RequestTrack(request);
            return result.ToActionResult();
        }
    }
}
