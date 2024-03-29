using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using Serilog;
using SocialGuard.Api.Data;

namespace SocialGuard.Api.DbMigrator;

public static class Program
{
	public static async Task Main(string[] args)
	{
		Log.Logger = new LoggerConfiguration()
			.MinimumLevel.Information()
			.Enrich.FromLogContext()
			.WriteTo.Console()
			.CreateLogger();
		
		Mappings.ConfigureMapper();
		
		IHost host = Host.CreateDefaultBuilder(args)
			.ConfigureServices(ConfigureWorkerServices)
			.UseSerilog()
			.Build();

		await host.RunAsync();
	}

	private static void ConfigureWorkerServices(HostBuilderContext hostContext, IServiceCollection services)
	{
		services.AddDbContext<ApiDbContext>(o =>
			{
				o.UseNpgsql(hostContext.Configuration.GetConnectionString("Postgres"));
				o.UseSnakeCaseNamingConvention();
				o.EnableDetailedErrors();
				o.EnableSensitiveDataLogging();
			}
		);
		services.AddDbContext<AuthDbContext>(o =>
			{
				o.UseNpgsql(hostContext.Configuration.GetConnectionString("Postgres"));
				o.UseSnakeCaseNamingConvention();
				o.EnableDetailedErrors();
				o.EnableSensitiveDataLogging();
			}
		);
		services.AddSingleton<IMongoClient>(_ => new MongoClient(hostContext.Configuration.GetConnectionString("Mongo")));

		services.AddHostedService<DbMigrator>();
	}
}

