using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SocialGuard.Api.Data;
using SocialGuard.Api.Hubs;
using SocialGuard.Common.Data.Models;
using SocialGuard.Common.Hubs;

namespace SocialGuard.Api.Services;

public class PostgresTrustlistService : ITrustlistService
{
	private readonly ApiDbContext _context;
	private readonly IHubContext<TrustlistHub, ITrustlistHubPush> _hubContext;

	public PostgresTrustlistService(ApiDbContext context, IHubContext<TrustlistHub, ITrustlistHubPush> hubContext)
	{
		_context = context;
		_hubContext = hubContext;
	}


	public IQueryable<ulong> ListUserIds() => _context.TrustlistEntries
		.OrderBy(e => e.EntryAt)
		.Select(u => u.UserId);

	public async Task<TrustlistUser> FetchUserAsync(ulong id)
	{
		List<TrustlistEntry> entries = await _context.TrustlistEntries.Include(e => e.Emitter)
			.Where(u => u.UserId == id).ToListAsync();

		return entries.Count > 0 
			? new TrustlistUser { Id = id, Entries = entries } 
			: null; 
	}

	public async Task<List<TrustlistUser>> FetchUsersAsync(IEnumerable<ulong> ids) => await _context.TrustlistEntries.Include(e => e.Emitter)
		.Where(u => ids.Contains(u.UserId))
		.GroupBy(u => u.UserId)
		.Select(g => new TrustlistUser { Id = g.Key, Entries = g.ToList() })
		.ToListAsync();

	public async Task InsertNewUserEntryAsync(ulong userId, TrustlistEntry entry, Emitter emitter)
	{
		entry = entry with
		{
			Id = Guid.NewGuid(),
			UserId = userId,
			EntryAt = DateTime.UtcNow,
			LastEscalated = DateTime.UtcNow,
			Emitter = emitter
		};
		
		await _context.TrustlistEntries.AddAsync(entry);
		await _context.SaveChangesAsync();

		await _hubContext.Clients.All.NotifyNewEntry(userId, entry);
	}

	public async Task UpdateUserEntryAsync(ulong userId, TrustlistEntry updated, Emitter emitter)
	{
		TrustlistEntry current = await _context.TrustlistEntries.Include(e => e.Emitter)
				.FirstOrDefaultAsync(u => u.UserId == userId && u.EmitterId == emitter.Login)
			?? throw new ArgumentException($"No entry found for user {userId} and emitter {emitter.Login}. Cannot update.");
		
		current.EscalationLevel = updated.EscalationLevel;
		current.LastEscalated = DateTime.UtcNow;
		current.EscalationNote = updated.EscalationNote;
		
		await _context.SaveChangesAsync();
		await _hubContext.Clients.All.NotifyEscalatedEntry(userId, current, current.EscalationLevel);
	}

	public async Task ImportEntriesAsync(IEnumerable<TrustlistUser> entries, Emitter commonEmitter, DateTime importTimestamp)
	{
		throw new NotImplementedException();
	}

	public async Task DeleteUserEntryAsync(ulong userId, Emitter emitter)
	{
		TrustlistEntry entry = await _context.TrustlistEntries.Include(e => e.Emitter)
				.FirstOrDefaultAsync(u => u.UserId == userId && u.EmitterId == emitter.Login)
			?? throw new ArgumentException($"No entry found for user {userId} and emitter {emitter.Login}. Cannot delete.");

		_context.TrustlistEntries.Remove(entry);
		await _context.SaveChangesAsync();

		await _hubContext.Clients.All.NotifyDeletedEntry(userId, entry);
	}
}