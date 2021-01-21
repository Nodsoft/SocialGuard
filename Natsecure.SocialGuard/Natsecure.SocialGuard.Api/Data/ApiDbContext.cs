using Microsoft.EntityFrameworkCore;
using Natsecure.SocialGuard.Api.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Natsecure.SocialGuard.Api.Data
{
	public class ApiDbContext : DbContext
	{
		public DbSet<TrustlistUser> TrustlistUsers { get; set; }


		public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options) { }

/*		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);
		}
*/
	}
}
