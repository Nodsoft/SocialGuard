namespace SocialGuard.Common.Data.Models;

/// <summary>
/// Represents an Emitter profile.
/// </summary>
public record Emitter
{
	[Key, Required, BsonId, BsonRequired, BsonRepresentation(BsonType.String)]
	public string Login { get; init; } = string.Empty;

	[Required]
	public EmitterType EmitterType { get; init; }

	public ulong Snowflake { get; init; }

	[Required]
	public string DisplayName { get; init; } = string.Empty;
}
