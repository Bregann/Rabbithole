using Rabbithole.Domain.DTOs.Player.Responses;
using Rabbithole.Domain.DTOs.ThirdParty.Navidrome;
using Rabbithole.Domain.Results;

namespace Rabbithole.Domain.Interfaces.Api.Player
{
    public interface IPlayerService
    {
        Task<Result<NavidromeStream>> Stream(string songId, string? rangeHeader);
        Task<Result<NavidromeStream>> GetCoverArt(string coverArtId, int size);
        Task<Result<GetPreviewDto>> GetPreview(long deezerTrackId);
    }
}
