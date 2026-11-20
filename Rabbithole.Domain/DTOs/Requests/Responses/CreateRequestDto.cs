using Rabbithole.Domain.Enums;

namespace Rabbithole.Domain.DTOs.Requests.Responses
{
    public class CreateRequestDto
    {
        public required int RequestId { get; set; }
        public required RequestStatus Status { get; set; }
        public string? StatusMessage { get; set; }
    }
}
