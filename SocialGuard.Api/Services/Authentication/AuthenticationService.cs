﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SocialGuard.Common.Data.Models.Authentication;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;



namespace SocialGuard.Api.Services.Authentication
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
			ApplicationUser user = new(model.Username) { Email = model.Email, SecurityStamp = Guid.NewGuid().ToString() };
			IdentityResult result = await userManager.CreateAsync(user, model.Password);

			if (!result.Succeeded)
			{
				return new() { StatusCode = 500, Response = Response.ErrorResponse() with { Message = "User creation has failed.", Details = result.Errors } };
			}

			if (firstUser)
			{
				await ProvisionFirstUseAsync(user);
			}

			return new() { StatusCode = 201, Response = Response.SuccessResponse() with { Message = "User created successfuly." } };
		}

		public async Task<AuthServiceResponse> HandleLogin(LoginModel model)
		{
			ApplicationUser user = await userManager.FindByNameAsync(model.Username);

			if (user is not null && await userManager.CheckPasswordAsync(user, model.Password))
			{
				List<Claim> authClaims = new()
				{
					new(ClaimTypes.Name, user.UserName),
					new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
				};
				authClaims.AddRange(await userManager.GetClaimsAsync(user));

				JwtSecurityToken token = GetToken(authClaims);

				return new()
				{
					StatusCode = 200,
					Response = Response.SuccessResponse() with
					{
						Message = "Login Successful.",
						Details = new TokenResult(new JwtSecurityTokenHandler().WriteToken(token), token.ValidTo)
					}
				};
			}

			return new() { StatusCode = 401, Response = Response.ErrorResponse() with { Message = "Login Failed." } };
		}


		private async Task ProvisionFirstUseAsync(ApplicationUser firstUser)
		{
			if (roleManager.Roles.Count() is 0)
			{
				await roleManager.CreateAsync(new(UserRole.Emitter));
				await roleManager.CreateAsync(new(UserRole.Emitter));

				await userManager.AddToRoleAsync(firstUser, UserRole.Admin);
			}
			else
			{
				throw new ApplicationException("Inconsistent Authentication Database state.");
			}
		}

		public AuthServiceResponse Whoami(HttpContext context)
		{
			object identity = new { context.User.Identity?.Name, context.User.Identity?.AuthenticationType };

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

		private static JwtSecurityToken GetToken(IEnumerable<Claim> authClaims) => new(
				issuer: configuration["JWT:ValidIssuer"],
				audience: configuration["JWT:ValidAudience"],
				expires: DateTime.Now.AddHours(1),
				claims: authClaims,
				signingCredentials: new(authSigningKey, SecurityAlgorithms.HmacSha256));
	}

	public record AuthServiceResponse<T>
	{
		public int StatusCode { get; init; }
		public Response<T> Response { get; init; }
	}

	public record AuthServiceResponse : AuthServiceResponse<object> { }
}
