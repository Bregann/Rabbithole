using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rabbithole.Domain.Database.Models
{
    // A song someone wanted in a playlist before it was in the library.
    // Added to the Navidrome playlist once its album request is Available and Navidrome has scanned it.
    public class PendingPlaylistTrack
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public required int AlbumRequestId { get; set; }

        [ForeignKey("AlbumRequestId")]
        public virtual AlbumRequest AlbumRequest { get; set; } = null!;

        [Required]
        public required string NavidromePlaylistId { get; set; }

        [Required]
        public required string ArtistName { get; set; }

        [Required]
        public required string TrackTitle { get; set; }

        [Required]
        public required DateTime CreatedAt { get; set; }

        public DateTime? AddedAt { get; set; }

        public DateTime? GaveUpAt { get; set; }
    }
}
