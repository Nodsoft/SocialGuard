using Microsoft.EntityFrameworkCore;
using SocialGuard.Common.Data.Models;

namespace SocialGuard.Api.Data;

public class ApiDbContext : DbContext
{
	public DbSet<TrustlistEntry> TrustlistEntries { get; init; }
	public DbSet<Emitter> Emitters { get; init; }

	public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options) { }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		modelBuilder.Entity<TrustlistEntry>()
			.HasIndex(e => e.UserId);
	}
}