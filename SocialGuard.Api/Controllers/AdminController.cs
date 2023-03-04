using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialGuard.Api.Data.Authentication;
using SocialGuard.Api.Services.Admin;

namespace SocialGuard.Api.Controllers;

/// <summary>
/// Provides administration endpoints for managing the SocialGuard API. 
/// </summary>
[ApiController, Route("api/v{version:apiVersion}/[controller]"), Authorize(Roles = UserRole.Admin)]
[ApiVersion("3.0"), ApiVersion("2.0")]
public class AdminController
{
	private readonly AdminService _adminService;

	public AdminController(AdminService adminService)
	{
		_adminService = adminService;
	}

	/// <summary>
	/// Lists all users registered on the API.
	/// </summary>
	[HttpGet("user/list")]
	public IAsyncEnumerable<ApplicationUser> ListUsers() => _adminService.ListUsers().AsEnumerable().Select(RedactSensitiveFields).ToAsyncEnumerable();
	
	/// <summary>
	/// Gets the details of a specific user.
	/// </summary>
	/// <param name="id">ID of the user</param>
	[HttpGet("user/{id:guid:required}")]
	public async Task<ApplicationUser> GetUser(Guid id)
	{
		ApplicationUser applicationUser = await _adminService.GetUserAsync(id);
		return applicationUser is null ? null : RedactSensitiveFields(applicationUser);
	}

	/// <summary>
	/// Gets the details of a specific user.
	/// </summary>
	/// <param name="username">Username of the user</param>
	[HttpGet("user/{username:alpha:required}")]
	public async Task<ApplicationUser> GetUser(string username)
	{
		ApplicationUser applicationUser = await _adminService.GetUserAsync(username);
		return applicationUser is null ? null : RedactSensitiveFields(applicationUser);
	}
	
	/// <summary>
	/// Gets the roles assigned to a specific user.
	/// </summary>
	/// <param name="id">ID of the user</param>
	[HttpGet("user/{id:guid:required}/roles")]
	public async Task<IEnumerable<string>> GetUserRoles(Guid id) => await _adminService.GetUserRolesAsync(id);

	/// <summary>
	/// Adds a role to a specific user.
	/// </summary>
	/// <param name="id">ID of the user</param>
	/// <param name="role">Name of role to assign</param>
	[HttpPost("user/{id:guid}/role")]
	public async Task AddRole(Guid id, [FromQuery, Required] string role) => await _adminService.AddUserRoleAsync(id, role);
	
	/// <summary>
	/// Revokes a role from a specific user.
	/// </summary>
	/// <param name="id">ID of the user</param>
	/// <param name="role">Name of the role to revoke</param>
	[HttpDelete("user/{id:guid}/role")]
	public async Task RemoveRole(Guid id, [FromQuery, Required] string role) => await _adminService.RemoveUserRoleAsync(id, role);
	
	/// <summary>
	/// Resets or changes the password of a specific user.
	/// </summary>
	/// <param name="id">ID of the user</param>
	/// <param name="password">(Optional) New password for said user</param>
	[HttpPut("user/{id:guid}/password")]
	public async Task ChangePassword(Guid id, [FromQuery] string password = null) => await _adminService.ResetPassword(id, password);
	
	private static ApplicationUser RedactSensitiveFields(ApplicationUser user)
	{
		user.PasswordHash = null;
		user.SecurityStamp = null;
		return user;
	}
}