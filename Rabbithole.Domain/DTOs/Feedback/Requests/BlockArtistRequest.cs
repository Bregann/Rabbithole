namespace Rabbithole.Domain.DTOs.Feedback.Requests
{
    public class BlockArtistRequest
    {
        public required string ArtistName { get; set; }
        public long? DeezerArtistId { get; set; }
    }
}
