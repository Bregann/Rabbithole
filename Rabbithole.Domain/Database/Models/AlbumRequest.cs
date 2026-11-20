using Rabbithole.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rabbithole.Domain.Database.Models
{
    public class AlbumRequest
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public required int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [Required]
        public required string ArtistName { get; set; }

        [Required]
        public required string AlbumTitle { get; set; }

        [Required]
        public required long DeezerAlbumId { get; set; }

        public string? CoverUrl { get; set; }

        [Required]
        public required RequestStatus Status { get; set; }

        public string? StatusMessage { get; set; }

        public int? LidarrArtistId { get; set; }

        public int? LidarrAlbumId { get; set; }

        [Required]
        public required DateTime RequestedAt { get; set; }

        [Required]
        public required DateTime UpdatedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public virtual ICollection<PendingPlaylistTrack> PendingPlaylistTracks { get; set; } = null!;
    }
}
