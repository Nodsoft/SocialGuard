using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SocialGuard.Common.Data.Models;

namespace SocialGuard.Api.Services;

public interface ITrustlistService
{
	IQueryable<ulong> ListUserIds();
	
	Task<TrustlistUser> FetchUserAsync(ulong id);
	Task<List<TrustlistUser>> FetchUsersAsync(IEnumerable<ulong> ids);
	
	Task InsertNewUserEntryAsync(ulong userId, TrustlistEntry entry, Emitter emitter);
	
	Task UpdateUserEntryAsync(ulong userId, TrustlistEntry updated, Emitter emitter);
	
	Task ImportEntriesAsync(IEnumerable<TrustlistUser> entries, Emitter commonEmitter, DateTime importTimestamp);
	
	Task DeleteUserEntryAsync(ulong userId, Emitter emitter);
}