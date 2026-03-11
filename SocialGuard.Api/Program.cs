using MongoDB.Bson.Serialization.Conventions;
using Microsoft.EntityFrameworkCore;
using SocialGuard.Api;
using SocialGuard.Api.Data;

// ConventionRegistry.Register("Ignore null values", new ConventionPack { new IgnoreIfNullConvention(true) }, t => true);

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.ConfigureApplicationServices();

await using WebApplication host = builder.Build();
host.ConfigureApplicationPipeline();

await using AsyncServiceScope scope = host.Services.CreateAsyncScope();

await using (ApiDbContext db = scope.ServiceProvider.GetRequiredService<ApiDbContext>())
{
	await db.Database.MigrateAsync();
}

await using (AuthDbContext db = scope.ServiceProvider.GetRequiredService<AuthDbContext>())
{
	await db.Database.MigrateAsync();
}

await host.RunAsync();