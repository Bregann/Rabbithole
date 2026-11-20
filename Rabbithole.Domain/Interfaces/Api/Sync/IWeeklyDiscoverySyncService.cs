using Hangfire;
using Rabbithole.Domain.DTOs.Discover.Responses;
using Rabbithole.Domain.Results;

namespace Rabbithole.Domain.Interfaces.Api.Sync
{
    public interface IWeeklyDiscoverySyncService
    {
        [DisableConcurrentExecution(timeoutInSeconds: 600)]
        [AutomaticRetry(Attempts = 0)]
        Task RunForAllUsers();
        Task<Result<SyncWeeklyPlaylistDto>> RunForUser(int userId);
    }
}
