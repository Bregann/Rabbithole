namespace Rabbithole.Domain.DTOs.ThirdParty.Navidrome
{
    // A proxied binary response (audio or cover art). The caller owns and must dispose Content.
    public class NavidromeStream
    {
        public required Stream Content { get; set; }
        public required string ContentType { get; set; }
        public required int StatusCode { get; set; }
        public long? ContentLength { get; set; }
        public string? ContentRange { get; set; }
        public string? AcceptRanges { get; set; }
    }
}
