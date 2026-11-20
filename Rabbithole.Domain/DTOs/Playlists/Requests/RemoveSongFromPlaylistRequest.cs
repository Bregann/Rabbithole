namespace Rabbithole.Domain.DTOs.Playlists.Requests
{
    public class RemoveSongFromPlaylistRequest
    {
        public required string PlaylistId { get; set; }
        public required int SongIndex { get; set; }
    }
}
