using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using MongoDB.Bson.Serialization.Conventions;
using System.Threading.Tasks;

namespace SocialGuard.Api
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			ConventionRegistry.Register("Ignore null values", new ConventionPack { new IgnoreIfNullConvention(true) }, t => true);

			using IHost host = CreateHostBuilder(args).Build();

			await host.RunAsync();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder.UseStartup<Startup>();
				});
	}
}
