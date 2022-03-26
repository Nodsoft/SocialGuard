using AspNetCore.Identity.Mongo;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using SocialGuard.Api.Hubs;
using SocialGuard.Api.Infrastructure.Conversions;
using SocialGuard.Api.Infrastructure.Swagger;
using SocialGuard.Api.Services;
using SocialGuard.Api.Services.Authentication;
using SocialGuard.Api.Services.Logging;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SocialGuard.Api.Data;


namespace SocialGuard.Api
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }
		public static string Version { get; } = typeof(Startup).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;


		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddControllers(config =>
			{
				config.ModelBinderProviders.Insert(0, new CommaSeparatedArrayModelBinderProvider());
			});

			services.AddApiVersioning(config =>
			{
				config.DefaultApiVersion = new(3, 0, "beta");
				config.AssumeDefaultVersionWhenUnspecified = true;
				config.ReportApiVersions = true;
			});

			services.AddVersionedApiExplorer(options =>
			{
				// add the versioned api explorer, which also adds IApiVersionDescriptionProvider service
				// note: the specified format code will format the version as "'v'major[.minor][-status]"
				options.GroupNameFormat = "'v'VVV";

				// note: this option is only necessary when versioning by url segment. the SubstitutionFormat
				// can also be used to control the format of the API version in route templates
				options.SubstituteApiVersionInUrl = true;
			});

			string dbConnectionString = Configuration.GetConnectionString("Database");

			services.AddDbContextPool<ApiDbContext>(
				o => o.UseNpgsql(dbConnectionString,
					p => { p.EnableRetryOnFailure(); }
				)
					.UseSnakeCaseNamingConvention()
			);

			services.AddDbContextPool<AuthDbContext>(o =>
				o.UseNpgsql(dbConnectionString,
						p => { p.EnableRetryOnFailure(); }
					)
					.UseSnakeCaseNamingConvention()
			);

			services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

			services.AddSwaggerGen(options =>
			{
				options.OperationFilter<SwaggerDefaultValues>();

				// Set the comments path for the Swagger JSON and UI.
				string xmlFile = $"{typeof(Startup).Assembly.GetName().Name}.xml";
				string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
				options.IncludeXmlComments(xmlPath);

				// Bearer token authentication
				options.AddSecurityDefinition("jwt_auth", new()
				{
					Name = "bearer",
					BearerFormat = "JWT",
					Scheme = "bearer",
					Description = "Specify the authorization token.",
					In = ParameterLocation.Header,
					Type = SecuritySchemeType.Http,
				});

				// Make sure swagger UI requires a Bearer token specified
				OpenApiSecurityScheme securityScheme = new()
				{
					Reference = new()
					{
						Id = "jwt_auth",
						Type = ReferenceType.SecurityScheme
					}
				};

				options.AddSecurityRequirement(new()
				{
					{ securityScheme, Array.Empty<string>() }
				});
			});


			services.AddSignalR(config => config.EnableDetailedErrors = true)
				.AddMessagePackProtocol();
			

			/*
			services.AddIdentityMongoDbProvider<ApplicationUser, UserRole, string>(
				options => { },
				mongo =>
				{
					IConfigurationSection config = Configuration.GetSection("Auth");
					mongo.ConnectionString = config["ConnectionString"];
					mongo.MigrationCollection = config["Tables:Migration"];
					mongo.RolesCollection = config["Tables:Role"];
					mongo.UsersCollection = config["Tables:User"];
				});
			*/

			// Add Identity
			services.AddIdentity<ApplicationUser, UserRole>()
				.AddEntityFrameworkStores<AuthDbContext>()
				.AddDefaultTokenProviders();


			services.AddAuthentication(options =>
			{
				options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
				options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
				options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
			})

			// Adding Jwt Bearer  
			.AddJwtBearer(options =>
			{
				options.SaveToken = true;
				options.RequireHttpsMetadata = false;
				options.TokenValidationParameters = new()
				{
					ValidateIssuer = true,
					ValidateAudience = true,
					ValidAudience = Configuration["JWT:ValidAudience"],
					ValidIssuer = Configuration["JWT:ValidIssuer"],
					IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JWT:Secret"]))
				};
			});

			services.AddCors(c => c.AddDefaultPolicy(builder => builder
				.AllowAnyOrigin()
				.AllowAnyHeader()
				.AllowAnyMethod()));


			services.AddTransient<AuthenticationService>();
			
			services.AddTransient<TrustlistHub>();

			services.AddSingleton(s => new MongoClient(Configuration["MongoDatabase:ConnectionString"]).GetDatabase(Configuration["MongoDatabase:DatabaseName"]));

			services.AddSingleton<ITrustlistUserService, TrustlistUserService>()
					.AddSingleton<IEmitterService, EmitterService>();
			
			services.AddApplicationInsightsTelemetry(options =>
			{
#if DEBUG
				options.DeveloperMode = true;
#endif
			});
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider provider)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
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
			}

			app.UseStaticFiles();

			app.UseSwagger(options => { options.RouteTemplate = "swagger/{documentName}/swagger.json"; });
			app.UseSwaggerUI(options =>
			{
				options.RoutePrefix = "swagger";

				foreach (ApiVersionDescription description in provider.ApiVersionDescriptions)
				{
					options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToLowerInvariant());
				}
			});

			app.UseHttpsRedirection();

			app.UseRouting();
			app.UseCors();

			app.UseAuthentication();
			app.UseAuthorization();

			app.UseMiddleware<RequestLoggingMiddleware>();

			app.UseEndpoints(endpoints =>
			{
				/*
				 * Remove once proper website is built.
				 */
				endpoints.MapGet("/", context =>
				{
					context.Response.Redirect("/swagger/index.html");
					return Task.CompletedTask;
				});

				endpoints.MapControllers();

				endpoints.MapHub<TrustlistHub>("/hubs/trustlist");
			});
		}
	}
}
