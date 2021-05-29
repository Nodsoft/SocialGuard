using Microsoft.AspNetCore.SignalR;
using MongoDB.Bson;
using MongoDB.Driver;
using SharpCompress.Common;
using SocialGuard.Api.Data.Models;
using SocialGuard.Api.Hubs;
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
		private readonly IHubContext<TrustlistHub> hubContext;

		public TrustlistUserService(IMongoDatabase database, IHubContext<TrustlistHub> hubContext)
		{
			trustlistUsers = database.GetCollection<TrustlistUser>(nameof(TrustlistUser));
			emitters = database.GetCollection<Emitter>(nameof(Emitter));
			this.hubContext = hubContext;
		}

		public IQueryable<ulong> ListUserIds() => from user in trustlistUsers.AsQueryable() select user.Id;

		public async Task<TrustlistUser> FetchUserAsync(ulong id) => (await trustlistUsers.FindAsync(u => u.Id == id)).FirstOrDefault();

		public async Task<IEnumerable<TrustlistUser>> FetchUsersAsync(ulong[] ids) => (await trustlistUsers.FindAsync(Builders<TrustlistUser>.Filter.In(u => u.Id, ids))).ToEnumerable();

		public async Task InsertNewUserEntryAsync(ulong userId, TrustlistEntry entry, Emitter emitter)
		{
			entry = entry with
			{
				Id = ObjectId.GenerateNewId().ToString(),
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
		}

		public async Task UpdateUserEntryAsync(ulong userId, TrustlistEntry updated, Emitter emitter)
		{
			TrustlistUser user = await FetchUserAsync(userId) ?? throw new ArgumentOutOfRangeException(nameof(userId), $"User {userId} not found.");
			TrustlistEntry existing = user.Entries.First(e => e.Emitter.Login == emitter.Login);

			await trustlistUsers.UpdateOneAsync(
				Builders<TrustlistUser>.Filter.Eq(u => u.Id, userId) 
				& Builders<TrustlistUser>.Filter.ElemMatch(u => u.Entries, Builders<TrustlistEntry>.Filter.Eq(e => e.Emitter.Login, emitter.Login)),
				Builders<TrustlistUser>.Update.Set(u => u.Entries[-1], updated with
				{
					Id = existing.Id,
					EntryAt = existing.EntryAt,
					LastEscalated = DateTime.UtcNow,
					Emitter = emitter
				})
			);
		}

		public Task ImportEntriesAsync(IEnumerable<TrustlistUser> entries, Emitter commonEmitter, DateTime importTimestamp)
		{
			throw new NotImplementedException();
		}

		public async Task DeleteUserEntryAsync(ulong id, Emitter emitter)
		{
			TrustlistUser user = await FetchUserAsync(id) ?? throw new ArgumentException($"No user found with ID {id}", nameof(id));
			TrustlistEntry existing = user.Entries.First(e => e.Emitter.Login == emitter.Login);

			await trustlistUsers.UpdateOneAsync(
				Builders<TrustlistUser>.Filter.Eq(u => u.Id, id),
				Builders<TrustlistUser>.Update.PullFilter(u => u.Entries, Builders<TrustlistEntry>.Filter.Eq(e => e.Emitter.Login, emitter.Login))
			);
		}
	}
}
