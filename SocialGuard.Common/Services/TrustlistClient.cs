using SocialGuard.Common.Data.Models;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;


namespace SocialGuard.Common.Services;


public class TrustlistClient : RestClientBase
{
	public TrustlistClient(HttpClient httpClient) : base(httpClient) { }


	public Task<TrustlistUser?> LookupUserAsync(ulong userId) => LookupUserAsync(userId, CancellationToken.None);
	public async Task<TrustlistUser?> LookupUserAsync(ulong userId, CancellationToken ct) => (await LookupUsersAsync(new ulong[] { userId }, ct)).FirstOrDefault();


	public Task<TrustlistUser[]> LookupUsersAsync(ulong[] usersId) => LookupUsersAsync(usersId, CancellationToken.None);
	public async Task<TrustlistUser[]> LookupUsersAsync(ulong[] usersId, CancellationToken ct)
	{
		using HttpRequestMessage request = new(HttpMethod.Get, $"/api/v3/user/{string.Join(',', usersId)}");
		using HttpResponseMessage response = await HttpClient.SendAsync(request, ct);

		return await response.Content.ReadFromJsonAsync<TrustlistUser[]>(SerializerOptions, ct) is TrustlistUser[] parsed
			? parsed
			: Array.Empty<TrustlistUser>();
	}

	public Task<ulong[]> ListKnownUsersAsync() => ListKnownUsersAsync(CancellationToken.None);
	public async Task<ulong[]> ListKnownUsersAsync(CancellationToken ct)
	{
		using HttpRequestMessage request = new(HttpMethod.Get, "/api/v3/user/list");
		using HttpResponseMessage response = await HttpClient.SendAsync(request, ct);

		response.EnsureSuccessStatusCode();

		return await response.Content.ReadFromJsonAsync<ulong[]>(SerializerOptions, ct) ?? Array.Empty<ulong>();
	}


	public Task SubmitEntryAsync(ulong userId, TrustlistEntry entry, string authToken) => SubmitEntryAsync(userId, entry, authToken);
	public async Task SubmitEntryAsync(ulong userId, TrustlistEntry entry, string authToken, CancellationToken ct)
	{
		using HttpRequestMessage request = new(HttpMethod.Post, $"/api/v3/user/{userId}");
		request.Content = JsonContent.Create(entry, new(JsonMimeType), SerializerOptions);
		request.Headers.Authorization = new("bearer", authToken);

		using HttpResponseMessage response = await HttpClient.SendAsync(request, ct);

		if (!response.IsSuccessStatusCode)
		{
			throw new ApplicationException($"API returned {response.StatusCode}.");
		}
	}
}
