using SocialGuard.Common.Data.Models;

namespace SocialGuard.Web.Viewer;

public static class Utilities
{
	public static string GetTrustlistLevelBootstrapColor(this TrustlistEntry entry) => entry.EscalationLevel switch
	{
		> 3 => "nuclear",
		3 => "danger",
		2 => "warning",
		1 => "info",
		_ => "success"
	};

	public static string GetTrustlistLevelDisplayString(this TrustlistEntry entry) => entry.EscalationLevel switch
	{
		> 3 => "☢️ Nuclear ☢️",
		3 => "Dangerous",
		2 => "Untrusted",
		1 => "Suspicious",
		_ => "Clean"
	};

	public static string GetEmitterTypeDisplayName(this EmitterType type) => type switch
	{
		EmitterType.User => "User",
		EmitterType.Server => "Guild / Server",
		_ => "Unknown / Other",
	};
}
