using SocialGuard.Common.Data.Models;


namespace SocialGuard.Common.Hubs;

public interface ITrustlistHubPush
{
	Task NotifyNewEntry(ulong userId, TrustlistEntry entry);
	Task NotifyEscalatedEntry(ulong userId, TrustlistEntry updated, byte newLevel);
	Task NotifyDeletedEntry(ulong userId, TrustlistEntry deleted);
}

public interface ITrustlistHubInvoke
{
	Task<TrustlistUser> GetUserRecord(ulong userId);
}
