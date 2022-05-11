using System;
using Microsoft.AspNetCore.Identity;

namespace SocialGuard.Api.Data.Authentication;

public class ApplicationUser : IdentityUser<Guid>
{
	public ApplicationUser() : base() { }
		
	public ApplicationUser(string username) : base(username)
	{
		UserName = username;
	}
}