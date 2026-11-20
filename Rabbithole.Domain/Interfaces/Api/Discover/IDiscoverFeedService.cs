using Rabbithole.Domain.DTOs.Discover.Responses;

namespace Rabbithole.Domain.Interfaces.Api.Discover
{
    public interface IDiscoverFeedService
    {
        Task<GetDiscoverFeedDto> GetFeed(bool refresh);
    }
}
