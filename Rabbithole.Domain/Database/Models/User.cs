using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rabbithole.Domain.Database.Models
{
    [Index(nameof(NavidromeUsername), IsUnique = true)]
    public class User
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public required string NavidromeUsername { get; set; }

        [Required]
        public required string EncryptedNavidromePassword { get; set; }

        public string? ListenBrainzUsername { get; set; }

        // The Navidrome playlist Rabbithole rebuilds every week for this user
        public string? DiscoverPlaylistId { get; set; }

        [Required]
        public required bool IsAdmin { get; set; }

        [Required]
        public required DateTime CreatedAt { get; set; }

        [Required]
        public required DateTime LastLoginAt { get; set; }

        public virtual ICollection<UserRefreshToken> RefreshTokens { get; set; } = null!;

        public virtual ICollection<BlockedArtist> BlockedArtists { get; set; } = null!;

        public virtual ICollection<AlbumRequest> AlbumRequests { get; set; } = null!;
    }
}
