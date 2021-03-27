using AspNetCore.Identity.Mongo.Model;



namespace SocialGuard.Api.Services.Authentication
{
	public class UserRole : MongoRole<string>
	{
		public UserRole() : base() { }
		public UserRole(string name) : base(name)
		{
			Id = name;
		}


		public const string Emitter = "emitter";
		public const string Admin = "admin";
	}
}
