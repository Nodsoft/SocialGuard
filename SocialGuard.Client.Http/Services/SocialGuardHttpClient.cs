using System.Net.Http.Json;
using System.Text.Json;
using JetBrains.Annotations;

namespace SocialGuard.Client.Http.Services;

/*
 * SocialGuard HTTP Client
 * SocialGuardHttpClient.cs
 *
 * Base file for the SocialGuardHttpClient class.
 * Contains the common properties and methods used for requests supported by this client.
 *
 * NB: The request methods are in separate files, in the ClientRequests folder.
 */

/// <summary>
/// Provides a client for communicating via HTTP with a SocialGuard Directory/API.
/// </summary>
[PublicAPI]
public sealed partial class SocialGuardHttpClient
{
	internal IServiceProvider Services { get; }

	/// <summary>
	/// Defines a delegate used to provide an authentication token for each request.
	/// </summary>
	public delegate ValueTask<string?> AsyncClientAuthTokenProvider(SocialGuardHttpClient client, IServiceProvider services, CancellationToken ct = default);
	
	/// <summary>
	/// The Authentication Token provider, used to provide an authentication token for each request.
	/// </summary>
	/// <remarks>
	/// To prevent spamming the API, this should be cached in some way, and only refreshed when needed.
	/// </remarks>
	public AsyncClientAuthTokenProvider ClientAuthTokenProviderAsync { internal get; set; } 
		= static (_, _, _) => ValueTask.FromResult<string?>(null);
	
	/// <summary>
	/// Identifies the client instance.
	/// </summary>
	/// <remarks>
	/// This can be freely set downstream to match other identifiers, but should hold some form of unicity.
	/// </remarks>
	public Guid ClientId { get; init; } = Guid.NewGuid();
	
	/// <summary>
	/// The base URL of the main SocialGuard Directory/API.
	/// </summary>
	public const string MainHost = "https://api.socialguard.net";
	internal const string JsonMimeType = "application/json";

	internal HttpClient HttpClient { get; init; }

	internal static JsonSerializerOptions SerializerOptions => new()
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase
	};

	/// <summary>
	/// Initializes a new instance of the <see cref="SocialGuardHttpClient"/> class.
	/// </summary>
	/// <param name="client">The base HTTP client used to send requests. This should already be configured to use the correct base URL and authentication.</param>
	/// <param name="services">The service provider used to resolve dependencies during the authentication process.</param>
	public SocialGuardHttpClient(HttpClient client, IServiceProvider services)
	{
		Services = services;
		HttpClient = client;
		HttpClient.BaseAddress ??= new(MainHost);
	}
	
	public Uri HostUri => HttpClient.BaseAddress!;
}
