using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using SocialGuard.Common.Data.Models.Authentication;

namespace SocialGuard.Client.Http.Services;

public static class ClientExtensions
{
	public static HttpRequestMessage WithAuthentication(this HttpRequestMessage message, string token)
	{
		message.Headers.Authorization = new("Bearer", token);
		return message;
	}

	/// <summary>
	/// Adds the authentication header to the request, using the <see cref="SocialGuardHttpClient.ClientAuthTokenProviderAsync"/> delegate.
	/// </summary>
	/// <param name="message">The request to add authentication to.</param>
	/// <param name="client">The client to use to get the authentication token.</param>
	/// <param name="ensureAuthentication">Whether to throw an exception if no authentication token is provided.</param>
	/// <param name="ct">The cancellation token.</param>
	/// <returns>The request with the authentication header.</returns>
	internal static async ValueTask<HttpRequestMessage> WithAuthenticationHandlerAsync(
		this HttpRequestMessage message,
		SocialGuardHttpClient client,
		bool ensureAuthentication = false,
		CancellationToken ct = default
	)
	{
		if (await client.ClientAuthTokenProviderAsync(client, client.Services, ct) is { } token)
		{
			return message.WithAuthentication(token);
		}

		if (ensureAuthentication)
		{
			throw new InvalidOperationException("No authentication token was provided, and this request requires authentication.");
		}

		return message;
	}

	private static readonly Dictionary<Guid, TokenResult> _cachedTokens = new();

	/// <summary>
	/// Provides an authentication token provider from the given login/password pair, for use with <see cref="SocialGuardHttpClient"/>.
	/// This provider is used to get the authentication token for the request, and caches the token until it expires.
	/// </summary>
	/// <param name="login">The login to use.</param>
	/// <param name="password">The password to use.</param>
	/// <returns>The authentication token provider.</returns>
	public static SocialGuardHttpClient.AsyncClientAuthTokenProvider BuildAuthTokenProviderFromCredentials(string login, string password) =>
		async (client, _, ct) =>
		{
			if (_cachedTokens.TryGetValue(client.ClientId, out TokenResult? existingToken) && existingToken.Expiration > DateTimeOffset.UtcNow)
			{
				return existingToken.Token;
			}

			if (await client.LoginAsync(login, password, ct) is { } newToken)
			{
				_cachedTokens[client.ClientId] = newToken;
				return newToken.Token;
			}
			
			_cachedTokens.Remove(client.ClientId);
			return null;
		};
}