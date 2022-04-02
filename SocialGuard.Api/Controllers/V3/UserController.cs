using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialGuard.Common.Data.Models;
using SocialGuard.Api.Services;
using SocialGuard.Api.Services.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;



namespace SocialGuard.Api.Controllers.V3
{
	public record TrustlistImportModel(IEnumerable<TrustlistUser> Entries, Emitter Emitter, DateTime Timestamp);


	[ApiController, Route("api/v{version:apiVersion}/[controller]"), ApiVersion("3.0")]
	public class UserController : ControllerBase
	{
		private readonly ITrustlistService trustlistService;
		private readonly IEmitterService emitterService;

		public UserController(ITrustlistService trustlistService, IEmitterService emitterService)
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
		/// Gets Trustlist record on user with specified ID
		/// </summary>
		/// <param name="ids">IDs of user</param>
		/// <response code="200">Returns record</response>
		/// <response code="204">If user ID is not found in DB</response>    
		/// <returns>Trustlist info</returns>
		[HttpGet("{ids}"), ProducesResponseType(typeof(TrustlistUser[]), 200), ProducesResponseType(204)]
		public async Task<IActionResult> FetchUsers([FromRoute] ulong[] ids)
		{
			if (ids is null)
			{
				return BadRequest();
			}

			IEnumerable<TrustlistUser> users = await trustlistService.FetchUsersAsync(ids);
			return StatusCode(users is not null ? 200 : 204, users);
		}

		/// <summary>
		/// Wipes User record from Trustlist
		/// </summary>
		/// <param name="id">ID of User to wipe</param>
		/// <response code="200">Record was wiped (if any)</response>
		[HttpDelete("{id}"), Authorize(Roles = UserRole.Admin)]
		public async Task<IActionResult> DeleteUserRecord(ulong id)
		{
			if (await emitterService.GetEmitterAsync(HttpContext) is Emitter emitter)
			{
				await trustlistService.DeleteUserEntryAsync(id, emitter);
				return StatusCode(200);
			}

			return StatusCode(401, "No emitter profile set for logged-in user. Please setup emitter first.");
		}

		/// <summary>
		/// Inserts or updates record into Trustlist
		/// </summary>
		/// <param name="id">ID of Trustlist user</param>
		/// <param name="entry">Trustlist Entry to submit</param>
		/// <response code="200">Entry was updated</response>
		/// <response code="201">Entry was created</response>
		[HttpPost("{id}"), Authorize(Roles = UserRole.Emitter)]
		public async Task<IActionResult> SubmitUserRecord([FromRoute] ulong id, [FromBody] TrustlistEntry entry)
		{
			if (await emitterService.GetEmitterAsync(HttpContext) is Emitter emitter)
			{
				if ((await trustlistService.FetchUserAsync(id))?.Entries?.Any(e => e.Emitter.Login == emitter.Login) is true)
				{
					await trustlistService.UpdateUserEntryAsync(id, entry, emitter);
					return StatusCode(200);
				}
				else
				{
					await trustlistService.InsertNewUserEntryAsync(id, entry, emitter);
					return StatusCode(201);
				}
			}

			return StatusCode(401, "No emitter profile set for logged-in user. Please setup emitter first.");
		}
	}
}
