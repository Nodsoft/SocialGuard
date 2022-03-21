using Microsoft.AspNetCore.SignalR;
using SocialGuard.Api.Services;
using SocialGuard.Common.Data.Models;
using SocialGuard.Common.Hubs;
using System.Threading.Tasks;

namespace SocialGuard.Api.Hubs
{
	public class TrustlistHub : Hub<ITrustlistHubPush>, ITrustlistHubInvoke
	{
		private readonly ITrustlistUserService userService;

		public TrustlistHub(ITrustlistUserService userService)
		{
			this.userService = userService;
		}


		public Task<TrustlistUser> GetUserRecord(ulong id)
		{
			return userService.FetchUserAsync(id);
		}
	}
}
