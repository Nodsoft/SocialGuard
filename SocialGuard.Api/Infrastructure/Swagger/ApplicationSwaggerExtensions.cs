using Microsoft.OpenApi;

namespace SocialGuard.Api.Infrastructure.Swagger;

public static class ApplicationSwaggerExtensions
{
    public static WebApplicationBuilder AddApplicationSwagger(this WebApplicationBuilder builder)
    {
        builder.Services.AddSwaggerGen(options =>
        {
            // Set the comments path for the Swagger JSON and UI.
            string xmlFile = $"{typeof(Startup).Assembly.GetName().Name}.xml";
            string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            options.IncludeXmlComments(xmlPath);

            OpenApiSecurityScheme jwtScheme = new()
            {
                Name = "bearer",
                BearerFormat = "JWT",
                Scheme = "bearer",
                Description = "Specify the authorization token.",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
            };
            
            // Bearer token authentication
            options.AddSecurityDefinition("jwt_auth", jwtScheme);

            options.AddSecurityRequirement(_ => new() {
                { new("jwt_auth"), [] }
            });
        });

        return builder;
    }
}