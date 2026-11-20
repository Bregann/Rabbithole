using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rabbithole.Domain.Database.Models
{
    public class BlockedArtist
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public required int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [Required]
        public required string ArtistName { get; set; }

        // Lower-cased, punctuation-free name used for matching across Deezer/Navidrome/Lidarr
        [Required]
        public required string NormalisedName { get; set; }

        public long? DeezerArtistId { get; set; }

        [Required]
        public required DateTime CreatedAt { get; set; }
    }
}
