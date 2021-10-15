namespace SocialGuard.Common.Data.Models.Authentication;

public class LoginModel
{
	[Required, NotNull]
	public string Username { get; set; } = string.Empty;

	[Required, NotNull]
	public string Password { get; set; } = string.Empty;
}
