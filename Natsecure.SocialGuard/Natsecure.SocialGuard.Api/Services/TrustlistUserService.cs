using Microsoft.EntityFrameworkCore;
using Natsecure.SocialGuard.Api.Data;
using Natsecure.SocialGuard.Api.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Natsecure.SocialGuard.Api.Services
{
	public class TrustlistUserService
	{
		private readonly ApiDbContext context;

		public TrustlistUserService(IDbContextFactory<ApiDbContext> contextFactory)
		{
			context = contextFactory.CreateDbContext();
		}

		public IEnumerable<ulong> ListUserIds() => from user in context.TrustlistUsers select user.Id;

		public async Task<TrustlistUser> FetchUserAsync(ulong id) => await context.TrustlistUsers.FindAsync(id);

		public async Task InsertNewUserAsync(TrustlistUser user)
		{
			user.EntryAt = DateTime.UtcNow;
			user.LastEscalated = DateTime.UtcNow;

			context.TrustlistUsers.Add(user);
			await context.SaveChangesAsync();
		}

		public async Task EscalateUserAsync(TrustlistUser updated)
		{
			TrustlistUser current = await context.TrustlistUsers.FindAsync(updated.Id) ?? throw new ArgumentOutOfRangeException(nameof(updated));

			if (current.EscalationLevel < updated.EscalationLevel)
			{
				current.EscalationLevel = updated.EscalationLevel;
				current.LastEscalated = DateTime.UtcNow;
				current.EscalationNote = updated.EscalationNote;

				await context.SaveChangesAsync();
			}
		}

		public async Task DeleteUserAsync(ulong id)
		{
			TrustlistUser user = await context.TrustlistUsers.FindAsync(id) ?? throw new ArgumentException($"No user found with ID {id}", nameof(id));

			context.TrustlistUsers.Remove(user);
			await context.SaveChangesAsync();
		}
	}
}
