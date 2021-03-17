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

namespace Transcom.SocialGuard.Api
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{

			services.AddControllers();
			services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1", new OpenApiInfo { Title = "Natsecure SocialGuard", Version = "v1" });

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
					{ securityScheme, Array.Empty<string>()},
				});
			});

			services.AddIdentityMongoDbProvider<ApplicationUser, UserRole>(
				options => { },
				mongo => { });

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
			app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Natsecure SocialGuard v1"));

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
				endpoints.MapControllers();
			});
		}
	}
}
