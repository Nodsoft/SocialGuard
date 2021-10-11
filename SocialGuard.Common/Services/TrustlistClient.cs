using SocialGuard.Common.Data.Models;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;

namespace SocialGuard.Common.Services;

public class TrustlistClient : RestClientBase
{
	public TrustlistClient(HttpClient httpClient) : base(httpClient)
	{
		HttpClient.BaseAddress ??= new Uri(RestClientExtensions.MainHost);
	}

	public async Task<TrustlistUser?> LookupUserAsync(ulong userId) => (await LookupUsersAsync(new ulong[] { userId }))?[0];

	public async Task<TrustlistUser[]> LookupUsersAsync(ulong[] usersId)
	{
		using HttpRequestMessage request = new(HttpMethod.Get, $"/api/v3/user/{string.Join(',', usersId)}");
		using HttpResponseMessage response = await HttpClient.SendAsync(request);

		return await ParseResponseFullAsync<TrustlistUser[]>(response) is TrustlistUser[] parsed and { Length: not 0 } ? parsed	: Array.Empty<TrustlistUser>();
	}

	public async Task<ulong[]> ListKnownUsersAsync()
	{
		using HttpRequestMessage request = new(HttpMethod.Get, "/api/v3/user/list");
		using HttpResponseMessage response = await HttpClient.SendAsync(request);

		return await ParseResponseFullAsync<ulong[]>(response) ?? Array.Empty<ulong>();
	}

	public async Task SubmitEntryAsync(ulong userId, TrustlistEntry entry, string authToken)
	{
		using HttpRequestMessage request = new(HttpMethod.Post, $"/api/v3/user/{userId}");
		request.Content = JsonContent.Create(entry, new(JsonMimeType), SerializerOptions);
		request.Headers.Authorization = new("bearer", authToken);

		using HttpResponseMessage response = await HttpClient.SendAsync(request);

		if (!response.IsSuccessStatusCode)
		{
			throw new ApplicationException($"API returned {response.StatusCode}.");
		}
	}
}
