using Blazored.LocalStorage;
using SocialGuard.Web.Viewer.Data;

namespace SocialGuard.Web.Viewer.Services;

/// <summary>
/// Provides a service to handle the user's authentication to a SocialGuard API.
/// </summary>
public sealed class ApiAuthenticationService
{
	private readonly ILocalStorageService _localStorage;

	public ApiAuthenticationService(ILocalStorageService localStorage)
	{
		_localStorage = localStorage;
		
		// Initialize the local storage with the default values, if they don't exist.
		// Async method in a constructor is not recommended, but this is a singleton service.
		_ = InitializeAsync();
	}

	/// <summary>
	/// Determines if the user is authenticated to the SocialGuard API.
	/// </summary>
	/// <see cref=""/>
	/// <returns><see langword="true"/> if the user is authenticated, otherwise <see langword="false"/>.</returns>
	public async Task<bool> IsAuthenticatedAsync(Uri host)
	{
		AuthenticationToken[]? tokens = await _localStorage.GetItemAsync<AuthenticationToken[]>("tokens");
		return tokens.FirstOrDefault(t => host == t.Host) is { } token && token.Expiration > DateTime.UtcNow;
	}

	/// <summary>
	/// Gets the authentication token for the SocialGuard API.
	/// </summary>
	/// <param name="performLogin">If true, a login will be performed if the user is not authenticated, or the token is expired.</param>
	/// <returns>The authentication token, if present in store</returns>
	public async Task<AuthenticationToken?> GetAuthenticationTokenAsync(bool performLogin = true) => await _localStorage.GetItemAsync<AuthenticationToken>("tokens");

	/// <summary>
	/// Gets all known authentication details API hosts, and their associated logins (if any).
	/// </summary>
	/// <returns>A collection of authentication details.</returns>
	/// <remarks>
	/// This method does not check if the authentication details are valid.
	/// These credentials may be updated, expired, or removed at any time.
	/// </remarks>
	public async Task<IEnumerable<AuthenticationDetails>> GetKnownAuthenticationDetailsAsync() =>
		from detail in await _localStorage.GetItemAsync<AuthenticationDetails[]?>("logins") ?? Array.Empty<AuthenticationDetails>()
		select detail with { Password = null };
	
	
	internal async Task InitializeAsync(CancellationToken ct = default)
	{
		if (await _localStorage.GetItemAsync<AuthenticationToken[]>("tokens", ct) is null)
		{
			await _localStorage.SetItemAsync("tokens", Array.Empty<AuthenticationToken>(), ct);
		}
		
		if (await _localStorage.GetItemAsync<AuthenticationDetails[]>("logins", ct) is null)
		{
			await _localStorage.SetItemAsync("logins", new[]
			{
				new AuthenticationDetails("NSYS SocialGuard", new("https://api.socialguard.net"))
			}, ct);
		}
	}
}