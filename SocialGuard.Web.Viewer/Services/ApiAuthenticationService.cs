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
		_ = InitializeAsync();
	}
	
	

	/// <summary>
	/// Determines if the user is authenticated to the SocialGuard API.
	/// </summary>
	/// <see cref=""/>
	/// <returns><see langword="true"/> if the user is authenticated, otherwise <see langword="false"/>.</returns>
	public async ValueTask<bool> IsAuthenticatedAsync(Uri host)
	{
		AuthenticationToken[]? tokens = await _localStorage.GetItemAsync<AuthenticationToken[]>("tokens");
		return tokens.FirstOrDefault(t => host == t.Host) is { } token && token.Expiration > DateTime.UtcNow;
	}

	/// <summary>
	/// Gets the authentication token for the SocialGuard API.
	/// </summary>
	/// <param name="performLogin">If true, a login will be performed if the user is not authenticated, or the token is expired.</param>
	/// <returns>The authentication token, if present in store</returns>
	public async ValueTask<AuthenticationToken?> GetAuthenticationTokenAsync(bool performLogin = true)
		=> await _localStorage.GetItemAsync<AuthenticationToken>("tokens");

	/// <summary>
	/// Gets all known authentication details API hosts, and their associated logins (if any).
	/// </summary>
	/// <returns>A collection of authentication details.</returns>
	/// <remarks>
	/// This method does not check if the authentication details are valid.
	/// These credentials may be updated, expired, or removed at any time.
	/// </remarks>
	public async ValueTask<Dictionary<Guid, AuthenticationDetails>> GetKnownAuthenticationDetailsAsync() =>
		new(from detail
			in await _localStorage.GetItemAsync<Dictionary<Guid, AuthenticationDetails>>("logins") ?? new Dictionary<Guid, AuthenticationDetails>()
			select new KeyValuePair<Guid, AuthenticationDetails>(detail.Key, detail.Value with { Id = detail.Key, Password = null }));
	
	internal async Task InitializeAsync(CancellationToken ct = default)
	{
		// Initialize the tokens and logins stores, if they don't exist.
		
		if (await _localStorage.GetItemAsync<Dictionary<Guid, AuthenticationDetails>>("tokens", ct) is null)
		{
			await _localStorage.SetItemAsync("tokens", new Dictionary<Guid, AuthenticationDetails>(), ct);
		}
		
		if (await _localStorage.GetItemAsync<Dictionary<Guid, AuthenticationDetails>>("logins", ct) is null)
		{
			AuthenticationDetails defaultAuthDetails = new("NSYS SocialGuard", new("https://api.socialguard.net"));
			
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
	/// <returns>The ID of the upserted authentication details.</returns>
	public async ValueTask<Guid> UpsertAuthenticationDetailsAsync(AuthenticationDetails details)
	{
		Dictionary<Guid, AuthenticationDetails> logins = await _localStorage.GetItemAsync<Dictionary<Guid, AuthenticationDetails>>("logins")
		    ?? throw new InvalidOperationException("The logins store is not initialized.");
		
		logins[details.Id] = details with { Password = await EncryptPasswordAsync(details.Password) };
		await _localStorage.SetItemAsync("logins", logins);
		return details.Id;
	}
	
	/// <summary>
	/// Removes the authentication details with the specified ID from the local storage.
	/// </summary>
	/// <param name="id">The ID of the authentication details to remove.</param>
	/// <returns><see langword="true"/> if the authentication details were removed, otherwise <see langword="false"/>.</returns>
	public async ValueTask<bool> RemoveAuthenticationDetailsAsync(Guid id)
	{
		Dictionary<Guid, AuthenticationDetails> logins = await _localStorage.GetItemAsync<Dictionary<Guid, AuthenticationDetails>>("logins")
		    ?? throw new InvalidOperationException("The logins store is not initialized.");
		
		bool removed = logins.Remove(id);
		await _localStorage.SetItemAsync("logins", logins);
		return removed;
	}

	/// <summary>
	/// Encrypts the specified password using the local key.
	/// </summary>
	/// <param name="password">The password to encrypt.</param>
	/// <returns>The encrypted password.</returns>
	/// <remarks>
	/// This method must be supported in the browser, therefore AES-CBC is the only option.
	/// </remarks>
	[method: Pure]
	[return: NotNullIfNotNull(nameof(password))]
	private async ValueTask<string?> EncryptPasswordAsync(string? password)
	{
		if (password is null or "")
		{
			return null;
		}
		
		return await _js.InvokeAsync<string>(/*lang=javascript*/@"encryptAsync", password);
	}
	
	/// <summary>
	/// Decrypts the specified AES-CBC encrypted password using the local key.
	/// </summary>
	/// <param name="password">The password to decrypt.</param>
	/// <returns>The decrypted password.</returns>
	/// <remarks>
	/// This method must be supported in the browser, therefore AES-CBC is the only option.
	/// </remarks>
	[method: Pure]
	[return: NotNullIfNotNull(nameof(password))]
	public async ValueTask<string?> DecryptPasswordAsync(string? password)
	{
		if (password is null or "")
		{
			return null;
		}
		
		return await _js.InvokeAsync<string>(/*lang=javascript*/@"decryptAsync", password);
	}
}