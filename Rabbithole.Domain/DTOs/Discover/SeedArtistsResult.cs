namespace Rabbithole.Domain.DTOs.Discover
{
    public class SeedArtistsResult
    {
        public required string Source { get; set; }
        public required string[] Names { get; set; } = [];
    }
}
