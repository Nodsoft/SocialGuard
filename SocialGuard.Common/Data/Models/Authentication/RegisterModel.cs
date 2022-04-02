namespace SocialGuard.Common.Data.Models.Authentication;


public record RegisterModel
{
	[Required]
	public string Username { get; init; } = string.Empty;

	[Required, EmailAddress]
	public string Email { get; init; } = string.Empty;

	[Required]
	public string Password { get; init; } = string.Empty;
}
