using Rabbithole.Domain.DTOs.Discover;

namespace Rabbithole.Domain.Interfaces.Api.Discover
{
    public interface IWeeklyExplorationService
    {
        Task<WeeklyExplorationTrack[]> GetWeeklyExploration(string listenBrainzUsername);
    }
}
