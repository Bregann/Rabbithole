using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rabbithole.Core.Extensions;
using Rabbithole.Domain.DTOs.Auth;
using Rabbithole.Domain.DTOs.Auth.Requests;
using Rabbithole.Domain.DTOs.Auth.Responses;
using Rabbithole.Domain.Helpers;
using Rabbithole.Domain.Interfaces.Api.Auth;

namespace Rabbithole.Core.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        private const string AccessTokenCookie = "accessToken";
        private const string RefreshTokenCookie = "refreshToken";
        private const string RefreshTokenPath = "/api/auth";

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<GetCurrentUserDto>> Login([FromBody] LoginRequest request)
        {
            var result = await authService.Login(request);

            if (!result.Success)
            {
                return result.ToErrorResponse();
            }

            SetTokenCookies(result.Value);
            return Ok(result.Value.User);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> RefreshToken()
        {
            var result = await authService.RefreshToken(Request.Cookies[RefreshTokenCookie]);

            if (!result.Success)
            {
                return result.ToErrorResponse();
            }

            SetTokenCookies(result.Value);
            return Ok();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> Logout()
        {
            await authService.Logout(Request.Cookies[RefreshTokenCookie]);

            Response.Cookies.Delete(AccessTokenCookie);
            Response.Cookies.Delete(RefreshTokenCookie, new CookieOptions { Path = RefreshTokenPath });

            return Ok();
        }

        [HttpGet]
        public async Task<ActionResult<GetCurrentUserDto>> GetCurrentUser()
        {
            var result = await authService.GetCurrentUser();
            return result.ToActionResult();
        }

        // Both tokens are HttpOnly: the SPA never touches them, it just calls GetCurrentUser
        private void SetTokenCookies(LoginResult tokens)
        {
            var expires = DateTimeOffset.UtcNow.Add(JwtTokenHelper.RefreshTokenLifetime);

            Response.Cookies.Append(AccessTokenCookie, tokens.AccessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Expires = expires
            });

            Response.Cookies.Append(RefreshTokenCookie, tokens.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Strict,
                Path = RefreshTokenPath,
                Expires = expires
            });
        }
    }
}
