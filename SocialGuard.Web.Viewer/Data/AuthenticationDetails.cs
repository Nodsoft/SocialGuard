using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SocialGuard.Web.Viewer.Data;

/// <summary>
/// Defines an API credential used to authenticate a user on a SocialGuard API.
/// </summary>
/// <remarks>
///	Blank login/password is allowed, and is used to authenticate as a guest.
/// This is useful for mapping out different directories.
/// </remarks>
[CustomValidation(typeof(AuthenticationDetails), nameof(ValidateLoginPasswordPair))]
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
	/// Defines a credential used to authenticate a user on a SocialGuard API/Directory.
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
	/// The ID used to identify these authentication details.
	/// </summary>
	/// <remarks>
	/// This value is specific to the Browser itself, and is not shared between users.
	/// It is used to identify the authentication details in the local storage.
	/// Value should be unique.
	/// </remarks>
	[JsonIgnore]
	public Guid Id { get; init; } = Guid.NewGuid();

	/// <summary>
	/// User-defined display name for this API Host.
	/// </summary>
	/// <remarks>
	///	The attribution of this value is currently up to the user, but will change in the future (SG 4.0).
	/// </remarks>
	[Required]
	public string DisplayName { get; set; }

	/// <summary>
	/// The API host to authenticate against.
	/// </summary>
	public Uri Host { get; set; }

	/// <summary>
	/// The API host to authenticate against, as a string.
	/// </summary>
	/// <remarks>
	/// This is a convenience property for form manipulation, and is not serialized.
	/// </remarks>
	[JsonIgnore, Required, EditorBrowsable(EditorBrowsableState.Advanced)]
	public string HostStr
	{
		get => Host.ToString();
		set => Host = new(value);
	}

	/// <summary>
	/// The API login to authenticate with.
	/// </summary>
	public string? Login { get; set; }

	/// <summary>
	/// The API password to authenticate with.
	/// </summary>
	[DataType(DataType.Password)]
	public string? Password { get; set; }

	/// <summary>
	/// Validates the login/password pair.
	/// </summary>
	/// <returns>A <see cref="ValidationResult"/> object.</returns>
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public static ValidationResult? ValidateLoginPasswordPair(AuthenticationDetails? authDetails,
		ValidationContext validationContext)
	{
		// Validate the presence of both login and password, or neither.
		return authDetails?.Login is null or "" ^ authDetails?.Password is null or ""
			? new("Login and password must be provided together.", new[] { nameof(Login), nameof(Password) })
			: ValidationResult.Success;
	}
}