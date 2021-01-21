using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Natsecure.SocialGuard.Api.Data.Models
{
	public enum EscalationLevel : byte
	{
		Neutral = 0,
		Suspicious = 1,
		Untrusted = 2,
		Blacklisted = 3
	}
}
