using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using SocialGuard.Common.Data.Models;

namespace SocialGuard.Api.DbMigrator.Models.Mongo;

public record MongoTrustlistEntry
{
	/// <remarks>
	///	TrustlistEntry ID is based on Mongo's ObjectId.
	/// </remarks>
	[BsonRequired, BsonId(IdGenerator = typeof(StringObjectIdGenerator)), BsonRepresentation(BsonType.ObjectId)]
	public string Id { get; init; } = string.Empty;

	[BsonRequired]
	public DateTime EntryAt { get; init; }

	[BsonRequired]
	public DateTime LastEscalated { get; set; }

	[Required, BsonRequired, Range(0, 3)]
	public byte EscalationLevel { get; set; }

	[Required, BsonRequired, MinLength(5), MaxLength(2000)]
	public string EscalationNote { get; set; } = string.Empty;

	[BsonRequired]
	public Emitter? Emitter { get; set; }
}