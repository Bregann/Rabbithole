using Rabbithole.Domain.Database.Context;
using Rabbithole.Domain.Database.Models;
using Rabbithole.Domain.Enums;

namespace Rabbithole.Tests.Infrastructure
{
    /// <summary>
    /// Small, focused seed methods. Seed only what a test needs.
    /// </summary>
    public static class TestDatabaseSeedHelper
    {
        public static async Task<User> SeedTestUser(AppDbContext context, string username = "testuser", bool isAdmin = true)
        {
            var user = new User
            {
                NavidromeUsername = username,
                EncryptedNavidromePassword = "encrypted-test-password",
                ListenBrainzUsername = username,
                IsAdmin = isAdmin,
                CreatedAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow
            };

            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            return user;
        }

        public static async Task SeedEnvironmentalSetting(AppDbContext context, EnvironmentalSettingEnum key, string value)
        {
            await context.EnvironmentalSettings.AddAsync(new EnvironmentalSetting
            {
                Key = key.ToString(),
                Value = value
            });

            await context.SaveChangesAsync();
        }
    }
}
