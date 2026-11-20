using Microsoft.EntityFrameworkCore;
using Rabbithole.Domain.Database.Context;
using Rabbithole.Domain.Database.Models;
using Rabbithole.Domain.DTOs.Requests.Requests;
using Rabbithole.Domain.DTOs.Requests.Responses;
using Rabbithole.Domain.Enums;
using Rabbithole.Domain.Exceptions;
using Rabbithole.Domain.Interfaces.Api.Requests;
using Rabbithole.Domain.Interfaces.Helpers;
using Rabbithole.Domain.Interfaces.ThirdParty;
using Rabbithole.Domain.Results;
using Serilog;

namespace Rabbithole.Domain.Services.Requests
{
    public class RequestsService(
        AppDbContext context,
        IUserContextHelper userContextHelper,
        IDeezerApiClient deezerApiClient,
        IEnvironmentalSettingHelper environmentalSettingHelper,
        IRequestProcessingService requestProcessingService) : IRequestsService
    {
        private const int DefaultWeeklyLimit = 5;

        public async Task<Result<CreateRequestDto>> RequestAlbum(RequestAlbumRequest request)
        {
            var albumRequest = await GetOrCreateAlbumRequest(request.DeezerAlbumId);

            if (!albumRequest.Success)
            {
                return Result.Fail<CreateRequestDto>(albumRequest.Error);
            }

            return Result.Ok(await ProcessAndDescribe(albumRequest.Value.Id));
        }

        public async Task<Result<CreateRequestDto>> RequestTrack(RequestTrackRequest request)
        {
            var track = await deezerApiClient.GetTrack(request.DeezerTrackId);

            if (track == null)
            {
                return Result.NotFoundFail<CreateRequestDto>("Couldn't find that song.");
            }

            if (track.Album == null || track.Album.Id == 0)
            {
                return Result.Fail<CreateRequestDto>("Couldn't work out which album that song is on.");
            }

            var albumRequest = await GetOrCreateAlbumRequest(track.Album.Id);

            if (!albumRequest.Success)
            {
                return Result.Fail<CreateRequestDto>(albumRequest.Error);
            }

            if (!string.IsNullOrWhiteSpace(request.PlaylistId))
            {
                // Remember to add this song to the playlist once Lidarr has it
                await context.PendingPlaylistTracks.AddAsync(new PendingPlaylistTrack
                {
                    AlbumRequestId = albumRequest.Value.Id,
                    NavidromePlaylistId = request.PlaylistId,
                    ArtistName = track.Artist?.Name ?? albumRequest.Value.ArtistName,
                    TrackTitle = track.Title,
                    CreatedAt = DateTime.UtcNow
                });

                await context.SaveChangesAsync();
            }

            return Result.Ok(await ProcessAndDescribe(albumRequest.Value.Id));
        }

        public async Task<GetRequestsDto> GetMyRequests()
        {
            var userId = userContextHelper.GetUserId();
            var isAdmin = userContextHelper.IsAdmin();
            var weeklyLimit = GetWeeklyLimit();

            var requests = await context.AlbumRequests
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.RequestedAt)
                .Select(r => new RequestItem
                {
                    Id = r.Id,
                    ArtistName = r.ArtistName,
                    AlbumTitle = r.AlbumTitle,
                    CoverUrl = r.CoverUrl,
                    Status = r.Status,
                    StatusMessage = r.StatusMessage,
                    RequestedAt = r.RequestedAt,
                    CompletedAt = r.CompletedAt,
                    PendingPlaylistTracks = r.PendingPlaylistTracks.Count(p => p.AddedAt == null && p.GaveUpAt == null)
                })
                .ToArrayAsync();

            var usedThisWeek = await CountRequestsThisWeek(userId);

            return new GetRequestsDto
            {
                Requests = requests,
                WeeklyLimit = isAdmin ? null : weeklyLimit,
                RemainingThisWeek = isAdmin ? null : Math.Max(0, weeklyLimit - usedThisWeek)
            };
        }

        private async Task<Result<AlbumRequest>> GetOrCreateAlbumRequest(long deezerAlbumId)
        {
            if (environmentalSettingHelper.TryGetEnvironmentalSettingValue(EnvironmentalSettingEnum.LidarrUrl) == null)
            {
                return Result.Fail<AlbumRequest>("Requests are off until an admin sets up Lidarr.");
            }

            var userId = userContextHelper.GetUserId();

            // Someone already asked for this album and it's not failed: share that request instead of asking Lidarr twice
            var existing = await context.AlbumRequests
                .Where(r => r.DeezerAlbumId == deezerAlbumId && r.Status != RequestStatus.Failed)
                .OrderByDescending(r => r.RequestedAt)
                .FirstOrDefaultAsync();

            if (existing != null)
            {
                return Result.Ok(existing);
            }

            if (!userContextHelper.IsAdmin() && await CountRequestsThisWeek(userId) >= GetWeeklyLimit())
            {
                return Result.Fail<AlbumRequest>($"You've used all {GetWeeklyLimit()} requests for this week.");
            }

            var album = await deezerApiClient.GetAlbum(deezerAlbumId);

            if (album == null)
            {
                return Result.NotFoundFail<AlbumRequest>("Couldn't find that album.");
            }

            if (album.Artist == null)
            {
                return Result.Fail<AlbumRequest>("Deezer doesn't say who made that album.");
            }

            var now = DateTime.UtcNow;

            var albumRequest = new AlbumRequest
            {
                UserId = userId,
                ArtistName = album.Artist.Name,
                AlbumTitle = album.Title,
                DeezerAlbumId = album.Id,
                CoverUrl = album.CoverXl ?? album.CoverMedium,
                Status = RequestStatus.Pending,
                RequestedAt = now,
                UpdatedAt = now
            };

            await context.AlbumRequests.AddAsync(albumRequest);
            await context.SaveChangesAsync();

            Log.Information("[Requests] {UserId} requested {Artist} - {Album}", userId, albumRequest.ArtistName, albumRequest.AlbumTitle);

            return Result.Ok(albumRequest);
        }

        private async Task<CreateRequestDto> ProcessAndDescribe(int albumRequestId)
        {
            try
            {
                // Try straight away so the user sees progress; the scheduled job picks up anything left over
                await requestProcessingService.ProcessRequest(albumRequestId);
            }
            catch (ExternalServiceException ex)
            {
                Log.Warning(ex, "[Requests] Immediate processing of {RequestId} failed, will retry", albumRequestId);
            }

            return await context.AlbumRequests
                .AsNoTracking()
                .Where(r => r.Id == albumRequestId)
                .Select(r => new CreateRequestDto
                {
                    RequestId = r.Id,
                    Status = r.Status,
                    StatusMessage = r.StatusMessage
                })
                .FirstAsync();
        }

        private async Task<int> CountRequestsThisWeek(int userId)
        {
            var weekAgo = DateTime.UtcNow.AddDays(-7);
            return await context.AlbumRequests.CountAsync(r => r.UserId == userId && r.RequestedAt > weekAgo);
        }

        private int GetWeeklyLimit()
        {
            var value = environmentalSettingHelper.TryGetEnvironmentalSettingValue(EnvironmentalSettingEnum.WeeklyRequestCap);
            return int.TryParse(value, out var limit) ? limit : DefaultWeeklyLimit;
        }
    }
}
