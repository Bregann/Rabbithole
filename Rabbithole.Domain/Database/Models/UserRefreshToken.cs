using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rabbithole.Domain.Database.Models
{
    [Index(nameof(TokenHash), IsUnique = true)]
    public class UserRefreshToken
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public required int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        // Only the SHA-256 hash is stored, never the token itself
        [Required]
        public required string TokenHash { get; set; }

        [Required]
        public required DateTime ExpiresAt { get; set; }

        [Required]
        public required DateTime CreatedAt { get; set; }
    }
}
