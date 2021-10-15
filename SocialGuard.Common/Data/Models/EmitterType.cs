namespace SocialGuard.Common.Data.Models;

/// <summary>
/// Represents types of Emitter profiles.
/// </summary>
public enum EmitterType : byte
{
	/// <summary>
	/// Represents an unknown, undefined, or other Emitter profile type.
	/// </summary>
	Unknown = 0,

	/// <summary>
	/// Emitter is a singular user.
	/// </summary>
	User = 1,

	/// <summary>
	/// Emitter is a Discord server.
	/// </summary>
	Server = 2
}
