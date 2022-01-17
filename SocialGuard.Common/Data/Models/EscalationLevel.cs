namespace SocialGuard.Common.Data.Models;

/// <summary>
/// Represents severity levels for Trustlist User entries.
/// </summary>
public enum EscalationLevel : byte
{
	Neutral = 0,
	Suspicious = 1,
	Untrusted = 2,
	Blacklisted = 3
}
