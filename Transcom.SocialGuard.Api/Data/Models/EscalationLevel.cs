namespace Transcom.SocialGuard.Api.Data.Models
{
	public enum EscalationLevel : byte
	{
		Neutral = 0,
		Suspicious = 1,
		Untrusted = 2,
		Blacklisted = 3
	}
}
