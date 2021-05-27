using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using SocialGuard.Api.Data.Models;

namespace SocialGuard.Api.Data.DTOs.V2
{
	/// <summary>
	/// Represents a Trustlist User entry.
	/// </summary>
	public record V2_TrustlistUser
	{
		[Required]
		public ulong Id { get; init; }

		public DateTime EntryAt { get; set; }

		public DateTime LastEscalated { get; set; }

		[Required, Range(0, 3)]
		public byte EscalationLevel { get; set; }

		[Required, MinLength(5), MaxLength(2000)]
		public string EscalationNote { get; set; }

		public Emitter Emitter { get; set; }
	}
}
