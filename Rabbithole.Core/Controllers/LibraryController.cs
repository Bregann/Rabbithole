using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rabbithole.Core.Extensions;
using Rabbithole.Domain.DTOs.Library.Responses;
using Rabbithole.Domain.Interfaces.Api.Library;

namespace Rabbithole.Core.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class LibraryController(ILibraryService libraryService) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<SearchLibraryDto>> SearchSongs([FromQuery] string query)
        {
            var result = await libraryService.SearchSongs(query);
            return result.ToActionResult();
        }
    }
}
