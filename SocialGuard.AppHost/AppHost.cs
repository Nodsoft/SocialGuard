using Microsoft.Extensions.Hosting;
using Projects;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

// DB: Postgres
IResourceBuilder<PostgresServerResource> postgres = builder.AddPostgres("postgres")
    .WithDataVolume();

IResourceBuilder<PostgresDatabaseResource> apiDb = postgres.AddDatabase("apidb");

// App: SocialGuard API
var api = builder.AddProject<SocialGuard_Api>("api")
    .WithExternalHttpEndpoints()
    .WithReference(apiDb).WaitFor(apiDb)
    .WithHttpHealthCheck("/health");

// App: SocialGuard Web
var web = builder.AddProject<SocialGuard_Web>("web")
    .WithExternalHttpEndpoints()
    .WithReference(api)
    .WithHttpHealthCheck("/health");

// App: Records Browser
var browser = builder.AddProject<SocialGuard_Web_Viewer>("browser", launchProfileName: "aspire")
    .WithExternalHttpEndpoints()
    .WithReference(api).WaitFor(api);

await using DistributedApplication host = builder.Build();

await host.StartAsync();
await host.WaitForShutdownAsync();