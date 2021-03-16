using AspNetCore.Identity.Mongo.Model;



namespace Transcom.SocialGuard.Api.Services.Authentication
{
	public class UserRole : MongoRole
	{
		public UserRole() : base() { }
		public UserRole(string name) : base(name) { }
	
		
		public const string User = "user";
		public const string Admin = "admin";
	}
}
