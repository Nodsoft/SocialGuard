using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace SocialGuard.Common.Services;

public abstract class RestClientBase
{
	protected const string JsonMimeType = "application/json";

	protected HttpClient HttpClient { get; init; }

	protected static JsonSerializerOptions SerializerOptions => new()
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase
	};

	protected RestClientBase(HttpClient client)
	{
		HttpClient = client;
		HttpClient.BaseAddress ??= new Uri(RestClientExtensions.MainHost);
	}

	public virtual void SetBaseUri(Uri uri) => HttpClient.BaseAddress = uri;

	public static Task<TData?> ParseResponseFullAsync<TData>(HttpResponseMessage response) => response.Content.ReadFromJsonAsync<TData>(SerializerOptions);
}
