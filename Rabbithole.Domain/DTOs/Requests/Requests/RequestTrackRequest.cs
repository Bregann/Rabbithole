namespace Rabbithole.Domain.DTOs.Requests.Requests
{
    public class RequestTrackRequest
    {
        public required long DeezerTrackId { get; set; }
        public string? PlaylistId { get; set; }
    }
}
