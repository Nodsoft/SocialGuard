namespace SocialGuard.Common.Data.Models.Authentication;

public sealed record TokenResult(string Token, DateTime Expiration)
{
	public bool IsValid() => Expiration > DateTime.UtcNow;
}
