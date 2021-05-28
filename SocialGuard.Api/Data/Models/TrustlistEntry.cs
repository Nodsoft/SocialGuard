using System;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace SocialGuard.Api.Data.Models
{
	public record TrustlistEntry
	{
		[BsonRequired, BsonId(IdGenerator = typeof(StringObjectIdGenerator)), BsonRepresentation(BsonType.ObjectId)]
		public string Id { get; init; }

		[BsonRequired]
		public DateTime EntryAt { get; init; }

		[BsonRequired]
		public DateTime LastEscalated { get; set; }

		[Required, BsonRequired, Range(0, 3)]
		public byte EscalationLevel { get; set; }

		[Required, BsonRequired, MinLength(5), MaxLength(2000)]
		public string EscalationNote { get; set; }

		[BsonRequired]
		public Emitter Emitter { get; set; }
	}
}
