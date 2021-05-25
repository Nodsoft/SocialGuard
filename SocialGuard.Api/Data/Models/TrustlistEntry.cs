using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SocialGuard.Api.Data.Models
{
	public record TrustlistEntry
	{
		[BsonRequired, BsonRepresentation(BsonType.ObjectId)]
		public ObjectId Id { get; init; }

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
