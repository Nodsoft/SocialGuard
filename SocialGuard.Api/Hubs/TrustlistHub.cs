using Microsoft.AspNetCore.SignalR;
using SocialGuard.Api.Services;
using SocialGuard.Common.Data.Models;
using SocialGuard.Common.Hubs;

namespace SocialGuard.Api.Hubs
{
	public class TrustlistHub : Hub<ITrustlistHubPush>, ITrustlistHubInvoke
	{
		private readonly ITrustlistService _service;

		public TrustlistHub(ITrustlistService service)
		{
			this._service = service;
		}


		public Task<TrustlistUser> GetUserRecord(ulong id)
		{
			return _service.FetchUserAsync(id);
		}
	}
}
