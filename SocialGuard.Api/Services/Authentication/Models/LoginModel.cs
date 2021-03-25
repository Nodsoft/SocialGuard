using System.ComponentModel.DataAnnotations;



namespace SocialGuard.Api.Services.Authentication.Models
{
	public class LoginModel
	{
		[Required]
		public string Username { get; set; }

		[Required]
		public string Password { get; set; }
	}
}
