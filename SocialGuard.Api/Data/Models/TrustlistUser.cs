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
		[Required, BsonRequired]
		public ulong Id { get; init; }

		public List<TrustlistEntry> Entries { get; set; }
	}
}
