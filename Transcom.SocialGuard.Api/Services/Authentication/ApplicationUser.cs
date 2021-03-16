using AspNetCore.Identity.Mongo.Model;



namespace Transcom.SocialGuard.Api.Services.Authentication
{
	public class ApplicationUser : MongoUser
	{
		public ApplicationUser() : base() 
		{
			Id = Id == default ? new() : Id;
		}
		public ApplicationUser(string username) : base(username)
		{
			Id = Id == default ? new() : Id;
		}
	}
}