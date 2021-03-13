using System;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson.Serialization.Attributes;

namespace Transcom.SocialGuard.Api.Data.Models
{
	public record TrustlistUser
	{
		[Required, BsonRequired]
		public ulong Id { get; init; }

		public DateTime EntryAt { get; set; }

		public DateTime LastEscalated { get; set; }

		[Required, BsonRequired, Range(0, 3)]
		public byte EscalationLevel { get; set; }

		[Required, BsonRequired, MinLength(5), MaxLength(2000)]
		public string EscalationNote { get; set; }
	}
}
