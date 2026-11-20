using Rabbithole.Domain.DTOs.Player.Responses;
using Rabbithole.Domain.DTOs.ThirdParty.Navidrome;
using Rabbithole.Domain.Interfaces.Api.Player;
using Rabbithole.Domain.Interfaces.Helpers;
using Rabbithole.Domain.Interfaces.ThirdParty;
using Rabbithole.Domain.Results;

namespace Rabbithole.Domain.Services.Player
{
    // Full songs and cover art are proxied from Navidrome (its URL may not be reachable from the browser,
    // and the user's credentials never leave the server). Previews are Deezer's 30-second clips.
    public class PlayerService(
        ICurrentUserHelper currentUserHelper,
        INavidromeApiClient navidromeApiClient,
        IDeezerApiClient deezerApiClient) : IPlayerService
    {
        private const int MaxCoverArtSize = 1200;

        public async Task<Result<NavidromeStream>> Stream(string songId, string? rangeHeader)
        {
            var credentials = await currentUserHelper.GetCredentials();
            var stream = await navidromeApiClient.Stream(credentials, songId, rangeHeader);

            return stream == null
                ? Result.NotFoundFail<NavidromeStream>("That song isn't available in Navidrome.")
                : Result.Ok(stream);
        }

        public async Task<Result<NavidromeStream>> GetCoverArt(string coverArtId, int size)
        {
            var credentials = await currentUserHelper.GetCredentials();
            var coverArt = await navidromeApiClient.GetCoverArt(credentials, coverArtId, Math.Clamp(size, 32, MaxCoverArtSize));

            return coverArt == null
                ? Result.NotFoundFail<NavidromeStream>("No cover art for that.")
                : Result.Ok(coverArt);
        }

        public async Task<Result<GetPreviewDto>> GetPreview(long deezerTrackId)
        {
            // Always fetched fresh: preview URLs are signed and expire
            var track = await deezerApiClient.GetTrack(deezerTrackId);

            if (track == null)
            {
                return Result.NotFoundFail<GetPreviewDto>("Couldn't find that track.");
            }

            return Result.Ok(new GetPreviewDto
            {
                PreviewUrl = string.IsNullOrWhiteSpace(track.Preview) ? null : track.Preview
            });
        }
    }
}
