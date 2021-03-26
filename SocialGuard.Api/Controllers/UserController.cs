using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialGuard.Api.Data.Models;
using SocialGuard.Api.Services;
using SocialGuard.Api.Services.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SocialGuard.Api.Controllers
{

	[ApiController, Route("api/[controller]")]
	public class UserController : ControllerBase
	{
		private readonly TrustlistUserService trustlistService;
		private readonly EmitterService emitterService;

		public UserController(TrustlistUserService trustlistService, EmitterService emitterService)
		{
			this.trustlistService = trustlistService;
			this.emitterService = emitterService;
		}

		/// <summary>
		/// Enumerates all users present in the Trustlist.
		/// </summary>
		/// <response code="200">Returns List</response>
		/// <response code="204">If Trustlist is empty</response>    
		/// <returns>List of user IDs</returns>
		[HttpGet("list"), ProducesResponseType(typeof(IEnumerable<ulong>), 200), ProducesResponseType(204)]
		public IActionResult ListUsersIds()
		{
			IEnumerable<ulong> users = trustlistService.ListUserIds();
			return users.Any()
				? StatusCode(200, users)
				: StatusCode(204);
		}


		/// <summary>
		/// Gets Trustlist record on user with specified Trustlist
		/// </summary>
		/// <param name="id">ID of user</param>
		/// <response code="200">Returns record</response>
		/// <response code="404">If user ID is not found in DB</response>    
		/// <returns>Trustlist info</returns>
		[HttpGet("{id}"), ProducesResponseType(typeof(TrustlistUser), 200), ProducesResponseType(404)]
		public async Task<IActionResult> FetchUser(ulong id)
		{
			TrustlistUser user = await trustlistService.FetchUserAsync(id);
			return StatusCode(user is not null ? 200 : 404, user);
		}

		[HttpPost("for"), ProducesResponseType(typeof(IEnumerable<TrustlistUser>), 200), ProducesResponseType(204)]
		public async Task<IActionResult> FetchUsers([FromBody] ulong[] ids)
		{
			IEnumerable<TrustlistUser> users = await trustlistService.FetchUsersAsync(ids);
			return users.Any()
				? StatusCode(200, users)
				: StatusCode(204);
		}


		/// <summary>
		/// Inserts record into Trustlist
		/// </summary>
		/// <param name="userRecord">User record to insert</param>
		/// <response code="201">User was created</response>
		/// <response code="409">If User record already exists</response> 
		[HttpPost, Authorize(Roles = UserRole.Emitter)]
		[ProducesResponseType(201), ProducesResponseType(409)]
		public async Task<IActionResult> InsertUserRecord([FromBody] TrustlistUser userRecord)
		{
			Emitter emitter = await emitterService.GetEmitterAsync(HttpContext);

			if (emitter is null)
			{
				return StatusCode(401, "No emitter profile set for logged-in user. Please setup emitter first.");
			}

			try
			{
				await trustlistService.InsertNewUserAsync(userRecord, emitter);
			}
			catch (ArgumentOutOfRangeException)
			{
				return StatusCode(409, "User already exists in DB. Use PUT request to update his Escalation Level.");
			}

			return StatusCode(201);
		}

		/// <summary>
		/// Escalates existing record in Trustlist
		/// </summary>
		/// <param name="userRecord">User record to escalate</param>
		/// <response code="202">Record escalation request was accepted</response>
		/// <response code="404">If user ID is not found in DB</response>
		[HttpPut, Authorize(Roles = UserRole.Emitter)]
		[ProducesResponseType(202), ProducesResponseType(404)]
		public async Task<IActionResult> EscalateUserRecord([FromBody] TrustlistUser userRecord)
		{
			Emitter emitter = await emitterService.GetEmitterAsync(HttpContext);

			if (emitter is null)
			{
				return StatusCode(401, "No emitter profile set for logged-in user. Please setup emitter first.");
			}

			try
			{
				await trustlistService.EscalateUserAsync(userRecord, emitter);
			}
			catch (ArgumentOutOfRangeException)
			{
				return StatusCode(404, "No user found in DB.");
			}

			return StatusCode(202);
		}


		/// <summary>
		/// Wipes User record from Trustlist
		/// </summary>
		/// <param name="id">ID of User to wipe</param>
		/// <response code="200">Record was wiped (if any)</response>
		[HttpDelete("{id}"), Authorize(Roles = UserRole.Admin)]
		public async Task<IActionResult> DeleteUserRecord(ulong id)
		{
			await trustlistService.DeleteUserRecordAsync(id);
			return StatusCode(200);
		}
	}
}
