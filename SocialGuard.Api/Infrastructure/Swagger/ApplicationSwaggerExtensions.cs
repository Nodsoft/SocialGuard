using Microsoft.OpenApi.Models;

namespace SocialGuard.Api.Infrastructure.Swagger;

public static class ApplicationSwaggerExtensions
{
    public static WebApplicationBuilder AddApplicationSwagger(this WebApplicationBuilder builder)
    {
        builder.Services.AddSwaggerGen(options =>
        {
            options.OperationFilter<SwaggerDefaultValues>();

            // Set the comments path for the Swagger JSON and UI.
            string xmlFile = $"{typeof(Startup).Assembly.GetName().Name}.xml";
            string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            options.IncludeXmlComments(xmlPath);

            // Bearer token authentication
            options.AddSecurityDefinition("jwt_auth",
                new()
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

        return builder;
    }
}