namespace SocialGuard.Common.Data.Models;

/// <summary>
/// Represents a Trustlist User entry.
/// </summary>
public record TrustlistUser
{
	[Key, Required, BsonId, BsonRequired]
	public ulong Id { get; init; }

	[Required, BsonRequired, DisallowNull]
	public List<TrustlistEntry>? Entries { get; set; }
}
