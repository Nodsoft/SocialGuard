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

		public byte EscalationLevel { get; set; }

		public string EscalationNote { get; set; }
	}
}
