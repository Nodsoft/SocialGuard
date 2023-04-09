namespace SocialGuard.Web.Viewer.Data;

/// <summary>
/// Defines an authentication token, obtained on API login.
/// </summary>
/// <param name="Host">The host name of the API.</param>
/// <param name="Token">The authentication token.</param>
/// <param name="Expiration">The expiration date of the token.</param>
public sealed record AuthenticationToken(Uri Host, string Token, DateTime Expiration)
{
	/// <summary>
	/// Courtesy method to evaluate if the token is expired.
	/// </summary>
	/// <returns><see langword="true"/> if the token is expired, otherwise <see langword="false"/>.</returns>
	public bool IsExpired => Expiration < DateTime.UtcNow;
}