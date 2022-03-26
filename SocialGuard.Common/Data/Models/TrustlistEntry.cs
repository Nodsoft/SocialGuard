namespace SocialGuard.Common.Data.Models;

public record TrustlistEntry
{
	[Key, Required, BsonRequired, BsonId(IdGenerator = typeof(CombGuidGenerator))]
	public Guid Id { get; init; } = Guid.NewGuid();

	[BsonRequired]
	public DateTime EntryAt { get; init; }

	[BsonRequired]
	public DateTime LastEscalated { get; set; }

	[Required, BsonRequired, Range(0, 3)]
	public byte EscalationLevel { get; set; }

	[Required, BsonRequired, MinLength(5), MaxLength(2000)]
	public string EscalationNote { get; set; } = string.Empty;

	[BsonRequired, DisallowNull]
	public Emitter? Emitter { get; set; }
}
