using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Rabbithole.Domain.Database.Models;

namespace Rabbithole.Domain.Database.Context
{
    public class AppDbContext(DbContextOptions options) : DbContext(options), IDataProtectionKeyContext
    {
        public DbSet<EnvironmentalSetting> EnvironmentalSettings { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserRefreshToken> UserRefreshTokens { get; set; }
        public DbSet<BlockedArtist> BlockedArtists { get; set; }
        public DbSet<AlbumRequest> AlbumRequests { get; set; }
        public DbSet<PendingPlaylistTrack> PendingPlaylistTracks { get; set; }

        // Persists ASP.NET Data Protection keys so stored Navidrome passwords survive container restarts
        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }
    }
}
