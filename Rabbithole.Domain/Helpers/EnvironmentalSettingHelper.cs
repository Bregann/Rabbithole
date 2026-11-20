using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Rabbithole.Domain.Database.Context;
using Rabbithole.Domain.Enums;
using Rabbithole.Domain.Interfaces.Helpers;

namespace Rabbithole.Domain.Helpers
{
    // Singleton cache of the EnvironmentalSettings table. Creates its own scope for the DbContext
    // because DbContext is scoped and this helper lives for the lifetime of the app.
    public class EnvironmentalSettingHelper(IServiceProvider serviceProvider) : IEnvironmentalSettingHelper
    {
        private Dictionary<EnvironmentalSettingEnum, string> _environmentalSettings = [];

        public async Task LoadEnvironmentalSettings()
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var settings = await context.EnvironmentalSettings.ToArrayAsync();

                _environmentalSettings = settings
                    .Where(s => Enum.TryParse<EnvironmentalSettingEnum>(s.Key, out _))
                    .ToDictionary(s => Enum.Parse<EnvironmentalSettingEnum>(s.Key), s => s.Value);
            }
        }

        public string? TryGetEnvironmentalSettingValue(EnvironmentalSettingEnum key)
        {
            return _environmentalSettings.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value) ? value : null;
        }

        public async Task<bool> UpdateEnvironmentalSettingValue(EnvironmentalSettingEnum key, string newValue)
        {
            if (!_environmentalSettings.ContainsKey(key))
            {
                return false;
            }

            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                await context.EnvironmentalSettings
                    .Where(s => s.Key == key.ToString())
                    .ExecuteUpdateAsync(s => s.SetProperty(x => x.Value, newValue));
            }

            _environmentalSettings[key] = newValue;
            return true;
        }
    }
}
