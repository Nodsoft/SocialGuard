using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using Blazored.LocalStorage;
using Microsoft.JSInterop;
using SocialGuard.Web.Viewer.Data;

namespace SocialGuard.Web.Viewer.Services;

/// <summary>
/// Provides a service to handle the user's authentication to a SocialGuard API.
/// </summary>
[SupportedOSPlatform("browser")]
public sealed class ApiAuthenticationService
{
	private readonly ILocalStorageService _localStorage;
	private readonly IJSRuntime _js;
	
	public ApiAuthenticationService(ILocalStorageService localStorage, IJSRuntime js)
	{
		_localStorage = localStorage;
		_js = js;

		// Initialize the local storage with the default values, if they don't exist.
		// Async method in a constructor is not recommended, but this is a singleton service.
		// InitializeAsync().GetAwaiter().GetResult();
	}
	
	

	/// <summary>
	/// Determines if the user is authenticated to the SocialGuard API.
	/// </summary>
	/// <param name="host">The host of the SocialGuard API.</param>
	/// <param name="ct">The cancellation token.</param>
	/// <returns><see langword="true"/> if the user is authenticated, otherwise <see langword="false"/>.</returns>
	public async ValueTask<bool> IsAuthenticatedAsync(Uri host, CancellationToken ct = default)
	{
		AuthenticationToken[]? tokens = await _localStorage.GetItemAsync<AuthenticationToken[]>("tokens", ct);
		return tokens.FirstOrDefault(t => host == t.Host) is { } token && token.Expiration > DateTime.UtcNow;
	}

	/// <summary>
	/// Gets the authentication token for the SocialGuard API.
	/// </summary>
	/// <param name="id">The Authentication details ID to get the token for.</param>
	/// <param name="ct">The cancellation token.</param>
	/// <returns>The authentication token, if present in store</returns>
	public async ValueTask<AuthenticationToken?> GetAuthenticationTokenAsync(Guid id, CancellationToken ct = default)
	{
		Dictionary<Guid, AuthenticationToken>? tokens = await _localStorage.GetItemAsync<Dictionary<Guid, AuthenticationToken>>("tokens", ct);

		return tokens.TryGetValue(id, out AuthenticationToken? details) && details.Expiration > DateTime.UtcNow 
			? details 
			: null;
	}

	/// <summary>
	/// Gets all known authentication details API hosts, and their associated logins (if any).
	/// </summary>
	/// <returns>A collection of authentication details.</returns>
	/// <remarks>
	/// This method does not check if the authentication details are valid.
	/// These credentials may be updated, expired, or removed at any time.
	/// </remarks>
	public async ValueTask<Dictionary<Guid, AuthenticationDetails>> GetKnownAuthenticationDetailsAsync(CancellationToken ct = default) =>
		new(from detail
			in await _localStorage.GetItemAsync<Dictionary<Guid, AuthenticationDetails>>("logins", ct) ?? new Dictionary<Guid, AuthenticationDetails>()
			select new KeyValuePair<Guid, AuthenticationDetails>(detail.Key, detail.Value with { Id = detail.Key, Password = null }));
	
	/// <summary>
	/// Gets all known active authentication details API hosts, and their associated credentials (if any).
	/// </summary>
	/// <param name="ct">The cancellation token.</param>
	/// <returns>A collection of authentication details.</returns>
	internal async ValueTask<Dictionary<Guid, AuthenticationDetails>> GetActiveAuthenticationDetailsAsync(CancellationToken ct = default) => new(
		from detail
		in await _localStorage.GetItemAsync<Dictionary<Guid, AuthenticationDetails>>("logins", ct) ?? new Dictionary<Guid, AuthenticationDetails>()
		where detail.Value.Active
		select new KeyValuePair<Guid, AuthenticationDetails>(detail.Key, detail.Value with { Id = detail.Key, Password = null })
	);
	
	internal async ValueTask InitializeAsync(CancellationToken ct = default)
	{
		// Initialize the tokens and logins stores, if they don't exist.
		
		if (await _localStorage.GetItemAsync<Dictionary<Guid, AuthenticationDetails>>("tokens", ct) is null)
		{
			await _localStorage.SetItemAsync("tokens", new Dictionary<Guid, AuthenticationDetails>(), ct);
		}
		
		if (await _localStorage.GetItemAsync<Dictionary<Guid, AuthenticationDetails>>("logins", ct) is null)
		{
			AuthenticationDetails defaultAuthDetails = new("NSYS SocialGuard", new("https://api.socialguard.net")) { Active = true };
			
			await _localStorage.SetItemAsync("logins", new Dictionary<Guid, AuthenticationDetails>
			{
				{ defaultAuthDetails.Id, defaultAuthDetails }
			}, ct);
		}
	}

	/// <summary>
	/// Upserts a new set of authentication details into the local storage.
	/// </summary>
	/// <param name="details">The authentication details to upsert.</param>
	/// <param name="ct">The cancellation token.</param>
	/// <returns>The ID of the upserted authentication details.</returns>
	public async ValueTask<Guid> UpsertAuthenticationDetailsAsync(AuthenticationDetails details, CancellationToken ct = default)
	{
		Dictionary<Guid, AuthenticationDetails> logins = await _localStorage.GetItemAsync<Dictionary<Guid, AuthenticationDetails>>("logins", ct)
		    ?? throw new InvalidOperationException("The logins store is not initialized.");
		
		logins[details.Id] = details with { Password = await EncryptPasswordAsync(details.Password, ct) };
		await _localStorage.SetItemAsync("logins", logins, ct);
		return details.Id;
	}

	/// <summary>
	/// Removes the authentication details with the specified ID from the local storage.
	/// </summary>
	/// <param name="id">The ID of the authentication details to remove.</param>
	/// <param name="ct">The cancellation token.</param>
	/// <returns><see langword="true"/> if the authentication details were removed, otherwise <see langword="false"/>.</returns>
	public async ValueTask<bool> RemoveAuthenticationDetailsAsync(Guid id, CancellationToken ct = default)
	{
		Dictionary<Guid, AuthenticationDetails> logins = await _localStorage.GetItemAsync<Dictionary<Guid, AuthenticationDetails>>("logins", ct)
		    ?? throw new InvalidOperationException("The logins store is not initialized.");
		
		bool removed = logins.Remove(id);
		await _localStorage.SetItemAsync("logins", logins, ct);
		return removed;
	}

	/// <summary>
	/// Encrypts the specified password using the local key.
	/// </summary>
	/// <param name="password">The password to encrypt.</param>
	/// <param name="ct">The cancellation token.</param>
	/// <returns>The encrypted password.</returns>
	/// <remarks>
	/// This method must be supported in the browser, therefore AES-CBC is the only option.
	/// </remarks>
	[method: Pure]
	[return: NotNullIfNotNull(nameof(password))]
	private async ValueTask<string?> EncryptPasswordAsync(string? password, CancellationToken ct = default)
	{
		if (password is null or "")
		{
			return null;
		}
		
		return await _js.InvokeAsync<string>(/*lang=javascript*/@"encryptAsync", ct, password);
	}

	/// <summary>
	/// Decrypts the specified AES-CBC encrypted password using the local key.
	/// </summary>
	/// <param name="password">The password to decrypt.</param>
	/// <param name="ct">The cancellation token.</param>
	/// <returns>The decrypted password.</returns>
	/// <remarks>
	/// This method must be supported in the browser, therefore AES-CBC is the only option.
	/// </remarks>
	[method: Pure]
	[return: NotNullIfNotNull(nameof(password))]
	public async ValueTask<string?> DecryptPasswordAsync(string? password, CancellationToken ct = default)
	{
		if (password is null or "")
		{
			return null;
		}
		
		return await _js.InvokeAsync<string>(/*lang=javascript*/@"decryptAsync", ct, password);
	}

	/// <summary>
	/// Enables/Disables a set of authentication details to be used.
	/// </summary>
	/// <param name="id">The ID of the authentication details to enable.</param>
	/// <param name="ct">The cancellation token.</param>
	/// <returns><see langword="true"/> if the authentication details were enabled, otherwise <see langword="false"/>.</returns>
	public async ValueTask<bool> ToggleAuthenticationDetailsAsync(Guid id, CancellationToken ct = default)
	{
		Dictionary<Guid, AuthenticationDetails> logins = await _localStorage.GetItemAsync<Dictionary<Guid, AuthenticationDetails>>("logins", ct)
		    ?? throw new InvalidOperationException("The logins store is not initialized.");
		
		AuthenticationDetails details = logins[id];
		details.Active = !details.Active;
		logins[id] = details;
		await _localStorage.SetItemAsync("logins", logins, ct);
		
		return details.Active;
	}

	/// <summary>
	/// Sets the authentication token for the specified ID.
	/// </summary>
	/// <param name="id">The ID of the authentication details to set the token for.</param>
	/// <param name="token">The authentication token to set.</param>
	public async ValueTask SetAuthenticationTokenAsync(Guid id, AuthenticationToken token)
	{
		Dictionary<Guid, AuthenticationToken> tokens = await _localStorage.GetItemAsync<Dictionary<Guid, AuthenticationToken>>("tokens")
		    ?? throw new InvalidOperationException("The tokens store is not initialized.");

		tokens[id] = token;

		await _localStorage.SetItemAsync("tokens", tokens);
	}
}