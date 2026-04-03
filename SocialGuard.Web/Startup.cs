using SocialGuard.Web.Services;
using SocialGuard.Web.Services.Logging;
using System.Net;
using Nodsoft.MoltenObsidian.Blazor;

namespace SocialGuard.Web;

public static class Startup
{
	// This method gets called by the runtime. Use this method to add services to the container.
	// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
	public static void ConfigureApplicationServices(this WebApplicationBuilder builder)
	{
		builder.AddServiceDefaults();
		
		builder.Services.AddRazorPages();

		builder.Services.AddServerSideBlazor()
			.AddInteractiveServerComponents();
		
		builder.Services.AddDistributedMemoryCache();
		builder.Services.AddDirectoryBrowser();
			
		// Add the MoltenObsidian Vault
		builder.Services.AddHttpClient("Docs", client => client.BaseAddress = new(builder.Configuration.GetValue<string>("Docs:WebRoot")));
		builder.Services.AddMoltenObsidianHttpVault(static services => services.GetRequiredService<IHttpClientFactory>().CreateClient("Docs"));
		builder.Services.AddMoltenObsidianBlazorIntegration();

		builder.Services.AddCors(c => c
			.AddDefaultPolicy(static builder => builder
				.AllowAnyOrigin()
				.AllowAnyHeader()
				.AllowAnyMethod()
			)
		);

		builder.Services.AddSingleton<PageContentLoader>();
	}

	// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
	public static void ConfigureApplicationPipeline(this WebApplication app)
	{
		app.UseSerilogRequestLogging();
		
		if (app.Environment.IsDevelopment())
		{
			app.UseDeveloperExceptionPage();
		}
		else if (app.Environment.IsProduction())
		{
			HashSet<IPAddress>? allowedProxies = app.Configuration.GetSection("AllowedProxies").Get<string[]>()?.Select(IPAddress.Parse).ToHashSet();
			app.ConfigureReverseProxySupport(allowedProxies);
			
			app.UseExceptionHandler("/Error");
			// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
			app.UseHsts();
		}

		app.UseHttpsRedirection();

		app.UseMiddleware<RequestLoggingMiddleware>();

		app.UseStaticFiles();
		// app.UseStaticFiles("/viewer");
		// app.UseBlazorFrameworkFiles("/viewer");

		app.UseRouting();
		app.UseCors();

		app.MapDefaultEndpoints();
		app.MapBlazorHub();
		app.MapFallbackToFile("/viewer/{*path:nonfile}", "viewer/index.html");
		app.MapFallbackToPage("/_Host");
	}
}