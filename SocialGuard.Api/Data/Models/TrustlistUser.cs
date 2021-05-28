using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;



namespace SocialGuard.Api.Data.Models
{
	/// <summary>
	/// Represents a Trustlist User entry.
	/// </summary>
	public record TrustlistUser
	{
		[Required, BsonId, BsonRequired]
		public ulong Id { get; init; }

		[BsonRequired]
		public List<TrustlistEntry> Entries { get; set; }
	}
}
