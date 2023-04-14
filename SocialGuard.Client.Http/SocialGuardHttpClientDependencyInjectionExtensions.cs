using SocialGuard.Client.Http.Services;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides extension methods for the <see cref="IServiceCollection"/> interface.
/// </summary>
public static class SocialGuardHttpClientDependencyInjectionExtensions
{
	/// <summary>
	/// Adds the SocialGuard HTTP client to the service collection.
	/// </summary>
	/// <param name="services">The service collection to add the client to.</param>
	/// <param name="host">The base URL of the SocialGuard Directory/API.</param>
	/// <param name="configureClient">A delegate used to configure the HTTP client.</param>
	/// <param name="clientAuthTokenProvider">A delegate used to get the authentication token for a request made by the client.</param>
	/// <returns>The service collection.</returns>
	public static IServiceCollection AddSocialGuardHttpClient(
		this IServiceCollection services,
		string host = SocialGuardHttpClient.MainHost,
		Action<HttpClient>? configureClient = null,
		SocialGuardHttpClient.AsyncClientAuthTokenProvider? clientAuthTokenProvider = null
	) {
		services.AddHttpClient<SocialGuardHttpClient>(client =>
		{
			client.BaseAddress = new(host);
			configureClient?.Invoke(client);
		});
		
		services.AddSingleton(clientAuthTokenProvider ?? DefaultClientAuthTokenProvider);
		return services;
	}
	
	/// <summary>
	/// Adds the SocialGuard HTTP client and its factory to the service collection.
	/// </summary>
	/// <param name="services">The service collection to add the client and factory to.</param>
	/// <param name="clientAuthTokenProvider">A delegate used to get the authentication token for a request made by the client.</param>
	/// <param name="clientIdAssignmentDelegate">A delegate used to assign an ID to a client. This can be used to correlate clients to internal identifiers.</param>
	/// <returns>The service collection.</returns>
	public static IServiceCollection AddSocialGuardHttpClientFactory(
		this IServiceCollection services,
		SocialGuardHttpClient.AsyncClientAuthTokenProvider? clientAuthTokenProvider = null,
		SocialGuardHttpClientFactory.ClientIdAssignmentAsyncDelegate? clientIdAssignmentDelegate = null
	) {
		services.AddHttpClient();
		
		services.AddSingleton(clientAuthTokenProvider ?? DefaultClientAuthTokenProvider);
		services.AddSingleton(clientIdAssignmentDelegate ?? DefaultClientIdAssignmentDelegate);
		
		services.AddScoped<SocialGuardHttpClientFactory>(static services => new(
			services.GetRequiredService<IHttpClientFactory>(),
			services.GetRequiredService<IServiceProvider>(),
			services.GetRequiredService<SocialGuardHttpClient.AsyncClientAuthTokenProvider>(),
			services.GetRequiredService<SocialGuardHttpClientFactory.ClientIdAssignmentAsyncDelegate>()
		));
		
		services.AddTransient<SocialGuardHttpClient>(static services => new(
			services.GetRequiredService<IHttpClientFactory>().CreateClient(),
			services
		));
		
		return services;
	}
	
	/// <summary>
	/// The default <see cref="SocialGuardHttpClient.AsyncClientAuthTokenProvider"/> implementation,
	/// which returns <see langword="null"/> for all requests.
	/// </summary>
	public static SocialGuardHttpClient.AsyncClientAuthTokenProvider DefaultClientAuthTokenProvider { get; } 
		= static (_, _, _) => ValueTask.FromResult<string?>(null);
	
	/// <summary>
	/// The default <see cref="SocialGuardHttpClientFactory.ClientIdAssignmentDelegate"/> implementation,
	/// which assigns a new <see cref="Guid"/> to each client.
	/// </summary>
	/// <remarks>
	/// This method is provided as a convenience for the <see cref="AddSocialGuardHttpClientFactory"/> method.
	/// </remarks>
	public static SocialGuardHttpClientFactory.ClientIdAssignmentAsyncDelegate DefaultClientIdAssignmentDelegate { get; } 
		= static (_, _) => ValueTask.FromResult(Guid.NewGuid());
}