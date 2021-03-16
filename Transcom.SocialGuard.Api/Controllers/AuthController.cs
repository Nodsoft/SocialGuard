using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Transcom.SocialGuard.Api.Services.Authentication;
using Transcom.SocialGuard.Api.Services.Authentication.Models;



namespace Transcom.SocialGuard.Api.Controllers
{
	[ApiController, Route("api/[controller]")]
	public class AuthController : ControllerBase
	{
		private readonly AuthenticationService service;

		public AuthController(AuthenticationService authenticationService)
		{
			service = authenticationService;
		}


		[HttpPost, Route("login")]
		public async Task<IActionResult> Login([FromBody] LoginModel model)
		{
			AuthServiceResponse result = await service.HandleLogin(model);
			return StatusCode(result.StatusCode, result.Response);
		}

		[HttpPost, Route("register")]
		public async Task<IActionResult> Register([FromBody] RegisterModel model)
		{
			AuthServiceResponse result = await service.HandleRegister(model);
			return StatusCode(result.StatusCode, result.Response);
		}

		[HttpGet, Route("Whoami"), Authorize]
		public IActionResult Whoami()
		{
			AuthServiceResponse result = service.Whoami(HttpContext);
			return StatusCode(result.StatusCode, result.Response);
		}
	}
}
