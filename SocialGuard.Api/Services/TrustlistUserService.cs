using MongoDB.Driver;
using SocialGuard.Api.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;



namespace SocialGuard.Api.Services
{
	public class TrustlistUserService
	{
		private readonly IMongoCollection<TrustlistUser> trustlistUsers;
		private readonly IMongoCollection<Emitter> emitters;

		public TrustlistUserService(IMongoDatabase database)
		{
			trustlistUsers = database.GetCollection<TrustlistUser>(nameof(TrustlistUser));
			emitters = database.GetCollection<Emitter>(nameof(Emitter));
		}

		public IQueryable<ulong> ListUserIds() => from user in trustlistUsers.AsQueryable() select user.Id;

		public async Task<TrustlistUser> FetchUserAsync(ulong id) => await (await trustlistUsers.FindAsync(u => u.Id == id)).FirstOrDefaultAsync();

		public async Task<IEnumerable<TrustlistUser>> FetchUsersAsync(ulong[] ids) => (await trustlistUsers.FindAsync(Builders<TrustlistUser>.Filter.In(u => u.Id, ids))).ToEnumerable();

		public async Task InsertNewUserAsync(TrustlistUser user, Emitter emitter)
		{
			// Check if user exists already.
			_ = await (await trustlistUsers.FindAsync(u => u.Id == user.Id)).AnyAsync() ? throw new ArgumentOutOfRangeException(nameof(user)) : false;

			await trustlistUsers.InsertOneAsync(user with
			{
				EntryAt = DateTime.UtcNow,
				LastEscalated = DateTime.UtcNow,
				Emitter = emitter
			});
		}

		public async Task EscalateUserAsync(TrustlistUser updated, Emitter emitter)
		{
			TrustlistUser current = await FetchUserAsync(updated.Id) ?? throw new ArgumentOutOfRangeException(nameof(updated));

			if (current.EscalationLevel < updated.EscalationLevel)
			{
				await trustlistUsers.ReplaceOneAsync(u => u.Id == current.Id, current with
				{
					EscalationLevel = updated.EscalationLevel,
					LastEscalated = DateTime.UtcNow,
					EscalationNote = updated.EscalationNote,
					Emitter = emitter
				});
			}
		}

		public async Task ImportEntriesAsync(IEnumerable<TrustlistUser> entries, Emitter commonEmitter, DateTime importTimestamp)
		{
			foreach (TrustlistUser entry in entries)
			{
				if (await FetchUserAsync(entry.Id) is null)
				{
					await trustlistUsers.InsertOneAsync(entry with
					{
						EscalationLevel = (byte)(entry.EscalationLevel is 0 ? 1 : entry.EscalationLevel),
						EscalationNote = entry.EscalationNote ?? $"Imported from {commonEmitter.DisplayName}",
						EntryAt = importTimestamp,
						LastEscalated = importTimestamp,
						Emitter = commonEmitter
					});
				}
			}
		}

		public async Task DeleteUserRecordAsync(ulong id)
		{
			TrustlistUser user = await FetchUserAsync(id) ?? throw new ArgumentException($"No user found with ID {id}", nameof(id));
			await trustlistUsers.DeleteOneAsync(u => u.Id == id);
		}
	}
}
