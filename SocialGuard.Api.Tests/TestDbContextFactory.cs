using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace SocialGuard.Api.Tests;

/// <summary>
/// Provides creation utilities for <see cref="DbContext"/> instances
/// </summary>
public class TestDbContextFactory
{
	/// <summary>
	/// Creates new options object for an SQLite in-memory backed DbContext.
	/// </summary>
	/// <typeparam name="TContext"></typeparam>
	/// <returns></returns>
	public static DbContextOptions<TContext> GetSqliteInMemoryContextOptions<TContext>() where TContext : DbContext
	{
		SqliteConnection connection = new("Filename=:memory:");
		connection.Open();
		
		return new DbContextOptionsBuilder<TContext>()
			.UseSqlite(connection)
			.EnableDetailedErrors()
			.EnableSensitiveDataLogging()
			.Options;
	}
}