using Microsoft.EntityFrameworkCore;
using Rabbithole.Domain.Database.Context;
using Rabbithole.Domain.Database.Models;
using Rabbithole.Domain.Enums;
using Serilog;

namespace Rabbithole.Domain.Helpers
{
    public static class DatabaseSeedHelper
    {
        // Makes sure every EnvironmentalSettingEnum key has a row.
        // A matching environment variable (e.g. NavidromeUrl=http://navidrome:4533) always wins,
        // so Unraid/Docker users can configure everything from the container template.
        public static async Task SeedEnvironmentalSettings(AppDbContext context, Func<string, string?> getEnvironmentVariable)
        {
            var existingSettings = await context.EnvironmentalSettings.ToDictionaryAsync(s => s.Key);

            foreach (var key in Enum.GetValues<EnvironmentalSettingEnum>())
            {
                var keyName = key.ToString();
                var environmentValue = getEnvironmentVariable(keyName);

                if (existingSettings.TryGetValue(keyName, out var existing))
                {
                    if (!string.IsNullOrWhiteSpace(environmentValue))
                    {
                        existing.Value = environmentValue;
                    }

                    continue;
                }

                await context.EnvironmentalSettings.AddAsync(new EnvironmentalSetting
                {
                    Key = keyName,
                    Value = environmentValue ?? GetDefaultValue(key)
                });
            }

            await context.SaveChangesAsync();
            Log.Information("[Seed] Environmental settings seeded");
        }

        private static string GetDefaultValue(EnvironmentalSettingEnum key)
        {
            return key switch
            {
                EnvironmentalSettingEnum.ListenBrainzApiUrl => "https://api.listenbrainz.org",
                EnvironmentalSettingEnum.WeeklyRequestCap => "5",
                _ => string.Empty
            };
        }
    }
}
