using Rabbithole.Domain.Enums;

namespace Rabbithole.Domain.DTOs.Status.Responses
{
    public class GetStatusDto
    {
        public required string Version { get; set; }
        public required bool IsFullyConfigured { get; set; }
        public required IntegrationStatusItem[] Integrations { get; set; } = [];
    }

    public class IntegrationStatusItem
    {
        public required IntegrationType Integration { get; set; }
        public required bool IsConfigured { get; set; }
    }
}
