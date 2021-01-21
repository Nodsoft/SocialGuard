using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Natsecure.SocialGuard.Api.Data.Models
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
