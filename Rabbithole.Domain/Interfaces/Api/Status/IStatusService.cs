using Rabbithole.Domain.DTOs.Status.Responses;

namespace Rabbithole.Domain.Interfaces.Api.Status
{
    public interface IStatusService
    {
        GetStatusDto GetStatus();
    }
}
