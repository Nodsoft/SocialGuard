using Transcom.SocialGuard.Api.Data.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;



namespace Transcom.SocialGuard.Api.Services
{
	public class TrustlistUserService
	{
		private readonly IMongoCollection<TrustlistUser> trustlistUsers;

		public TrustlistUserService(IMongoDatabase database) => trustlistUsers = database.GetCollection<TrustlistUser>(nameof(TrustlistUser));


		public IQueryable<ulong> ListUserIds() => from user in trustlistUsers.AsQueryable() select user.Id;

		public async Task<TrustlistUser> FetchUserAsync(ulong id) => await (await trustlistUsers.FindAsync(u => u.Id == id)).FirstOrDefaultAsync();

		public async Task InsertNewUserAsync(TrustlistUser user)
		{
			// Check if user exists already.
			_ = await (await trustlistUsers.FindAsync(u => u.Id == user.Id)).AnyAsync() ? throw new ArgumentOutOfRangeException(nameof(user)) : false;


			user.EntryAt = DateTime.UtcNow;
			user.LastEscalated = DateTime.UtcNow;

			await trustlistUsers.InsertOneAsync(user);
		}

		public async Task EscalateUserAsync(TrustlistUser updated)
		{
			TrustlistUser current =	await FetchUserAsync(updated.Id) ?? throw new ArgumentOutOfRangeException(nameof(updated));

			if (current.EscalationLevel < updated.EscalationLevel)
			{
				current.EscalationLevel = updated.EscalationLevel;
				current.LastEscalated = DateTime.UtcNow;
				current.EscalationNote = updated.EscalationNote;

				await trustlistUsers.ReplaceOneAsync(u => u.Id == current.Id, current);
			}
		}

		public async Task DeleteUserAsync(ulong id)
		{
			TrustlistUser user = await FetchUserAsync(id) ?? throw new ArgumentException($"No user found with ID {id}", nameof(id));
			await trustlistUsers.DeleteOneAsync(u => u.Id == id);
		}
	}
}
