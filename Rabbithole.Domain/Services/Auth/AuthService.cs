using Microsoft.EntityFrameworkCore;
using Rabbithole.Domain.DTOs.Auth;
using Rabbithole.Domain.DTOs.Auth.Requests;
using Rabbithole.Domain.DTOs.Auth.Responses;
using Rabbithole.Domain.DTOs.ThirdParty.Navidrome;
using Rabbithole.Domain.Database.Context;
using Rabbithole.Domain.Database.Models;
using Rabbithole.Domain.Helpers;
using Rabbithole.Domain.Interfaces.Api.Auth;
using Rabbithole.Domain.Interfaces.Helpers;
using Rabbithole.Domain.Interfaces.ThirdParty;
using Rabbithole.Domain.Results;
using Serilog;

namespace Rabbithole.Domain.Services.Auth
{
    // Users sign in with their Navidrome account. There's no separate registration:
    // a successful Navidrome sign-in creates the Rabbithole user, and the very first user becomes admin.
    public class AuthService(
        AppDbContext context,
        INavidromeApiClient navidromeApiClient,
        ICredentialProtector credentialProtector,
        IJwtTokenHelper jwtTokenHelper,
        IUserContextHelper userContextHelper) : IAuthService
    {
        public async Task<Result<LoginResult>> Login(LoginRequest request)
        {
            var username = request.Username.Trim();

            if (username.Length == 0 || string.IsNullOrEmpty(request.Password))
            {
                return Result.Fail<LoginResult>("Enter your Navidrome username and password.");
            }

            var credentials = new NavidromeCredentials { Username = username, Password = request.Password };

            if (!await navidromeApiClient.Ping(credentials))
            {
                return Result.UnauthorizedFail<LoginResult>("Wrong Navidrome username or password.");
            }

            var now = DateTime.UtcNow;
            var user = await context.Users.FirstOrDefaultAsync(u => u.NavidromeUsername == username);

            if (user == null)
            {
                var isFirstUser = !await context.Users.AnyAsync();

                user = new User
                {
                    NavidromeUsername = username,
                    EncryptedNavidromePassword = credentialProtector.Protect(request.Password),
                    IsAdmin = isFirstUser,
                    CreatedAt = now,
                    LastLoginAt = now
                };

                await context.Users.AddAsync(user);
                Log.Information("[Auth] New user {Username} (admin: {IsAdmin})", username, isFirstUser);
            }
            else
            {
                // Keep the stored password in step with Navidrome in case it changed
                user.EncryptedNavidromePassword = credentialProtector.Protect(request.Password);
                user.LastLoginAt = now;
            }

            await context.SaveChangesAsync();

            return Result.Ok(await IssueTokens(user));
        }

        public async Task<Result<LoginResult>> RefreshToken(string? refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return Result.UnauthorizedFail<LoginResult>("Your session has expired. Sign in again.");
            }

            var tokenHash = jwtTokenHelper.HashToken(refreshToken);
            var storedToken = await context.UserRefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash);

            if (storedToken == null || storedToken.ExpiresAt < DateTime.UtcNow)
            {
                return Result.UnauthorizedFail<LoginResult>("Your session has expired. Sign in again.");
            }

            // Rotate: every refresh token works once
            var user = storedToken.User;
            context.UserRefreshTokens.Remove(storedToken);

            return Result.Ok(await IssueTokens(user));
        }

        public async Task Logout(string? refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return;
            }

            var tokenHash = jwtTokenHelper.HashToken(refreshToken);

            await context.UserRefreshTokens
                .Where(t => t.TokenHash == tokenHash)
                .ExecuteDeleteAsync();
        }

        public async Task<Result<GetCurrentUserDto>> GetCurrentUser()
        {
            var userId = userContextHelper.GetUserId();

            var user = await context.Users
                .Where(u => u.Id == userId)
                .Select(u => new GetCurrentUserDto
                {
                    Id = u.Id,
                    Username = u.NavidromeUsername,
                    IsAdmin = u.IsAdmin,
                    ListenBrainzUsername = u.ListenBrainzUsername
                })
                .FirstOrDefaultAsync();

            return user == null
                ? Result.UnauthorizedFail<GetCurrentUserDto>("Your account no longer exists. Sign in again.")
                : Result.Ok(user);
        }

        private async Task<LoginResult> IssueTokens(User user)
        {
            var now = DateTime.UtcNow;
            var refreshToken = jwtTokenHelper.CreateRefreshToken();

            await context.UserRefreshTokens.AddAsync(new UserRefreshToken
            {
                UserId = user.Id,
                TokenHash = jwtTokenHelper.HashToken(refreshToken),
                CreatedAt = now,
                ExpiresAt = now.Add(JwtTokenHelper.RefreshTokenLifetime)
            });

            await context.SaveChangesAsync();

            await context.UserRefreshTokens
                .Where(t => t.UserId == user.Id && t.ExpiresAt < now)
                .ExecuteDeleteAsync();

            return new LoginResult
            {
                AccessToken = jwtTokenHelper.CreateAccessToken(user),
                RefreshToken = refreshToken,
                User = new GetCurrentUserDto
                {
                    Id = user.Id,
                    Username = user.NavidromeUsername,
                    IsAdmin = user.IsAdmin,
                    ListenBrainzUsername = user.ListenBrainzUsername
                }
            };
        }
    }
}
