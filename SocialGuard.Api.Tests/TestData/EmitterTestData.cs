using SocialGuard.Common.Data.Models;

namespace SocialGuard.Api.Tests.TestData;

/// <summary>
/// Provides test data for the <see cref="Emitter"/> class.
/// </summary>
public static class EmitterTestData
{
	/// <summary>
	/// Represents a nominal emitting user.
	/// </summary>
	public static Emitter UserEmitter => new()
	{
		Snowflake = 1,
		Login = "computerman",
		EmitterType = EmitterType.User,
		DisplayName = "The Computer Man"
	};

	/// <summary>
	/// Represents a nominal emitting server.
	/// </summary>
	public static Emitter ServerEmitter => new()
	{
		Snowflake = 2,
		Login = "computerwizzes",
		EmitterType = EmitterType.Server,
		DisplayName = "The Computer Wizards"
	};

	/// <summary>
	/// Represents a collection of users.
	/// </summary>
	public static IEnumerable<Emitter> Emitters { get; } = new[]
	{
		UserEmitter,
		ServerEmitter
	}; 
}