using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Transcom.SocialGuard.Api.Data.Models;
using Transcom.SocialGuard.Api.Services;
using Transcom.SocialGuard.Api.Services.Authentication;



namespace Transcom.SocialGuard.Api.Controllers
{
	[ApiController, Route("api/[controller]"), Authorize(Roles = UserRole.Emitter)]
	public class EmitterController : ControllerBase
	{
		private readonly EmitterService emitterService;

		public EmitterController(EmitterService emitterService)
		{
			this.emitterService = emitterService;
		}

		/// <summary>
		/// Fetches currently logged-in user's Emitter profile.
		/// </summary>
		/// <response code="200">Returns Emitter profile</response>
		/// <response code="204">If user's Emitter profile is not set up.</response>    
		/// <returns>Emitter profile</returns>
		[HttpGet, ProducesResponseType(typeof(Emitter), 200), ProducesResponseType(204)]
		public async Task<IActionResult> GetEmitterProfile()
		{
			Emitter emitter = await emitterService.GetEmitterAsync(HttpContext);
			return emitter is null 
				? StatusCode(204) 
				: StatusCode(200, emitter);
		}

		/// <summary>
		/// Creates or Updates currently logged-in user's Emitter profile.
		/// </summary>
		/// <param name="emitter">New or updated Emitter info</param>
		/// <response code="200">Emitter profile was successfully created or updated.</response>
		/// <returns></returns>
		[HttpPost, ProducesResponseType(200)]
		public async Task<IActionResult> UpdateEmitterProfile([FromBody] Emitter emitter) 
		{
			await emitterService.CreateOrUpdateEmitterSelfAsync(emitter, HttpContext);
			return StatusCode(200);
		}
	}
}
