using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using SocialGuard.Web.Services;



namespace SocialGuard.Web
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			IHost host = CreateHostBuilder(args).Build();

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
