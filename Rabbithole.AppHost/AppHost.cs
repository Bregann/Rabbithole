var builder = DistributedApplication.CreateBuilder(args);

// Generated once and persisted in user secrets, so dev logins survive restarts
var jwtKey = builder.AddParameter("jwt-key", new GenerateParameterDefault { MinLength = 48, Special = false }, secret: true, persist: true);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume();

var database = postgres.AddDatabase("rabbithole");

var api = builder.AddProject<Projects.Rabbithole_Core>("api")
    .WithReference(database)
    .WaitFor(database)
    .WithEnvironment("JwtKey", jwtKey)
    .WithExternalHttpEndpoints();

var web = builder.AddViteApp("web", "../rabbithole.web")
    .WithReference(api)
    .WaitFor(api);

// At publish time the built SPA is copied into the API's wwwroot, so Rabbithole ships as one image
api.PublishWithContainerFiles(web, "./wwwroot");

builder.Build().Run();
