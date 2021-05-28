using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialGuard.Api.Data.Models;
using SocialGuard.Api.Services;
using System.Threading.Tasks;



namespace SocialGuard.Api.Controllers
{
	[ApiController, Route("api/v{version:apiVersion}/[controller]"), Authorize]
	[ApiVersion("3.0"), ApiVersion("2.0")]
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
		/// Fetches specified user's Emitter profile.
		/// </summary>
		/// <response code="200">Returns Emitter profile</response>
		/// <response code="404">If user's Emitter profile is not found.</response>    
		/// <returns>Emitter profile</returns>
		[HttpGet("{id}"), AllowAnonymous]
		[ProducesResponseType(typeof(Emitter), 200), ProducesResponseType(404)]
		public async Task<IActionResult> GetEmitterProfile(string id)
		{
			Emitter emitter = await emitterService.GetEmitterAsync(id);

			return emitter is null
				? StatusCode(404)
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
