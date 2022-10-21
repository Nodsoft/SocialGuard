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
	}

	/// <summary>
	/// Determines if the user is authenticated to the SocialGuard API.
	/// </summary>
	/// <see cref=""/>
	/// <returns><see langword="true"/> if the user is authenticated, otherwise <see langword="false"/>.</returns>
	public async Task<bool> IsAuthenticatedAsync(Uri host)
	{
		AuthenticationToken[]? tokens = await _localStorage.GetItemAsync<AuthenticationToken[]>("tokens");

		if (tokens.FirstOrDefault(token => host == token.Host))
		{
			
		}
	}

	/// <summary>
	/// Gets the authentication token for the SocialGuard API.
	/// </summary>
	/// <returns>The authentication token, if present in store</returns>
	public async Task<AuthenticationToken?> GetAuthenticationTokenAsync() => await _localStorage.GetItemAsync<AuthenticationToken>("tokens")
	{
		
	}
}