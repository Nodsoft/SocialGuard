using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Natsecure.SocialGuard.Api.Controllers
{
	[ApiController, Route("api/[controller]")]
	public class UserController : ControllerBase
	{
		[HttpGet("list")]
		public async Task<IActionResult> ListUsers() => throw new NotImplementedException();

		[HttpGet("{id}")]
		public async Task<IActionResult> FetchUser(long id) => throw new NotImplementedException();

		[HttpPost]
		public async Task<IActionResult> NewUserRecord([FromBody] object userRecord) => throw new NotImplementedException();

		[HttpPut]
		public async Task<IActionResult> EscalateUserRecord([FromBody] object userRecord) => throw new NotImplementedException();

		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteUserRecord(long id) => throw new NotImplementedException();
	}
}
