namespace Rabbithole.Domain.DTOs.Feedback.Responses
{
    public class GetBlockedArtistsDto
    {
        public required BlockedArtistItem[] Artists { get; set; } = [];
    }

    public class BlockedArtistItem
    {
        public required int Id { get; set; }
        public required string ArtistName { get; set; }
        public long? DeezerArtistId { get; set; }
        public required DateTime CreatedAt { get; set; }
    }
}
