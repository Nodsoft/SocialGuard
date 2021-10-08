using SocialGuard.Common.Data.Models;



namespace SocialGuard.Common.Data.DTOs.V2;

/// <summary>
/// Represents a Trustlist User entry.
/// </summary>
public record TrustlistUser
{
	[Required, BsonId]
	public ulong Id { get; init; }

	public DateTime EntryAt { get; set; }

	public DateTime LastEscalated { get; set; }

	[Required, Range(0, 3)]
	public byte EscalationLevel { get; set; }

	[Required, DisallowNull, MinLength(5), MaxLength(2000)]
	public string EscalationNote { get; set; } = string.Empty;

	[Required, DisallowNull]
	public Emitter? Emitter { get; set; }
}
