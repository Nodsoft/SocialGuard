using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Transcom.SocialGuard.Api.Services.Authentication.Models;



namespace Transcom.SocialGuard.Api.Services.Authentication
{
	public class AuthenticationService
	{
		private readonly UserManager<ApplicationUser> userManager;
		private readonly RoleManager<UserRole> roleManager;
		private static IConfiguration configuration;
		private static SymmetricSecurityKey authSigningKey;

		public AuthenticationService(UserManager<ApplicationUser> userManager, RoleManager<UserRole> roleManager, IConfiguration configuration)
		{
			this.userManager = userManager;
			this.roleManager = roleManager;
			AuthenticationService.configuration ??= configuration;
			authSigningKey = new(Encoding.UTF8.GetBytes(configuration["JWT:Secret"]));
		}

		public async Task<AuthServiceResponse> HandleRegister(RegisterModel model)
		{
			ApplicationUser userExists = await userManager.FindByNameAsync(model.Username);
			if (userExists is not null)
			{
				return new() { StatusCode = 409, Response = Response.ErrorResponse() with { Message = "User already exists." } };
			}

			bool firstUser = userManager.Users.Count() is 0;
			ApplicationUser user = new() { Email = model.Email, SecurityStamp = Guid.NewGuid().ToString(), UserName = model.Username };
			IdentityResult result = await userManager.CreateAsync(user, model.Password);

			if (!result.Succeeded)
			{
				return new() { StatusCode = 500, Response = Response.ErrorResponse() with { Message = $"User creation has failed.", Details = result.Errors } };
			}

			if (firstUser)
			{
				await ProvisionFirstUseAsync(user);
			}

			await userManager.AddToRoleAsync(user, UserRole.User);


			return new() { StatusCode = 200, Response = Response.SuccessResponse() with { Message = "User created successfuly." } };
		}

		public async Task<AuthServiceResponse> HandleLogin(LoginModel model)
		{
			ApplicationUser user = await userManager.FindByNameAsync(model.Username);

			if (user is not null && await userManager.CheckPasswordAsync(user, model.Password))
			{
				IList<string> userRoles = await userManager.GetRolesAsync(user);
				List<Claim> authClaims = new()
				{
					new Claim(ClaimTypes.Name, user.UserName),
					new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
				};
				authClaims.AddRange(from string userRole in userRoles select new Claim(ClaimTypes.Role, userRole));

				JwtSecurityToken token = GetToken(authClaims);

				return new()
				{
					StatusCode = 200,
					Response = Response.SuccessResponse() with
					{
						Message = "Login Successful.",
						Details = new { token = new JwtSecurityTokenHandler().WriteToken(token), expiration = token.ValidTo }
					}
				};
			}

			return new() { StatusCode = 401, Response = Response.ErrorResponse() with { Message = "Login Failed." } };
		}


		private async Task ProvisionFirstUseAsync(ApplicationUser firstUser)
		{
			if (roleManager.Roles.Count() is 0)
			{
				await roleManager.CreateAsync(new(UserRole.Admin));
				await roleManager.CreateAsync(new(UserRole.User));

				await userManager.AddToRoleAsync(firstUser, UserRole.Admin);
			}
			else
			{
				throw new ApplicationException("Inconsistent Authentication Database state.");
			}
		}

		public AuthServiceResponse Whoami(HttpContext context)
		{
			object identity = new { context.User.Identity.Name, context.User.Identity.AuthenticationType };

			List<object> claims = new(from claim in context.User.Claims select claim.Value);


			return new()
			{
				StatusCode = 200,
				Response = Response.SuccessResponse() with
				{
					Details = new
					{
						Identity = identity,
						Claims = claims
					}
				}
			};
		}

		private static JwtSecurityToken GetToken(List<Claim> authClaims) => new(
				issuer: configuration["JWT:ValidIssuer"],
				audience: configuration["JWT:ValidAudience"],
				expires: DateTime.Now.AddHours(1),
				claims: authClaims,
				signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256));
	}

	public record AuthServiceResponse
	{
		public int StatusCode { get; init; }
		public Response Response { get; init; }
		internal object InternalDetails { get; init; }
	}
}
