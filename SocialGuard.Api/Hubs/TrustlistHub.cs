using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SocialGuard.Api.Services;

namespace SocialGuard.Api.Hubs
{
	public class TrustlistHub : Hub
	{
		private readonly TrustlistUserService userService;

		public TrustlistHub(TrustlistUserService userService)
		{
			this.userService = userService;
		}


		public async Task GetUserRecord(ulong id)
		{
			await Clients.Caller.SendAsync("ResultUserRecord", await userService.FetchUserAsync(id));
		}
	}
}
