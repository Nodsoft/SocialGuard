using Microsoft.AspNetCore.SignalR;
using SocialGuard.Api.Services;
using SocialGuard.Common.Data.Models;
using SocialGuard.Common.Hubs;

namespace SocialGuard.Api.Hubs
{
	public class TrustlistHub : Hub<ITrustlistHubPush>, ITrustlistHubInvoke
	{
		private readonly TrustlistUserService userService;

		public TrustlistHub(TrustlistUserService userService)
		{
			this.userService = userService;
		}


		public Task<TrustlistUser> GetUserRecord(ulong id)
		{
			return userService.FetchUserAsync(id);
		}
	}
}
