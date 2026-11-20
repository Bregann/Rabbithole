using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Rabbithole.Domain.DTOs.Feedback.Requests;
using Rabbithole.Domain.DTOs.Feedback.Responses;
using Rabbithole.Domain.Database.Context;
using Rabbithole.Domain.Database.Models;
using Rabbithole.Domain.Extensions;
using Rabbithole.Domain.Interfaces.Api.Feedback;
using Rabbithole.Domain.Interfaces.Helpers;
using Rabbithole.Domain.Results;
using Rabbithole.Domain.Services.Discover;

namespace Rabbithole.Domain.Services.Feedback
{
    // "Not for me": blocked artists never appear in suggestions for that user
    public class FeedbackService(AppDbContext context, IUserContextHelper userContextHelper, IMemoryCache memoryCache) : IFeedbackService
    {
        public async Task<GetBlockedArtistsDto> GetBlockedArtists()
        {
            var userId = userContextHelper.GetUserId();

            return new GetBlockedArtistsDto
            {
                Artists = await context.BlockedArtists
                    .Where(b => b.UserId == userId)
                    .OrderBy(b => b.ArtistName)
                    .Select(b => new BlockedArtistItem
                    {
                        Id = b.Id,
                        ArtistName = b.ArtistName,
                        DeezerArtistId = b.DeezerArtistId,
                        CreatedAt = b.CreatedAt
                    })
                    .ToArrayAsync()
            };
        }

        public async Task<Result<int>> BlockArtist(BlockArtistRequest request)
        {
            var userId = userContextHelper.GetUserId();
            var normalisedName = request.ArtistName.NormaliseArtistName();

            if (normalisedName.Length == 0)
            {
                return Result.Fail<int>("Which artist do you want to hide?");
            }

            if (await context.BlockedArtists.AnyAsync(b => b.UserId == userId && b.NormalisedName == normalisedName))
            {
                return Result.ConflictFail<int>($"{request.ArtistName} is already hidden.");
            }

            var blockedArtist = new BlockedArtist
            {
                UserId = userId,
                ArtistName = request.ArtistName.Trim(),
                NormalisedName = normalisedName,
                DeezerArtistId = request.DeezerArtistId,
                CreatedAt = DateTime.UtcNow
            };

            await context.BlockedArtists.AddAsync(blockedArtist);
            await context.SaveChangesAsync();

            // Drop the cached feed so the artist disappears straight away
            memoryCache.Remove(DiscoverFeedService.GetCacheKey(userId));

            return Result.Ok(blockedArtist.Id);
        }

        public async Task<Result> UnblockArtist(int blockedArtistId)
        {
            var userId = userContextHelper.GetUserId();

            var changes = await context.BlockedArtists
                .Where(b => b.Id == blockedArtistId && b.UserId == userId)
                .ExecuteDeleteAsync();

            if (changes == 0)
            {
                return Result.NotFoundFail("That artist isn't hidden.");
            }

            memoryCache.Remove(DiscoverFeedService.GetCacheKey(userId));

            return Result.Ok();
        }
    }
}
