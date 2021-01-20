using Microsoft.AspNetCore.Mvc;
using Natsecure.SocialGuard.Api.Data.Models;
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
		public async Task<IActionResult> FetchUser(ulong id) => throw new NotImplementedException();

		[HttpPost]
		public async Task<IActionResult> NewUserRecord([FromBody] TrustlistUser userRecord) => throw new NotImplementedException();

		[HttpPut]
		public async Task<IActionResult> EscalateUserRecord([FromBody] TrustlistUser userRecord) => throw new NotImplementedException();

		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteUserRecord(ulong id) => throw new NotImplementedException();
	}
}
