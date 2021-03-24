using System;
using AspNetCore.Identity.Mongo;
using AspNetCore.Identity.Mongo.Model;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Transcom.SocialGuard.Api.Services.Authentication;
using System.IO;
using MongoDB.Driver;
using Transcom.SocialGuard.Api.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.OpenApi.Extensions;

namespace Transcom.SocialGuard.Api
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }
		public static string Version { get; } = typeof(Startup).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;


		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddControllers();
			services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc(Version, new OpenApiInfo
				{
					Title = "SocialGuard",
					Version = Version,
					Description = "Centralised Discord Trustlist to keep servers safe from known blacklisted users.",
					Contact = new() { Name = "Transcom-DT", Url = new("https://github.com/Transcom-DT/SocialGuard") },
					License = new() { Name = "GNU GPLv3", Url = new("https://www.gnu.org/licenses/gpl-3.0.html") },
				});

				// Set the comments path for the Swagger JSON and UI.
				string xmlFile = $"{typeof(Startup).Assembly.GetName().Name}.xml";
				string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
				c.IncludeXmlComments(xmlPath);

				// Bearer token authentication
				c.AddSecurityDefinition("jwt_auth", new OpenApiSecurityScheme()
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

				c.AddSecurityRequirement(new OpenApiSecurityRequirement()
				{
					{ securityScheme, Array.Empty<string>() },
				});
			});

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


			services.AddTransient<AuthenticationService>();

			services.AddSingleton(s => new MongoClient(Configuration["MongoDatabase:ConnectionString"]).GetDatabase(Configuration["MongoDatabase:DatabaseName"]));

			services.AddSingleton<TrustlistUserService>()
					.AddSingleton<EmitterService>();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseStaticFiles();

			app.UseSwagger();
			app.UseSwaggerUI(c => c.SwaggerEndpoint($"/swagger/{Version}/swagger.json", $"SocialGuard {Version}"));

			app.UseHttpsRedirection();

			app.UseRouting();

			if (env.IsProduction()) // Nginx configuration step
			{
				app.UseForwardedHeaders(new ForwardedHeadersOptions
				{
					ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
				});
			}

			app.UseAuthentication();
			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				/*
				 * Remove once proper website is built.
				 */
				endpoints.MapGet("/", context =>
				{
					context.Response.Redirect("swagger/index.html");
					return Task.CompletedTask;
				});

				endpoints.MapControllers();
			});
		}
	}
}
