using System;
using AspNetCore.Identity.Mongo.Model;
using Microsoft.AspNetCore.Identity;


namespace SocialGuard.Api.Services.Authentication
{
	public class ApplicationUser : IdentityUser<Guid>
	{
		public ApplicationUser() : base() { }
		
		public ApplicationUser(string username) : base(username)
		{
			UserName = username;
		}
	}
}