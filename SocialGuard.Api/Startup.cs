using Microsoft.Extensions.Options;
using SocialGuard.Api.Hubs;
using SocialGuard.Api.Infrastructure.Conversions;
using SocialGuard.Api.Infrastructure.Swagger;
using SocialGuard.Api.Services;
using SocialGuard.Api.Services.Authentication;
using SocialGuard.Api.Services.Logging;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Net;
using System.Reflection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using SocialGuard.Api.Data;
using SocialGuard.Api.Data.Authentication;
using SocialGuard.Api.Infrastructure.Authentication;
using SocialGuard.Api.Services.Admin;


namespace SocialGuard.Api;

public static class Startup
{
	public static string Version { get; } = typeof(Startup).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;


	// This method gets called by the runtime. Use this method to add services to the container.
	public static void ConfigureApplicationServices(this WebApplicationBuilder builder)
	{
		builder.AddServiceDefaults();
		
		Log.Logger = new LoggerConfiguration()
			.WriteTo.Console()
			.CreateBootstrapLogger();
		
		builder.Services.AddSerilog((services, lc) => lc
			.ReadFrom.Configuration(builder.Configuration)
			.ReadFrom.Services(services)
			.Enrich.FromLogContext()
			.WriteTo.Console(theme: Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme.Code)
		);
		
		builder.Services.AddControllers(config => { config.ModelBinderProviders.Insert(0, new CommaSeparatedArrayModelBinderProvider()); });

		builder.Services.AddApiVersioning(config =>
		{
			config.DefaultApiVersion = new(3, 1, "rc1");
			config.AssumeDefaultVersionWhenUnspecified = true;
			config.ReportApiVersions = true;
		});

		builder.Services.AddVersionedApiExplorer(options =>
		{
			// add the versioned api explorer, which also adds IApiVersionDescriptionProvider service
			// note: the specified format code will format the version as "'v'major[.minor][-status]"
			options.GroupNameFormat = "'v'VVV";

			// note: this option is only necessary when versioning by url segment. the SubstitutionFormat
			// can also be used to control the format of the API version in route templates
			options.SubstituteApiVersionInUrl = true;
		});

		string dbConnectionString = builder.Configuration.GetConnectionString("Database");

		builder.Services.AddDbContextPool<ApiDbContext>(o => o
			.UseNpgsql(dbConnectionString)
			.UseSnakeCaseNamingConvention()
		);

		builder.Services.AddDbContextPool<AuthDbContext>(o => o
			.UseNpgsql(dbConnectionString)
			.UseSnakeCaseNamingConvention()
		);

		builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
		
		builder.AddApplicationSwagger();
		
		builder.Services.AddSignalR(config => config.EnableDetailedErrors = true)
			.AddMessagePackProtocol();

		// Add Identity
		builder.Services.AddIdentity<ApplicationUser, UserRole>()
			.AddEntityFrameworkStores<AuthDbContext>()
			.AddDefaultTokenProviders();

		builder.AddApplicationAuthentication();

		
		builder.Services.AddCors(c => c.AddDefaultPolicy(builder => builder
			.AllowAnyOrigin()
			.AllowAnyHeader()
			.AllowAnyMethod()
		));
		
		builder.Services.AddScoped<ITrustlistService, PostgresTrustlistService>();
		builder.Services.AddScoped<IEmitterService, PostgresEmitterService>();
		
		builder.Services.AddTransient<AuthenticationService>();
		builder.Services.AddTransient<AdminService>();

		builder.Services.AddApplicationInsightsTelemetry(options => { });
	}

	// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
	public static void ConfigureApplicationPipeline(this WebApplication app)
	{
		// Request Logging
		app.UseSerilogRequestLogging(options =>
		{
			// Customize the message template
			options.MessageTemplate =
				"{RequestScheme} {RequestMethod} {RequestPath} by {RequestClient} responded {StatusCode} in {Elapsed:0.0000} ms";

			// Attach additional properties to the request completion event
			options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
			{
				diagnosticContext.Set("RequestClient", httpContext.Connection.RemoteIpAddress);
				diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme.ToUpperInvariant());
			};
		});
		
		if (app.Environment.IsDevelopment())
		{
			app.UseDeveloperExceptionPage();
		}

		app.UseStaticFiles();
		
		app.UseHttpsRedirection();

		app.UseRouting();
		app.UseCors();

		HashSet<IPAddress>? allowedProxies = app.Configuration.GetSection("AllowedProxies").Get<string[]>()?.Select(IPAddress.Parse).ToHashSet();
		app.ConfigureReverseProxySupport(allowedProxies);
			
		app.UseAuthentication();
		app.UseAuthorization();

		app.MapDefaultEndpoints();
		app.MapGet("/", ctx =>
		{
			ctx.Response.Redirect("/swagger/index.html");
			return Task.CompletedTask;
		});
		
		app.MapControllers();

		app.MapHub<TrustlistHub>("/hubs/trustlist");
	}
}