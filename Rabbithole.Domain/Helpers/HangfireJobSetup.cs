using Hangfire;
using Rabbithole.Domain.Interfaces.Api.Requests;
using Rabbithole.Domain.Interfaces.Api.Sync;
using Serilog;

namespace Rabbithole.Domain.Helpers
{
    public class HangfireJobSetup
    {
        // Every recurring job is registered here, always against an interface
        public static void RegisterJobs()
        {
            // Move album requests through Lidarr and add finished songs to playlists
            RecurringJob.AddOrUpdate<IRequestProcessingService>("ProcessAlbumRequests", s => s.ProcessAllRequests(), "*/15 * * * *");

            // ListenBrainz refreshes Weekly Exploration on Mondays, so rebuild playlists early Tuesday
            RecurringJob.AddOrUpdate<IWeeklyDiscoverySyncService>("WeeklyDiscoverySync", s => s.RunForAllUsers(), "0 4 * * 2");

            Log.Information("[Hangfire] Jobs have been registered successfully.");
        }
    }
}
