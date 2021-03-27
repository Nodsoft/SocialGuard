using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Bson.Serialization.Conventions;
using Serilog;
using Serilog.Events;
using System.Threading.Tasks;

namespace SocialGuard.Api
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			ConventionRegistry.Register("Ignore null values", new ConventionPack { new IgnoreIfNullConvention(true) }, t => true);

			using IHost host = CreateHostBuilder(args).Build();
			using IServiceScope scope = host.Services.CreateScope();

			IConfiguration configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

			Log.Logger = new LoggerConfiguration()
			#if DEBUG
				.MinimumLevel.Verbose()
			#else
				.MinimumLevel.Information()
				.MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
				.MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
			#endif
				.Enrich.FromLogContext()
				.Enrich.WithProperty("_Source", typeof(Program).Assembly.GetName())
				.Enrich.WithProperty("_Environment", configuration["environment"])
				.WriteTo.Console()
//				.WriteTo.Logger(fileLogger)
				.CreateLogger();

			await host.RunAsync();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder.UseStartup<Startup>();
					webBuilder.UseSerilog();
				});
	}
}
