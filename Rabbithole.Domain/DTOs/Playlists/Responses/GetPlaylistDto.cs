using Rabbithole.Domain.DTOs.Shared;

namespace Rabbithole.Domain.DTOs.Playlists.Responses
{
    public class GetPlaylistDto
    {
        public required PlaylistItem Playlist { get; set; }
        public required LibrarySongItem[] Songs { get; set; } = [];
    }
}
