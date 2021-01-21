using Microsoft.AspNetCore.Mvc;
using Natsecure.SocialGuard.Api.Data.Models;
using Natsecure.SocialGuard.Api.Services;
using Natsecure.SocialGuard.Api.Services.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;



namespace Natsecure.SocialGuard.Api.Controllers
{
	[ApiController, Route("api/[controller]")]
	public class UserController : ControllerBase
	{
		private readonly TrustlistUserService service;

		public UserController(TrustlistUserService service)
		{
			this.service = service;
		}


		[HttpGet("list")]
		public IActionResult ListUsersIds()
		{
			IEnumerable<ulong> users = service.ListUserIds();
			return users.Any()
				? StatusCode(200, users)
				: StatusCode(204);
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> FetchUser(ulong id) 
		{
			TrustlistUser user = await service.FetchUserAsync(id);
			return StatusCode(user is not null ? 200 : 404, user);
		}

		[HttpPost, AccessKey(AccessScopes.Insert)]
		public async Task<IActionResult> NewUserRecord([FromBody] TrustlistUser userRecord) 
		{
			try
			{
				await service.InsertNewUserAsync(userRecord);
			}
			catch (ArgumentOutOfRangeException)
			{
				return StatusCode(409, "User already exists in DB. Use PUT request to update his Escalation Level.");
			}

			return StatusCode(201);
		}

		[HttpPut, AccessKey(AccessScopes.Escalate)]
		public async Task<IActionResult> EscalateUserRecord([FromBody] TrustlistUser userRecord) 
		{
			try
			{
				await service.EscalateUserAsync(userRecord);
			}
			catch (ArgumentOutOfRangeException)
			{
				return StatusCode(404, "No user found in DB.");
			}

			return StatusCode(202);
		}

		[HttpDelete("{id}"), AccessKey(AccessScopes.Delete)]
		public async Task<IActionResult> DeleteUserRecord(ulong id) 
		{
			await service.DeleteUserAsync(id);
			return StatusCode(200);
		}
	}
}
