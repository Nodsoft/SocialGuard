using Microsoft.AspNetCore.Identity;

namespace SocialGuard.Api.Data.Authentication;

public class UserRole : IdentityRole<Guid>
{
	public UserRole() { }
	public UserRole(string roleName) : base(roleName) { }
		
		
	public const string Emitter = "emitter";
	public const string Admin = "admin";
}