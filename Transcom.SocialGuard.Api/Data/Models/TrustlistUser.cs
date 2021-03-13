using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;



namespace Transcom.SocialGuard.Api.Data.Models
{
	public record TrustlistUser
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
		public ulong Id { get; init; }

		public DateTime EntryAt { get; set; }

		public DateTime LastEscalated { get; set; }

		[Required, Range(0, 3)]
		public byte EscalationLevel { get; set; }

		[MinLength(5), MaxLength(2000)]
		public string EscalationNote { get; set; }
	}
}
