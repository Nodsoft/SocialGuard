using MongoDB.Bson.Serialization.Conventions;
using Serilog;
using Microsoft.EntityFrameworkCore;
using Serilog.Events;
using SocialGuard.Api.Data;

namespace SocialGuard.Api
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			ConventionRegistry.Register("Ignore null values", new ConventionPack { new IgnoreIfNullConvention(true) }, t => true);

			using IHost host = CreateHostBuilder(args).Build();
			using IServiceScope scope = host.Services.CreateScope();

			await using (ApiDbContext db = scope.ServiceProvider.GetRequiredService<ApiDbContext>())
			{
				await db.Database.MigrateAsync();
			}

			await using (AuthDbContext db = scope.ServiceProvider.GetRequiredService<AuthDbContext>())
			{
				await db.Database.MigrateAsync();
			}

			
			await host.RunAsync();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureWebHostDefaults(webBuilder => 
					webBuilder.UseStartup<Startup>())
				.UseSerilog((hostingCtx, config) => config.ReadFrom.Configuration(hostingCtx.Configuration));
	}
}
