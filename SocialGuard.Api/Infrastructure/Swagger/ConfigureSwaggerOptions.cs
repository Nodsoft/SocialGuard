using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SocialGuard.Api.Infrastructure.Swagger
{
	public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
	{
		private readonly IApiVersionDescriptionProvider _provider;

		/// <summary>
		/// Initializes a new instance of the <see cref="ConfigureSwaggerOptions"/> class.
		/// </summary>
		/// <param name="provider">The <see cref="IApiVersionDescriptionProvider">provider</see> used to generate Swagger documents.</param>
		public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider) => _provider = provider;

		/// <inheritdoc />
		public void Configure(SwaggerGenOptions options)
		{
			// add a swagger document for each discovered API version
			// note: you might choose to skip or document deprecated API versions differently
			foreach (ApiVersionDescription description in _provider.ApiVersionDescriptions)
			{
				options.SwaggerDoc(description.GroupName, CreateInfoForApiVersion(description));
			}
		}

		private static OpenApiInfo CreateInfoForApiVersion(ApiVersionDescription description)
		{
			OpenApiInfo info = new()
			{
				Title = "SocialGuard",
				Version = description.ApiVersion.ToString(),
				Description = "Centralised Discord Trustlist to keep servers safe from known blacklisted users.",
				Contact = new() { Name = "Nodsoft Systems", Url = new("https://github.com/Nodsoft/SocialGuard") },
				License = new() { Name = "GNU GPLv3", Url = new("https://www.gnu.org/licenses/gpl-3.0.html") }
			};

			if (description.IsDeprecated)
			{
				info.Description += " This API version has been deprecated.";
			}

			return info;
		}
	}
}
