using System.Text.Json.Serialization;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SocialGuard.Client.Http.Services;
using SocialGuard.Common.Data.Models.Authentication;
using SocialGuard.Web.Viewer;
using SocialGuard.Web.Viewer.Data;
using SocialGuard.Web.Viewer.Services;

#pragma warning disable CA1416 // Validate platform compatibility (WebAssembly)

WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddSocialGuardHttpClientFactory(
	static async (client, services, ct) =>
	{
		ApiAuthenticationService apiAuthService = services.GetRequiredService<ApiAuthenticationService>();

		if (await apiAuthService.GetAuthenticationTokenAsync(client.ClientId, ct: ct) is { IsExpired: false } token)
		{
			return token.Token;
		}
		
		
		if (!(await apiAuthService.GetActiveAuthenticationDetailsAsync(ct)).TryGetValue(client.ClientId, out AuthenticationDetails? details))
		{
			return null;
		}

		if (details is { Active: true, Login: not null, Password: not null })
		{
			// Now we need to login.
			if (await client.LoginAsync(details.Login, details.Password, ct) is { } authResult)
			{
				token = new(details.Host, authResult.Token, authResult.Expiration);

				// Save the token.
				await apiAuthService.SetAuthenticationTokenAsync(client.ClientId, token);
				return token.Token;
			}

			// The details are not valid, so we need to remove them.
			await apiAuthService.RemoveAuthenticationDetailsAsync(client.ClientId, ct);
		}

		return null;
	},

	static (hostUri, services) =>
	{
		// Assign the ClientId based on the AuthenticationDetails stored in the Blazored LocalStorage.
		ApiAuthenticationService apiAuthService = services.GetRequiredService<ApiAuthenticationService>();
		AuthenticationDetails? details = apiAuthService.GetActiveAuthenticationDetailsAsync().GetAwaiter().GetResult().Values.FirstOrDefault(x => x.Host == hostUri);
		return details?.Id ?? Guid.NewGuid();
	}
	
);

builder.Services.AddBlazoredLocalStorage(options =>
	{
		options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
	}
);

builder.Services.AddScoped<ApiAuthenticationService>();

WebAssemblyHost host = builder.Build();
await host.RunAsync();