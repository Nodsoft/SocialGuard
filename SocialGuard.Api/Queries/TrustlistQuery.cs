using HotChocolate;
using HotChocolate.Data;
using Microsoft.EntityFrameworkCore;
using SocialGuard.Api.Data;
using SocialGuard.Api.Services;
using SocialGuard.Common.Data.Models;

namespace SocialGuard.Api.Queries;


/// <summary>
/// GraphQL query for fetching trustlist user records.
/// </summary>
public class TrustlistQuery
{
	/// <summary>
	/// Fetches trustlist entries
	/// </summary>
	[UseProjection, UseFiltering, UseSorting]
	public IQueryable<TrustlistEntry> GetEntries(ApiDbContext context) => context.TrustlistEntries
		.Include(e => e.Emitter)
		.AsNoTracking();

	/// <summary>
	/// Fetches trustlist emitters
	/// </summary>
	[UseProjection, UseFiltering, UseSorting]
	public IQueryable<Emitter> GetEmitters(ApiDbContext context) => context.Emitters.AsNoTracking();
}