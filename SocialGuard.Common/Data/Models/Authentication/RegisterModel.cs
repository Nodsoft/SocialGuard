namespace SocialGuard.Common.Data.Models.Authentication;


public record RegisterModel
{
	[Required, NotNull]
	public string Username { get; init; } = string.Empty;

	[Required, NotNull, EmailAddress]
	public string Email { get; init; } = string.Empty;

	[Required, NotNull]
	public string Password { get; init; } = string.Empty;
}
