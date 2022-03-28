using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using SocialGuard.Api.Services.Authentication;
using SocialGuard.Common.Data.Models.Authentication;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;



namespace SocialGuard.Api.Controllers
{
	[ApiController, Route("api/v{version:apiVersion}/[controller]")]
	[ApiVersion("3.0"), ApiVersion("2.0")]
	public class AuthController : ControllerBase
	{
		private readonly AuthenticationService _service;

		public AuthController(AuthenticationService authenticationService)
		{
			_service = authenticationService;
		}

		/// <summary>
		/// Provides JWT Authentication token for specified credentials.
		/// </summary>
		/// <response code="200">Authentication success response with JWT Token</response>
		/// <response code="401">Authentication failure response</response>   
		/// <param name="model">Login credentials</param>
		/// <returns>Auth response</returns>
		[HttpPost("login")]
		[ProducesResponseType(200), ProducesResponseType(401)]
		public async Task<IActionResult> Login([FromBody] LoginModel model)
		{
			AuthServiceResponse result = await _service.HandleLogin(model);
			return StatusCode(result.StatusCode, result.Response);
		}

		/// <summary>
		/// Creates user with specified credentials.
		/// </summary>
		/// <response code="201">Registration success response</response>
		/// <param name="model">User signup details</param>
		/// <returns>Registration status response</returns>
		[HttpPost("register")]
		[ProducesResponseType(201)]
		public async Task<IActionResult> Register([FromBody] RegisterModel model)
		{
			AuthServiceResponse result = await _service.HandleRegister(model);
			return StatusCode(result.StatusCode, result.Response);
		}


		/// <summary>
		/// Ftches full status/info for current Authentication method.
		/// </summary>
		/// <response code="200">Returns full authentication info</response>
		/// <response code="401">Authentication failure response</response>   
		/// <returns>Full Auth info</returns>
		[HttpGet("whoami"), Authorize]
		[ProducesResponseType(200)]
		public IActionResult Whoami()
		{
			return StatusCode(200, AuthenticationService.Whoami(HttpContext));
		}
	}
}
