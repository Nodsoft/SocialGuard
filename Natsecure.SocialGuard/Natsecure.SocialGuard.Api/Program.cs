using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Natsecure.SocialGuard.Api.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Natsecure.SocialGuard.Api
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			using IHost host = CreateHostBuilder(args).Build();
			using IServiceScope scope = host.Services.CreateScope();

			ApiDbContext context = host.Services.GetRequiredService<IDbContextFactory<ApiDbContext>>().CreateDbContext();
			await context.Database.MigrateAsync();

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
