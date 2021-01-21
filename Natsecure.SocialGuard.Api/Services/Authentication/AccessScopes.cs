using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Natsecure.SocialGuard.Api.Services.Authentication
{
	public static class AccessScopes
	{
		public const string
			Read = "0",
			Insert = "1",
			Escalate = "2",
			Delete = "3";
	}
}
