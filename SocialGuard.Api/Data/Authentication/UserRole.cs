using System;
using AspNetCore.Identity.Mongo.Model;
using Microsoft.AspNetCore.Identity;


namespace SocialGuard.Api.Services.Authentication
{
	public class UserRole : IdentityRole<Guid>
	{
		public UserRole() : base() { }
		public UserRole(string roleName) : base(roleName) { }
		
		
		public const string Emitter = "emitter";
		public const string Admin = "admin";
	}
}
