using Hangfire;
using Hangfire.Dashboard.BasicAuthorization;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Rabbithole.Domain.Database.Context;
using Rabbithole.Domain.Enums;
using Rabbithole.Domain.Helpers;
using Rabbithole.Domain.Interfaces.Api.Artists;
using Rabbithole.Domain.Interfaces.Api.Auth;
using Rabbithole.Domain.Interfaces.Api.Discover;
using Rabbithole.Domain.Interfaces.Api.Feedback;
using Rabbithole.Domain.Interfaces.Api.Library;
using Rabbithole.Domain.Interfaces.Api.Player;
using Rabbithole.Domain.Interfaces.Api.Playlists;
using Rabbithole.Domain.Interfaces.Api.Requests;
using Rabbithole.Domain.Interfaces.Api.Settings;
using Rabbithole.Domain.Interfaces.Api.Status;
using Rabbithole.Domain.Interfaces.Api.Sync;
using Rabbithole.Domain.Interfaces.Helpers;
using Rabbithole.Domain.Interfaces.ThirdParty;
using Rabbithole.Domain.Services.Artists;
using Rabbithole.Domain.Services.Auth;
using Rabbithole.Domain.Services.Discover;
using Rabbithole.Domain.Services.Feedback;
using Rabbithole.Domain.Services.Library;
using Rabbithole.Domain.Services.Player;
using Rabbithole.Domain.Services.Playlists;
using Rabbithole.Domain.Services.Requests;
using Rabbithole.Domain.Services.Settings;
using Rabbithole.Domain.Services.Status;
using Rabbithole.Domain.Services.Sync;
using Rabbithole.Domain.ThirdParty.AudioMuse;
using Rabbithole.Domain.ThirdParty.Deezer;
using Rabbithole.Domain.ThirdParty.Lidarr;
using Rabbithole.Domain.ThirdParty.ListenBrainz;
using Rabbithole.Domain.ThirdParty.Navidrome;
using Serilog;
using System.Text;

var loggerConfiguration = new LoggerConfiguration()
    .WriteTo.Async(x => x.File("/app/Logs/log.log", retainedFileCountLimit: 7, rollingInterval: RollingInterval.Day))
    .WriteTo.Console()
    .Enrich.WithProperty("Application", "Rabbithole" + (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development" ? "-Test" : ""));

// Seq is optional so the public image doesn't depend on anyone's log server
var seqUrl = Environment.GetEnvironmentVariable("SeqUrl");
if (!string.IsNullOrWhiteSpace(seqUrl))
{
    loggerConfiguration.WriteTo.Seq(seqUrl);
}

Log.Logger = loggerConfiguration.CreateLogger();
Log.Information("Logger Setup");

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("rabbithole")
    ?? throw new InvalidOperationException("ConnectionStrings__rabbithole is not set. Point it at your Postgres database.");

var jwtKey = builder.Configuration["JwtKey"]
    ?? throw new InvalidOperationException("JwtKey is not set. Use a random string of at least 32 characters.");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();
builder.Services.AddHealthChecks();

// Expected failures come back from the services as Results, so only unhandled exceptions reach
// the exception handler. ProblemDetails turns those into a generic 500 with no internals exposed
builder.Services.AddProblemDetails();

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseLazyLoadingProxies()
           .UseNpgsql(connectionString));

// Stored Navidrome passwords are encrypted with keys kept in Postgres, so they survive container rebuilds
builder.Services.AddDataProtection()
    .SetApplicationName("Rabbithole")
    .PersistKeysToDbContext<AppDbContext>();

// Helpers
builder.Services.AddSingleton<IEnvironmentalSettingHelper, EnvironmentalSettingHelper>();
builder.Services.AddScoped<IUserContextHelper, UserContextHelper>();
builder.Services.AddScoped<ICurrentUserHelper, CurrentUserHelper>();
builder.Services.AddScoped<ICredentialProtector, CredentialProtector>();
builder.Services.AddScoped<INavidromeCredentialHelper, NavidromeCredentialHelper>();
builder.Services.AddScoped<ILibraryStateHelper, LibraryStateHelper>();
builder.Services.AddSingleton<IJwtTokenHelper, JwtTokenHelper>();

// External API clients
builder.Services.AddHttpClient<INavidromeApiClient, NavidromeApiClient>();
builder.Services.AddHttpClient<IDeezerApiClient, DeezerApiClient>(c => c.Timeout = TimeSpan.FromSeconds(15));
builder.Services.AddHttpClient<IListenBrainzApiClient, ListenBrainzApiClient>(c => c.Timeout = TimeSpan.FromSeconds(20));
builder.Services.AddHttpClient<ILidarrApiClient, LidarrApiClient>(c => c.Timeout = TimeSpan.FromSeconds(30));
builder.Services.AddHttpClient<IAudioMuseApiClient, AudioMuseApiClient>(c => c.Timeout = TimeSpan.FromSeconds(20));

// Status
builder.Services.AddScoped<IStatusService, StatusService>();

// Auth & settings
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ISettingsService, SettingsService>();
builder.Services.AddScoped<IFeedbackService, FeedbackService>();

// Discovery
builder.Services.AddScoped<IArtistsService, ArtistsService>();
builder.Services.AddScoped<IRelatedArtistsService, RelatedArtistsService>();
builder.Services.AddScoped<ISeedArtistsService, SeedArtistsService>();
builder.Services.AddScoped<IDiscoverFeedService, DiscoverFeedService>();
builder.Services.AddScoped<IWeeklyExplorationService, WeeklyExplorationService>();
builder.Services.AddScoped<IWeeklyPicksService, WeeklyPicksService>();

// Library, playlists & playback
builder.Services.AddScoped<ILibraryService, LibraryService>();
builder.Services.AddScoped<IPlaylistsService, PlaylistsService>();
builder.Services.AddScoped<IPlaylistSuggestionsService, PlaylistSuggestionsService>();
builder.Services.AddScoped<IPlayerService, PlayerService>();

// Requests & scheduled jobs
builder.Services.AddScoped<IRequestsService, RequestsService>();
builder.Services.AddScoped<IRequestProcessingService, RequestProcessingService>();
builder.Services.AddScoped<IWeeklyDiscoverySyncService, WeeklyDiscoverySyncService>();

// Auth: JWT read from the accessToken cookie (same pattern as Orbit)
builder.Services.AddAuthorization();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Map "nameid"/"unique_name"/"role" back to ClaimTypes.* so UserContextHelper and [Authorize(Roles)] work
        options.MapInboundClaims = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = JwtTokenHelper.Issuer,
            ValidAudience = JwtTokenHelper.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                context.Token = context.Request.Cookies["accessToken"];
                return Task.CompletedTask;
            }
        };
    });

// Hangfire
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(c => c.UseNpgsqlConnection(connectionString)));

builder.Services.AddHangfireServer(options => options.SchedulePollingInterval = TimeSpan.FromSeconds(10));

var app = builder.Build();

// Migrate and seed before anything reads settings
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await context.Database.MigrateAsync();
    await DatabaseSeedHelper.SeedEnvironmentalSettings(context, Environment.GetEnvironmentVariable);
}

var environmentalSettingHelper = app.Services.GetRequiredService<IEnvironmentalSettingHelper>();
await environmentalSettingHelper.LoadEnvironmentalSettings();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// The React SPA is copied into wwwroot at publish time (see Dockerfile / AppHost)
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

var hangfireAuth = new[] { new BasicAuthAuthorizationFilter(new BasicAuthAuthorizationFilterOptions
{
    RequireSsl = false,
    SslRedirect = false,
    LoginCaseSensitive = true,
    Users =
    [
        new BasicAuthAuthorizationUser
        {
            Login = environmentalSettingHelper.TryGetEnvironmentalSettingValue(EnvironmentalSettingEnum.HangfireUsername) ?? "admin",
            PasswordClear = environmentalSettingHelper.TryGetEnvironmentalSettingValue(EnvironmentalSettingEnum.HangfirePassword) ?? Guid.NewGuid().ToString()
        }
    ]
})};

app.MapHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = hangfireAuth,
    IgnoreAntiforgeryToken = true
}, JobStorage.Current);

app.MapControllers();
// Unraid/Docker health checks
app.MapHealthChecks("/health");

// Client-side routes fall back to the SPA
app.MapFallbackToFile("index.html");

HangfireJobSetup.RegisterJobs();

app.Run();
