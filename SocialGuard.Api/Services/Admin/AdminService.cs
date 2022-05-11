using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SocialGuard.Api.Data.Authentication;
using SocialGuard.Api.Services.Authentication;

namespace SocialGuard.Api.Services.Admin;

/// <summary>
/// Provides methods to manage the API users and roles attribution.
/// </summary>
public class AdminService
{
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly RoleManager<UserRole> _roleManager;
	private readonly ILogger<AdminService> _logger;

	public AdminService(UserManager<ApplicationUser> userManager, RoleManager<UserRole> roleManager, ILogger<AdminService> logger)
	{
		_userManager = userManager;
		_roleManager = roleManager;
		_logger = logger;
	}
	
	public IQueryable<ApplicationUser> ListUsers() => _userManager.Users.AsNoTracking();

	public async Task<ApplicationUser> GetUserAsync(Guid userId) => await _userManager.Users.FirstOrDefaultAsync(x=> x.Id == userId);
	public async Task<ApplicationUser> GetUserAsync(string username) => await _userManager.FindByNameAsync(username);

	public async Task<IList<string>> GetUserRolesAsync(Guid userId)
	{
		ApplicationUser applicationUser = await GetUserAsync(userId);

		return applicationUser is null 
			? null 
			: await _userManager.GetRolesAsync(applicationUser);
	}

	public async Task AddUserRoleAsync(Guid userId, string roleName)
	{
		ApplicationUser user = await GetUserAsync(userId);
		UserRole role = await _roleManager.FindByNameAsync(roleName);
		
		if (user is not null && role is not null)
		{
			await _userManager.AddToRoleAsync(user, roleName);
			_logger.LogInformation("User {User.UserName} was given role {Role.Name}.", user, role);
		}
	}
	
	public async Task RemoveUserRoleAsync(Guid userId, string roleName)
	{
		ApplicationUser user = await GetUserAsync(userId);
		UserRole role = await _roleManager.FindByNameAsync(roleName);
		
		if (user is not null && role is not null)
		{
			await _userManager.RemoveFromRoleAsync(user, roleName);
			_logger.LogInformation("User {User.UserName} was revoked role {Role.Name}.", user, role);
		}
	}
	
	public async Task ResetPassword(Guid userId, string password = null)
	{
		ApplicationUser user = await GetUserAsync(userId);
		
		if (user is not null)
		{
			await _userManager.RemovePasswordAsync(user);
			
			if (password is not null)
			{
				await _userManager.AddPasswordAsync(user, password);
				_logger.LogInformation("User {User.UserName} password was reset.", user);
			}
			else
			{
				_logger.LogInformation("User {User.UserName} password was removed.", user);
			}
		}
	}
}