namespace SocialGuard.Web.Viewer.Data;

/// <summary>
/// Defines a credential used to authenticate a user on a SocialGuard API.
/// </summary>
/// <param name="Host">The API host to authenticate against.</param>
/// <param name="Login">The API login to authenticate with.</param>
/// <param name="Password">The API password to authenticate with.</param>
public sealed record AuthenticationDetails(
	Uri Host,
	string Login,
	string Password
);