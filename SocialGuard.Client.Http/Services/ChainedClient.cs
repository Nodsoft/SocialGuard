using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using SocialGuard.Client.Http.Data;

namespace SocialGuard.Client.Http.Services;

/// <summary>
/// Provides a class for chained clients, allowing any number of <see cref="SocialGuardHttpClient" /> instances to be chained together.
/// This is useful for querying multiple hosts providing the same service.
/// </summary>
[PublicAPI]
public sealed class ChainedClient
{
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
		List<KeyValuePair<Uri, ValueTask<TResult>>> tasks = new();
		
		await foreach (KeyValuePair<Uri, ValueTask<TResult>> task in TriggerAsyncQueries(_QueryWrapper, Clients, ct))
		{
			tasks.Add(task);
		}

		// Await all tasks. If a task throws an exception, it will be added to the exceptions dictionary.
		Dictionary<Uri, TResult> results = new();

		foreach ((Uri host, ValueTask<TResult> task) in tasks)
		{
			if (exceptions.ContainsKey(host))
			{
				continue;
			}
			
			results.Add(host, await task.ConfigureAwait(false));
		}
		
		return new(results, exceptions);
		
		async ValueTask<TResult> _QueryWrapper(SocialGuardHttpClient client)
		{
			try
			{
				return await query(client);
			}
			catch (Exception ex)
			{
				exceptions.Add(client.HostUri, ex);
				throw; // This is fine, because we're going to check for exceptions later on.
			}
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
		if (query is null)
		{
			throw new ArgumentNullException(nameof(query));
		}

		await foreach (SocialGuardHttpClient client in clients.WithCancellation(ct))
		{
			yield return new(client.HostUri, query(client));
		}
	}
    
    private static async IAsyncEnumerable<TValue> ToAsyncEnumerable<TValue>(IEnumerable<TValue> enumerable, [EnumeratorCancellation] CancellationToken ct = default)
	{
	    foreach (TValue value in enumerable)
	    {
		    yield return value;
		    await Task.Yield();
	    }
	}
}