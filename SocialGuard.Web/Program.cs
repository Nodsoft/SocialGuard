using SocialGuard.Web;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.ConfigureApplicationServices();

await using WebApplication host = builder.Build();
host.ConfigureApplicationPipeline();

await host.RunAsync();