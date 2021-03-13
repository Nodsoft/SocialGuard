using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



// https://github.com/DJDaemonix/WoWS-Karma/blob/main/WowsKarma.Api/Services/Authentication/AccessKeyAttribute.cs

namespace Transcom.SocialGuard.Api.Services.Authentication
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
	public sealed class AccessKeyAttribute : Attribute, IAsyncAuthorizationFilter
	{
		private const string AccessKeyHeaderName = "Access-Key";
		private static readonly string apiKeysLocation = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "access-keys.txt";

		public string[] Scopes { get; init; }

		public AccessKeyAttribute()
		{
			Scopes = null;
		}

		public AccessKeyAttribute(params string[] scope)
		{
			Scopes = scope;
		}


		public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
		{
			// "AllowAnonymous" skips all authorization
			if (context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any())
			{
				return;
			}

			if (!context.HttpContext.Request.Headers.TryGetValue(AccessKeyHeaderName, out StringValues extractedApiKey))
			{
				context.Result = new UnauthorizedResult();
				return;
			} 

			string[] apiKeys = await File.ReadAllLinesAsync(apiKeysLocation, Encoding.ASCII);
			Dictionary<string, string[]> apiKeysSplitted = apiKeys.Select(x => x.Split('|')).ToDictionary(x => x[0], x => x[1].Split(','));

			if (apiKeysSplitted.TryGetValue(extractedApiKey, out string[] keyScopes))
			{
				if (Scopes is not null)
				{
					if (!Scopes.Append("*").Intersect(keyScopes).Any())
					{
						context.Result = new UnauthorizedResult();
					}
				}

				return;
			}

			context.Result = new UnauthorizedResult();
			return;
		}
	}
}
