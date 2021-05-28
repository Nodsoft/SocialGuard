using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SocialGuard.Api.Infrastructure.Swagger
{
	/// <summary>
	/// Represents the Swagger/Swashbuckle operation filter used to document the implicit API version parameter.
	/// </summary>
	/// <remarks>This <see cref="IOperationFilter"/> is only required due to bugs in the <see cref="SwaggerGenerator"/>.
	/// Once they are fixed and published, this class can be removed.</remarks>
	public class SwaggerDefaultValues : IOperationFilter
	{
		/// <summary>
		/// Applies the filter to the specified operation using the given context.
		/// </summary>
		/// <param name="operation">The operation to apply the filter to.</param>
		/// <param name="context">The current operation filter context.</param>
		public void Apply(OpenApiOperation operation, OperationFilterContext context)
		{
			ApiDescription apiDescription = context.ApiDescription;
			operation.Deprecated |= apiDescription.IsDeprecated();
			
			foreach ((OpenApiResponse response, string contentType) in
				from ApiResponseType responseType in context.ApiDescription.SupportedResponseTypes
				let responseKey = responseType.IsDefaultResponse ? "default" : responseType.StatusCode.ToString()
				let response = operation.Responses[responseKey]
				from string contentType in response.Content.Keys
				where responseType.ApiResponseFormats.All(x => x.MediaType != contentType)
				select (response, contentType))
			{
				response.Content.Remove(contentType);
			}

			if (operation.Parameters is not null)
			{
				foreach ((OpenApiParameter parameter, ApiParameterDescription description) in
					from OpenApiParameter parameter in operation.Parameters
					let description = apiDescription.ParameterDescriptions.First(p => p.Name == parameter.Name)
					select (parameter, description))
				{
					parameter.Description ??= description.ModelMetadata.Description;
					
					if (parameter.Schema.Default is null && description.DefaultValue is not null)
					{
						parameter.Schema.Default = OpenApiAnyFactory.CreateFromJson(JsonSerializer.Serialize(description.DefaultValue, description.ModelMetadata.ModelType));
					}

					parameter.Required |= description.IsRequired;
				}
			}
		}
	}
}
