namespace SocialGuard.Client.Http.Data;

/// <summary>
/// Defines a struct for chained response results.
/// </summary>
/// <typeparam name="TResult">The type of the result being returned.</typeparam>
public readonly struct ChainedQueryResult<TResult>
{
	/// <summary>
	/// The results of the chained query's execution.
	/// </summary>
	public IReadOnlyDictionary<Uri, TResult> Results { get; }
	
	/// <summary>
	/// The exceptions thrown during the execution of the chained query.
	/// </summary>
	/// <remarks>
	/// This is a dictionary of exceptions, keyed by the client that threw the exception.
	/// </remarks>
	public IReadOnlyDictionary<Uri, Exception> Exceptions { get; }
	
	/// <summary>
	/// Initializes a new instance of the <see cref="ChainedQueryResult{TClient, TResult}"/> struct.
	/// </summary>
	/// <param name="results">The results of the chained query's execution.</param>
	/// <param name="exceptions">The exceptions thrown during the execution of the chained query.</param>
	public ChainedQueryResult(IReadOnlyDictionary<Uri, TResult> results, IReadOnlyDictionary<Uri, Exception> exceptions)
	{
		Results = results;
		Exceptions = exceptions;
	}
}