using System.Text.Json.Serialization;

namespace SocialGuard.Web.Viewer.Data;

/// <summary>
/// Defines an API credential used to authenticate a user on a SocialGuard API.
/// </summary>
/// <remarks>
///	Blank login/password is allowed, and is used to authenticate as a guest.
/// This is useful for mapping out different directories.
/// </remarks>
public sealed record AuthenticationDetails
{
	/// <summary>
	/// Defines an API host, with no authentication information.
	/// </summary>
	/// <param name="displayName">The display name of the API host.</param>
	/// <param name="host">The host of the SocialGuard API.</param>
	public AuthenticationDetails(string displayName, Uri host)
	{
		DisplayName = displayName;
		Host = host;
	}

	/// <summary>
	/// Defines a credential used to authenticate a user on a SocialGuard API.
	/// </summary>
	/// <param name="displayName">The display name of the API Host.</param>
	/// <param name="host">The API host to authenticate against.</param>
	/// <param name="login">The API login to authenticate with.</param>
	/// <param name="password">The API password to authenticate with.</param>
	[JsonConstructor]
	public AuthenticationDetails(string displayName, Uri host, string login, string password)
	{
		DisplayName = displayName;
		Host = host;
		Login = login;
		Password = password;
	}
	
	/// <summary>
	/// User-defined display name for this API Host.
	/// </summary>
	/// <remarks>
	///	The attribution of this value is currently up to the user, but will change in the future (SG 4.0).
	/// </remarks>
	public string DisplayName { get; set; }

	/// <summary>
	/// The API host to authenticate against.
	/// </summary>
	public Uri Host { get; init; }

	/// <summary>
	/// The API login to authenticate with.
	/// </summary>
	public string? Login { get; set; }

	/// <summary>
	/// The API password to authenticate with.
	/// </summary>
	public string? Password { get; set; }
}