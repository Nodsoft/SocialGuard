using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Polly;
using Polly.Extensions.Http;
using Polly.Fallback;
using Polly.Wrap;
using SocialGuard.Client.Http.Data;

namespace SocialGuard.Client.Http.Services;

/// <summary>
/// Provides a class for chained clients, allowing any number of <see cref="SocialGuardHttpClient" /> instances to be chained together.
/// This is useful for querying multiple hosts providing the same service.
/// </summary>
[PublicAPI]
public sealed class ChainedClient
{
	private static readonly ConcurrentDictionary<Guid, IPolicyWrap> Circuits = new(); 

	/// <summary>
    /// Initializes a new instance of the <see cref="ChainedClient"/> class.
    /// </summary>
    /// <param name="clients">The clients to be chained.</param>
    public ChainedClient(IEnumerable<SocialGuardHttpClient> clients)
	{
		Clients = ToAsyncEnumerable(clients);
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="ChainedClient"/> class.
	/// </summary>
	/// <param name="clients">The clients to be chained.</param>
	public ChainedClient(IAsyncEnumerable<SocialGuardHttpClient> clients)
	{
		Clients = clients;
	}

    /// <summary>
    /// Gets the clients being chained.
    /// </summary>
    public IAsyncEnumerable<SocialGuardHttpClient> Clients { get; }

    /// <summary>
    /// Executes a query against all clients in the chain.
    /// </summary>
    /// <typeparam name="TResult">The type of the query return value.</typeparam>
    /// <param name="query">The query to execute.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    public async ValueTask<ChainedQueryResult<TResult>> ExecuteQueryAsync<TResult>(Func<SocialGuardHttpClient, ValueTask<TResult>> query, CancellationToken ct = default)
	{
		Dictionary<Uri, Exception> exceptions = new();

		// This policy will swallow all exceptions and return a dictionary of exceptions.

		var policy = Policy<TResult>
			.Handle<Exception>()
			.FallbackAsync(default(TResult)!, static (result, ctx) =>
			{
				if (ctx["hostUri"] is Uri hostUri && ctx["exceptions"] is Dictionary<Uri, Exception> exceptions && result.Exception is not null)
				{
					exceptions.Add(hostUri, result.Exception);
				}

				return Task.CompletedTask;
			});

		// Await all tasks. If a task throws an exception, it will be added to the exceptions dictionary.
		Dictionary<Uri, TResult> results = new();

		await foreach ((Uri host, ValueTask<TResult> task) in TriggerAsyncQueries(_QueryWrapper, Clients, ct))
		{
			TResult result = await task;
			
			if (exceptions.ContainsKey(host))
			{
				continue;
			}
			
			results.Add(host, result);
		}
		
		return new(results, exceptions);
		
		
		
		async ValueTask<TResult> _QueryWrapper(SocialGuardHttpClient client)
		{
			Console.WriteLine($"{nameof(_QueryWrapper)}: Querying {client.HostUri} ({client.ClientId})...");

			return await policy.ExecuteAsync(async ctx => await query((SocialGuardHttpClient)ctx["client"]), new()
			{
				{ "client" , client },
				{ "hostUri", client.HostUri }
			});
		}
	}
    
    
    /// <summary>
    /// Triggers asynchronous queries against a collection of clients, returning a collection of tasks.
    /// </summary>
    private static async IAsyncEnumerable<KeyValuePair<Uri, ValueTask<TReturn>>> TriggerAsyncQueries<TReturn>(
	    Func<SocialGuardHttpClient, ValueTask<TReturn>> query, 
	    IAsyncEnumerable<SocialGuardHttpClient> clients,
	    [EnumeratorCancellation] CancellationToken ct = default
    ) {
		await foreach (SocialGuardHttpClient client in clients.WithCancellation(ct))
		{
			Debug.WriteLine($"{nameof(TriggerAsyncQueries)}: Querying {client.HostUri} ({client.ClientId})...");
			yield return new(client.HostUri, query(client));
		}
	}
    
    private static async IAsyncEnumerable<TValue> ToAsyncEnumerable<TValue>(IEnumerable<TValue> enumerable)
	{
	    foreach (TValue value in enumerable)
	    {
		    yield return value;
		    await Task.Yield();
	    }
	}
}