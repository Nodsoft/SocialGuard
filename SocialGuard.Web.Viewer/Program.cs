using System.Text.Json.Serialization;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SocialGuard.Client.Http.Services;
using SocialGuard.Web.Viewer;
using SocialGuard.Web.Viewer.Data;
using SocialGuard.Web.Viewer.Services;

#pragma warning disable CA1416 // Validate platform compatibility (WebAssembly)

WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddSocialGuardHttpClientFactory(
	_ClientAuthTokenProvider,
	_ClientIdAssignmentAsyncDelegate
);

builder.Services.AddTransient<Task<ChainedClient>>(static async services =>
{
	// Get all active authentication details from the Blazored LocalStorage.
	ApiAuthenticationService apiAuthService = services.GetRequiredService<ApiAuthenticationService>();
	ValueTask<Dictionary<Guid, AuthenticationDetails>> getActiveDetailsTask = apiAuthService.GetActiveAuthenticationDetailsAsync();

	// Create a ChainedClient with all the active authentication details.
	return new(services.GetRequiredService<SocialGuardHttpClientFactory>().CreateClientsAsync((
		await getActiveDetailsTask).Values.Select(static x => x.Host)));
});

builder.Services.AddBlazoredLocalStorage(options =>
{
	options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
});

builder.Services.AddScoped<ApiAuthenticationService>();

WebAssemblyHost host = builder.Build();
await host.RunAsync();



#region Local Functions


static async ValueTask<string?> _ClientAuthTokenProvider(SocialGuardHttpClient client, IServiceProvider services, CancellationToken ct)
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
}

static async ValueTask<Guid> _ClientIdAssignmentAsyncDelegate(Uri hostUri, IServiceProvider services)
{
	// Assign the ClientId based on the AuthenticationDetails stored in the Blazored LocalStorage.
	ApiAuthenticationService apiAuthService = services.GetRequiredService<ApiAuthenticationService>();
	AuthenticationDetails? details = (await apiAuthService.GetActiveAuthenticationDetailsAsync()).Values.FirstOrDefault(x => x.Host == hostUri);
	return details?.Id ?? Guid.NewGuid();
}


#endregion Local Functions