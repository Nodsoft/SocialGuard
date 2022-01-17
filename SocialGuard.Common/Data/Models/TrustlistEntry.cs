namespace SocialGuard.Common.Data.Models;

public record TrustlistEntry
{
	[BsonRequired, BsonId(IdGenerator = typeof(StringObjectIdGenerator)), BsonRepresentation(BsonType.ObjectId)]
	public string Id { get; init; } = string.Empty;

	[BsonRequired]
	public DateTime EntryAt { get; init; }

	[BsonRequired]
	public DateTime LastEscalated { get; set; }

	[Required, BsonRequired, Range(0, 3)]
	public byte EscalationLevel { get; set; }

	[Required, DisallowNull, BsonRequired, MinLength(5), MaxLength(2000)]
	public string EscalationNote { get; set; } = string.Empty;

	[BsonRequired, DisallowNull]
	public Emitter? Emitter { get; set; }
}
