using Rabbithole.Domain.DTOs.Discover.Responses;

namespace Rabbithole.Domain.Interfaces.Api.Discover
{
    public interface IWeeklyPicksService
    {
        Task<GetWeeklyPicksDto> GetWeeklyPicks();
    }
}
