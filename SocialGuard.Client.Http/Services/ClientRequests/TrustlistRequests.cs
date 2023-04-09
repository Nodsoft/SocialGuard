using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using SocialGuard.Common.Data.Models;

namespace SocialGuard.Client.Http.Services;

public partial class SocialGuardHttpClient
{
	/// <summary>
	/// Gets the trustlist record for a user.
	/// </summary>
	/// <param name="userId">The user to get the trustlist record for.</param>
	/// <param name="ct">The cancellation token.</param>
	/// <returns>The trustlist record for the user.</returns>
	public async Task<TrustlistUser?> LookupUserAsync(ulong userId, CancellationToken ct = default) => (await LookupUsersAsync(new[] { userId }, ct)).FirstOrDefault();

	/// <summary>
	/// Gets the trustlist records for a list of users.
	/// </summary>
	/// <param name="usersId">The users to get the trustlist records for.</param>
	/// <param name="ct">The cancellation token.</param>
	/// <returns>The trustlist records for the users.</returns>
	public async Task<TrustlistUser[]> LookupUsersAsync(IEnumerable<ulong> usersId, CancellationToken ct = default)
	{
		using HttpRequestMessage request = new(HttpMethod.Get, $"/api/v3/user/{string.Join(',', usersId)}");
		using HttpResponseMessage response = await HttpClient.SendAsync(await request.WithAuthenticationHandlerAsync(this, ct: ct), ct);

		return await response.Content.ReadFromJsonAsync<TrustlistUser[]>(SerializerOptions, ct) ?? Array.Empty<TrustlistUser>();
	}

	/// <summary>
	/// Gets a list of all users known by the directory.
	/// </summary>
	/// <param name="ct">The cancellation token.</param>
	/// <returns>A list of all users known by the directory.</returns>
	public async Task<ulong[]> ListKnownUsersAsync(CancellationToken ct = default)
	{
		using HttpRequestMessage request = new(HttpMethod.Get, "/api/v3/user/list");
		using HttpResponseMessage response = await HttpClient.SendAsync(await request.WithAuthenticationHandlerAsync(this, ct: ct), ct);

		response.EnsureSuccessStatusCode();

		return await response.Content.ReadFromJsonAsync<ulong[]>(SerializerOptions, ct) ?? Array.Empty<ulong>();
	}

	/// <summary>
	/// Submits a trustlist entry for a user, or updates an existing entry made by the same emitter.
	/// </summary>
	/// <param name="userId">The user to submit the trustlist entry for.</param>
	/// <param name="entry">The trustlist entry to submit.</param>
	/// <param name="ct">The cancellation token.</param>
	/// <remarks>
	/// Authentication is required to submit a trustlist entry.
	/// </remarks>
	public async Task SubmitEntryAsync(ulong userId, TrustlistEntry entry, CancellationToken ct = default)
	{
		using HttpRequestMessage request = new(HttpMethod.Post, $"/api/v3/user/{userId}");
		request.Content = JsonContent.Create(entry, new(JsonMimeType), SerializerOptions);

		using HttpResponseMessage response = await HttpClient.SendAsync(await request.WithAuthenticationHandlerAsync(this, true, ct), ct);
		response.EnsureSuccessStatusCode();
	}
}