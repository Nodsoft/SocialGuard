using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SocialGuard.Web.Services;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace SocialGuard.Web
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddRazorPages();
			services.AddServerSideBlazor();
			services.AddDistributedMemoryCache();
			services.AddDirectoryBrowser();

			services.AddCors(c => c.AddDefaultPolicy(builder => builder
				.AllowAnyOrigin()
				.AllowAnyHeader()
				.AllowAnyMethod()));

			services.AddSingleton<PageContentLoader>();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseWebAssemblyDebugging();
			}
			else if (env.IsProduction())
			{
				IEnumerable<IPAddress> allowedProxies = Configuration.GetSection("AllowedProxies")?.Get<string[]>()?.Select(x => IPAddress.Parse(x));

				// Nginx configuration step
				ForwardedHeadersOptions forwardedHeadersOptions = new()
				{
					ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
				};

				if (allowedProxies is not null && allowedProxies.Any())
				{
					forwardedHeadersOptions.KnownProxies.Clear();

					foreach (IPAddress address in allowedProxies)
					{
						forwardedHeadersOptions.KnownProxies.Add(address);
					}
				}
			
				app.UseExceptionHandler("/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}

			app.UseHttpsRedirection();
			app.UseStaticFiles();
			app.UseStaticFiles("/viewer");
			app.UseBlazorFrameworkFiles("/viewer");

			app.UseRouting();
			app.UseCors();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapBlazorHub();
				endpoints.MapFallbackToFile("/viewer/{*path:nonfile}", "viewer/index.html");
				endpoints.MapFallbackToPage("/_Host");
			});
		}
	}
}
