using System.ComponentModel.DataAnnotations;



namespace Transcom.SocialGuard.Api
{
	public record RegisterModel
	{
		[Required]
		public string Username { get; init; }

		[Required, EmailAddress]
		public string Email { get; init; }

		[Required]
		public string Password { get; init; }
	}
}