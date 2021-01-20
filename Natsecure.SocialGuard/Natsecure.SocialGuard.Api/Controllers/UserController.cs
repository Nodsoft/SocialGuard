using Microsoft.AspNetCore.Mvc;
using Natsecure.SocialGuard.Api.Data.Models;
using Natsecure.SocialGuard.Api.Services;
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
		public IActionResult ListUsersids()
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

		[HttpPost]
		public async Task<IActionResult> NewUserRecord([FromBody] TrustlistUser userRecord) 
		{
			await service.InsertNewUserAsync(userRecord);
			return StatusCode(201);
		}

		[HttpPut]
		public async Task<IActionResult> EscalateUserRecord([FromBody] TrustlistUser userRecord) 
		{
			try
			{
				await service.EscalateUserAsync(userRecord);
			}
			catch (ArgumentOutOfRangeException)
			{
				return StatusCode(404);
			}

			return StatusCode(202);
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteUserRecord(ulong id) 
		{
			await service.DeleteUserAsync(id);
			return StatusCode(200);
		}
	}
}
