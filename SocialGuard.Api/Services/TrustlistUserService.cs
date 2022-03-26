using Microsoft.AspNetCore.SignalR;
using MongoDB.Bson;
using MongoDB.Driver;
using SocialGuard.Common.Data.Models;
using SocialGuard.Common.Hubs;
using SocialGuard.Api.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;



namespace SocialGuard.Api.Services;

public class TrustlistUserService : ITrustlistUserService
{
	private readonly IMongoCollection<TrustlistUser> trustlistUsers;
	private readonly IMongoCollection<Emitter> emitters;
	private readonly IHubContext<TrustlistHub, ITrustlistHubPush> hubContext;

	public TrustlistUserService(IMongoDatabase database, IHubContext<TrustlistHub, ITrustlistHubPush> hubContext)
	{
		trustlistUsers = database.GetCollection<TrustlistUser>(nameof(TrustlistUser));
		emitters = database.GetCollection<Emitter>(nameof(Emitter));
		this.hubContext = hubContext;
	}

	public IQueryable<ulong> ListUserIds() => from user in trustlistUsers.AsQueryable() select user.Id;

	public async Task<TrustlistUser> FetchUserAsync(ulong id) => (await trustlistUsers.FindAsync(u => u.Id == id)).FirstOrDefault();

	public async Task<List<TrustlistUser>> FetchUsersAsync(IEnumerable<ulong> ids) 
		=> await (await trustlistUsers.FindAsync(Builders<TrustlistUser>.Filter.In(u => u.Id, ids))).ToListAsync();

	public async Task InsertNewUserEntryAsync(ulong userId, TrustlistEntry entry, Emitter emitter)
	{
		entry = entry with
		{
			Id = Guid.NewGuid(),
			EntryAt = DateTime.UtcNow,
			LastEscalated = DateTime.UtcNow,
			Emitter = emitter
		};

		if (await FetchUserAsync(userId) is null)
		{
			await trustlistUsers.InsertOneAsync(new()
			{
				Id = userId,
				Entries = new() { entry }
			});
		}
		else
		{
			await trustlistUsers.UpdateOneAsync(
				Builders<TrustlistUser>.Filter.Eq(u => u.Id, userId),
				Builders<TrustlistUser>.Update.Push(u => u.Entries, entry)
			);
		}

		await hubContext.Clients.All.NotifyNewEntry(userId, entry);
	}

	public async Task UpdateUserEntryAsync(ulong userId, TrustlistEntry updated, Emitter emitter)
	{
		TrustlistUser user = await FetchUserAsync(userId) ?? throw new ArgumentOutOfRangeException(nameof(userId), $"User {userId} not found.");
		TrustlistEntry existing = user.Entries.First(e => e.Emitter.Login == emitter.Login);

		updated = updated with
		{
			Id = existing.Id,
			EntryAt = existing.EntryAt,
			LastEscalated = DateTime.UtcNow,
			Emitter = emitter
		};

		await trustlistUsers.UpdateOneAsync(
			Builders<TrustlistUser>.Filter.Eq(u => u.Id, userId) 
			& Builders<TrustlistUser>.Filter.ElemMatch(u => u.Entries, Builders<TrustlistEntry>.Filter.Eq(e => e.Emitter.Login, emitter.Login)),
			Builders<TrustlistUser>.Update.Set(u => u.Entries[-1], updated)
		);

		if (updated.EscalationLevel > existing.EscalationLevel)
		{
			await hubContext.Clients.All.NotifyEscalatedEntry(userId, updated, existing.EscalationLevel);
		}
	}

	public Task ImportEntriesAsync(IEnumerable<TrustlistUser> entries, Emitter commonEmitter, DateTime importTimestamp)
	{
		throw new NotImplementedException();
	}

	public async Task DeleteUserEntryAsync(ulong userId, Emitter emitter)
	{
		TrustlistUser user = await FetchUserAsync(userId) ?? throw new ArgumentException($"No user found with ID {userId}", nameof(userId));
		TrustlistEntry existing = user.Entries.First(e => e.Emitter.Login == emitter.Login);

		await trustlistUsers.UpdateOneAsync(
			Builders<TrustlistUser>.Filter.Eq(u => u.Id, userId),
			Builders<TrustlistUser>.Update.PullFilter(u => u.Entries, Builders<TrustlistEntry>.Filter.Eq(e => e.Emitter.Login, emitter.Login))
		);

		await hubContext.Clients.All.NotifyDeletedEntry(userId, existing);
	}
}