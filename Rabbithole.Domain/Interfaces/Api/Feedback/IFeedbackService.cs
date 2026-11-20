using Rabbithole.Domain.DTOs.Feedback.Requests;
using Rabbithole.Domain.DTOs.Feedback.Responses;
using Rabbithole.Domain.Results;

namespace Rabbithole.Domain.Interfaces.Api.Feedback
{
    public interface IFeedbackService
    {
        Task<GetBlockedArtistsDto> GetBlockedArtists();
        Task<Result<int>> BlockArtist(BlockArtistRequest request);
        Task<Result> UnblockArtist(int blockedArtistId);
    }
}
