var builder = DistributedApplication.CreateBuilder(args);

// Generated once and saved to the AppHost's user secrets, so restarts keep the same values.
// Postgres only reads its password when the data volume is first created, so it must never change between runs.
var postgresPassword = builder.AddParameter("postgres-password", new GenerateParameterDefault { MinLength = 24, Special = false }, secret: true, persist: true);
var jwtKey = builder.AddParameter("jwt-key", new GenerateParameterDefault { MinLength = 48, Special = false }, secret: true, persist: true);

// No default: the dashboard asks for it on the first run (e.g. http://192.168.1.100:4533).
// Sign-in needs it, and the in-app Settings page is only reachable after signing in.
// Lidarr and AudioMuse are set from Settings once you're in.
var navidromeUrl = builder.AddParameter("navidrome-url");

var postgres = builder.AddPostgres("postgres", password: postgresPassword)
    .WithDataVolume();

var database = postgres.AddDatabase("rabbithole");

var api = builder.AddDotnetProject("api", "../Rabbithole.Core/Rabbithole.Core.csproj")
    .WithReference(database)
    .WaitFor(database)
    .WithEnvironment("JwtKey", jwtKey)
    .WithEnvironment("NavidromeUrl", navidromeUrl)
    .WithHttpHealthCheck("/health")
    .WithExternalHttpEndpoints();

// Fixed port so the app is always at http://localhost:5173 in development
var web = builder.AddViteApp("web", "../rabbithole.web")
    .WithEndpoint("http", endpoint => endpoint.Port = 5173)
    .WithReference(api)
    .WaitFor(api);

// At publish time the built SPA is copied into the API's wwwroot, so Rabbithole ships as one image
api.PublishWithContainerFiles(web, "./wwwroot");

builder.Build().Run();
