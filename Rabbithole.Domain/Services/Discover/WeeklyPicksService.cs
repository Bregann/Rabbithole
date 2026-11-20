using Microsoft.EntityFrameworkCore;
using Rabbithole.Domain.DTOs.Discover.Responses;
using Rabbithole.Domain.Database.Context;
using Rabbithole.Domain.Extensions;
using Rabbithole.Domain.Interfaces.Api.Discover;
using Rabbithole.Domain.Interfaces.Helpers;
using Rabbithole.Domain.Interfaces.ThirdParty;

namespace Rabbithole.Domain.Services.Discover
{
    // This week's ListenBrainz exploration tracks, matched to Deezer so they can be previewed and requested
    public class WeeklyPicksService(
        AppDbContext context,
        ICurrentUserHelper currentUserHelper,
        IWeeklyExplorationService weeklyExplorationService,
        IDeezerApiClient deezerApiClient) : IWeeklyPicksService
    {
        private const int MaxPicks = 30;

        public async Task<GetWeeklyPicksDto> GetWeeklyPicks()
        {
            var userId = currentUserHelper.GetUserId();

            var listenBrainzUsername = await context.Users
                .Where(u => u.Id == userId)
                .Select(u => u.ListenBrainzUsername)
                .FirstOrDefaultAsync();

            if (listenBrainzUsername == null)
            {
                return new GetWeeklyPicksDto { IsListenBrainzLinked = false, Picks = [] };
            }

            var snapshot = await currentUserHelper.GetLibrarySnapshot();
            var tracks = await weeklyExplorationService.GetWeeklyExploration(listenBrainzUsername);
            var picks = new List<WeeklyPickItem>();

            foreach (var track in tracks.Where(t => !snapshot.IsBlocked(t.ArtistName)).Take(MaxPicks))
            {
                var deezerMatch = (await deezerApiClient.SearchTracks(track.ArtistName, track.Title, 1)).FirstOrDefault();

                picks.Add(new WeeklyPickItem
                {
                    Title = track.Title,
                    ArtistName = track.ArtistName,
                    LibraryState = snapshot.GetState(track.ArtistName),
                    Track = deezerMatch?.ToTrackItem()
                });
            }

            return new GetWeeklyPicksDto
            {
                IsListenBrainzLinked = true,
                PlaylistTitle = tracks.FirstOrDefault()?.PlaylistTitle,
                Picks = picks.ToArray()
            };
        }
    }
}
