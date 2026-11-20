using Rabbithole.Domain.DTOs.Requests.Requests;
using Rabbithole.Domain.DTOs.Requests.Responses;
using Rabbithole.Domain.Results;

namespace Rabbithole.Domain.Interfaces.Api.Requests
{
    public interface IRequestsService
    {
        Task<Result<CreateRequestDto>> RequestAlbum(RequestAlbumRequest request);
        Task<Result<CreateRequestDto>> RequestTrack(RequestTrackRequest request);
        Task<GetRequestsDto> GetMyRequests();
    }
}
