using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using SocialGuard.Web.Services;



namespace SocialGuard.Web;

public class Program
{
	public static async Task Main(string[] args)
	{
		IHost host = CreateHostBuilder(args).Build();

		Log.Logger = new LoggerConfiguration()
#if DEBUG
			.MinimumLevel.Debug()
#else
				.MinimumLevel.Information()
				.MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
				.MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
#endif
			.Enrich.FromLogContext()
			.WriteTo.Console()
			.CreateLogger();

		await host.RunAsync();
	}

	public static IHostBuilder CreateHostBuilder(string[] args) =>
		Host.CreateDefaultBuilder(args)
			.ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>())
			.UseSerilog();
}