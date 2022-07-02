using System.Net.Http.Json;
using System.Text.Json;

namespace SocialGuard.Client.Http.Services;

public abstract class RestClientBase
{
	public const string MainHost = "https://api.socialguard.net";
	protected const string JsonMimeType = "application/json";

	protected HttpClient HttpClient { get; init; }

	protected static JsonSerializerOptions SerializerOptions => new()
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase
	};

	protected RestClientBase(HttpClient client)
	{
		HttpClient = client;
		HttpClient.BaseAddress ??= new Uri(MainHost);
	}

	public virtual void SetHostUri(Uri uri) => HttpClient.BaseAddress = uri;
	public virtual Uri GetHostUri() => HttpClient.BaseAddress!;

	public static Task<TData?> ParseResponseFullAsync<TData>(HttpResponseMessage response) => response.Content.ReadFromJsonAsync<TData>(SerializerOptions);
}
