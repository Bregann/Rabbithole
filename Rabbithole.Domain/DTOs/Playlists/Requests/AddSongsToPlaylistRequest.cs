namespace Rabbithole.Domain.DTOs.Playlists.Requests
{
    public class AddSongsToPlaylistRequest
    {
        public required string PlaylistId { get; set; }
        public required string[] SongIds { get; set; } = [];
    }
}
