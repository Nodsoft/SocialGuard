using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using SocialGuard.Common.Data.Models.Authentication;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using SocialGuard.Api.Data.Authentication;


namespace SocialGuard.Api.Services.Authentication;

public class AuthenticationService
{
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly RoleManager<UserRole> _roleManager;
	private static IConfiguration _configuration;
	private static SymmetricSecurityKey _authSigningKey;

	public AuthenticationService(UserManager<ApplicationUser> userManager, RoleManager<UserRole> roleManager, IConfiguration configuration)
	{
		_userManager = userManager;
		_roleManager = roleManager;
		_configuration ??= configuration;
		_authSigningKey = new(Encoding.UTF8.GetBytes(configuration["JWT:Secret"]));
	}

	public async Task<AuthServiceResponse> HandleRegister(RegisterModel model)
	{
		ApplicationUser userExists = await _userManager.FindByNameAsync(model.Username);
			
		if (userExists is not null)
		{
			return new() { StatusCode = 409, Response = Response.ErrorResponse() with { Message = "User already exists." } };
		}

		bool firstUser = _userManager.Users.Count() is 0;
		ApplicationUser user = new(model.Username) { Email = model.Email, SecurityStamp = Guid.NewGuid().ToString() };
		IdentityResult result = await _userManager.CreateAsync(user, model.Password);

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
		ApplicationUser user = await _userManager.FindByNameAsync(model.Username);

		if (user is not null && await _userManager.CheckPasswordAsync(user, model.Password))
		{
			List<Claim> authClaims = new()
			{
				new(ClaimTypes.Name, user.UserName),
				new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
			};
			authClaims.AddRange(await _userManager.GetClaimsAsync(user));
			authClaims.AddRange((await _userManager.GetRolesAsync(user)).Select(r => new Claim(ClaimTypes.Role, r)));

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


	public async Task ChangeUserPasswordAsync(string username, string oldPassword, string newPassword)
	{
		ApplicationUser user = await _userManager.FindByNameAsync(username);

		if (user is not null && await _userManager.CheckPasswordAsync(user, oldPassword))
		{
			IdentityResult result = await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);

			if (!result.Succeeded)
			{
				throw new(result.Errors.First().Description);
			}
		}
		else
		{
			throw new InvalidOperationException("Invalid old password.");
		}
	}


	private async Task ProvisionFirstUseAsync(ApplicationUser firstUser)
	{
		await _userManager.AddToRoleAsync(firstUser, UserRole.Admin);
	}

	public static AuthServiceResponse Whoami(HttpContext context)
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
		issuer: _configuration["JWT:ValidIssuer"],
		audience: _configuration["JWT:ValidAudience"],
		expires: DateTime.Now.AddHours(1),
		claims: authClaims,
		signingCredentials: new(_authSigningKey, SecurityAlgorithms.HmacSha256));
}

public record AuthServiceResponse<T>
{
	public int StatusCode { get; init; }
	public Response<T> Response { get; init; }
}

public record AuthServiceResponse : AuthServiceResponse<object>;