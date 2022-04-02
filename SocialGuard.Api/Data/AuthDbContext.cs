using System;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SocialGuard.Api.Services.Authentication;

namespace SocialGuard.Api.Data;

public class AuthDbContext : IdentityDbContext<ApplicationUser, UserRole, Guid>
{
	public AuthDbContext(DbContextOptions<AuthDbContext> options)
		: base(options)
	{
		
	}

	protected override void OnModelCreating(ModelBuilder builder)
	{
		base.OnModelCreating(builder);
		
		// Customize the ASP.NET Identity model and override the defaults if needed.
		// For example, you can rename the ASP.NET Identity table names and more.
		// Add your customizations after calling base.OnModelCreating(builder);
		
		builder.HasDefaultSchema("auth");

		builder.Entity<UserRole>()
			.HasData(
				new UserRole { Id = Guid.NewGuid(), Name = UserRole.Admin, NormalizedName = UserRole.Admin.ToUpper() },
				new UserRole { Id = Guid.NewGuid(), Name = UserRole.Emitter, NormalizedName = UserRole.Emitter.ToUpper() }
			);
		
		/*
		builder.Entity<ApplicationUser>()
			.Property(u => u.Id)
			.UseIdentityColumn();
		
		builder.Entity<UserRole>()
			.Property(u => u.Id)
			.UseIdentityColumn();
		*/
	}
}