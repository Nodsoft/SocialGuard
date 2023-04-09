using Microsoft.Extensions.DependencyInjection;

namespace SocialGuard.Client.Http.Services;

/// <summary>
/// Provides a factory for creating <see cref="SocialGuardHttpClient"/> instances.
/// </summary>
public sealed class SocialGuardHttpClientFactory
{
	private readonly IHttpClientFactory _httpClientFactory;
	private readonly IServiceProvider _services;
	private readonly SocialGuardHttpClient.AsyncClientAuthTokenProvider _clientAuthTokenProvider;
	private readonly ClientIdAssignmentDelegate _clientIdAssignmentDelegate;

	/// <summary>
	/// A delegate used to assign an ID to a client.
	/// </summary>
	public delegate Guid ClientIdAssignmentDelegate(Uri host, IServiceProvider services);
	
	/// <summary>
	/// Initializes a new instance of the <see cref="SocialGuardHttpClientFactory"/> class.
	/// </summary>
	public SocialGuardHttpClientFactory(
		IHttpClientFactory httpClientFactory, 
		IServiceProvider services, 
		SocialGuardHttpClient.AsyncClientAuthTokenProvider clientAuthTokenProvider,
		ClientIdAssignmentDelegate clientIdAssignmentDelegate
	) {
		_httpClientFactory = httpClientFactory;
		_services = services;
		_clientAuthTokenProvider = clientAuthTokenProvider;
		_clientIdAssignmentDelegate = clientIdAssignmentDelegate;
	}
	
	/// <summary>
	/// Creates a series of <see cref="SocialGuardHttpClient"/> instances.
	/// </summary>
	/// <param name="hosts">The hosts to create clients for.</param>
	/// <param name="clientIdAssignmentDelegate">A delegate used to assign an ID to a client. This can be used to correlate clients to internal identifiers.</param>
	/// <returns>A series of <see cref="SocialGuardHttpClient"/> instances.</returns>
	public IEnumerable<SocialGuardHttpClient> CreateClients(
		IEnumerable<Uri> hosts
	) {
		using IServiceScope scope = _services.CreateScope();
		
		foreach (Uri host in hosts)
		{
			HttpClient httpClient = _httpClientFactory.CreateClient();
			httpClient.BaseAddress = host;

			SocialGuardHttpClient client = new(httpClient, scope.ServiceProvider)
			{
				ClientId = _clientIdAssignmentDelegate(host, scope.ServiceProvider),
				ClientAuthTokenProviderAsync = _clientAuthTokenProvider
			};
			yield return client;
		}
	}
}