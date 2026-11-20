using Hangfire;
using Rabbithole.Domain.Results;

namespace Rabbithole.Domain.Interfaces.Api.Requests
{
    public interface IRequestProcessingService
    {
        [DisableConcurrentExecution(timeoutInSeconds: 600)]
        [AutomaticRetry(Attempts = 0)]
        Task ProcessAllRequests();
        Task<Result> ProcessRequest(int albumRequestId);
    }
}
