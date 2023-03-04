using AspNetCore.Identity.Mongo.Model;
using Mapster;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using SocialGuard.Api.Data;
using SocialGuard.Api.Data.Authentication;
using SocialGuard.Api.DbMigrator.Models.Mongo;
using SocialGuard.Api.Services.Authentication;
using SocialGuard.Common.Data.Models;

namespace SocialGuard.Api.DbMigrator;

public class DbMigrator : BackgroundService
{
	private readonly ILogger<DbMigrator> _logger;
	private readonly IServiceScopeFactory _scopeFactory;

	public DbMigrator(IServiceScopeFactory scopeFactory, ILogger<DbMigrator> logger)
	{
		_logger = logger;
		_scopeFactory = scopeFactory;
	}
	
	protected override async Task ExecuteAsync(CancellationToken ct)
	{
		_logger.LogInformation("Starting Database migrations...");
		await MigrateApiDatabase(ct);
		await MigrateAuthDatabase(ct);
		_logger.LogInformation("Migrations completed !");
	}

	/// <summary>
	/// Migrates all authentication entities from the MongoDB database to the PostgreSQL database.
	/// </summary>
	private async Task MigrateAuthDatabase(CancellationToken ct)
	{
		// Setup scope services
		await using AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();
		_logger.LogInformation("Starting migration of Auth database...");
		
		IMongoDatabase mongoDatabase = scope.ServiceProvider.GetRequiredService<IMongoClient>().GetDatabase("socialguard-auth");
		AuthDbContext context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
		await context.Database.MigrateAsync(cancellationToken: ct);

		// Perform MongoDB introspection for authentication entities
		MongoUser<string>[] users = mongoDatabase.GetCollection<MongoUser<string>>("User").AsQueryable().ToArray();
		MongoRole<string>[] roles = mongoDatabase.GetCollection<MongoRole<string>>("Role").AsQueryable().ToArray();
		_logger.LogInformation("Detected an estimated {UserCount} users and {RoleCount} roles within the Auth database.", users.Length, roles.Length);


		// Migrate entities to the PostgreSQL database
		_logger.LogInformation("Migrating users...");
		await context.Users.UpsertRange(users.Adapt<IEnumerable<ApplicationUser>>())
			.UpdateIf((u1, u2) => u1.Email == u2.Email)
			.AllowIdentityMatch()
			.RunAsync(ct);

		_logger.LogInformation("Migrating roles...");
		await context.Roles.UpsertRange(roles.Adapt<UserRole>())
			.UpdateIf((r1, r2) => r1.Name == r2.Name)
			.AllowIdentityMatch()
			.RunAsync(ct);
		
		await context.SaveChangesAsync(ct);
		
		_logger.LogInformation("Migration of Auth database complete.");
	}

	/// <summary>
	/// Migrates all Application-related entities from the MongoDB database to the PostgreSQL database.
	/// </summary>
	private async Task MigrateApiDatabase(CancellationToken ct)
	{
		// Setup scope services
		await using AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();
		_logger.LogInformation("Starting migration of API database...");

		IMongoDatabase mongoDatabase = scope.ServiceProvider.GetRequiredService<IMongoClient>().GetDatabase("socialguard-api");
		ApiDbContext context = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
		await context.Database.MigrateAsync(cancellationToken: ct);
		
		// Perform MongoDB introspection for application entities
		Emitter[] emitters = mongoDatabase.GetCollection<Emitter>(nameof(Emitter)).AsQueryable().ToArray();
		IQueryable<MongoTrustlistUser> users = mongoDatabase.GetCollection<MongoTrustlistUser>(nameof(TrustlistUser)).AsQueryable();
		
		List<TrustlistEntry> entries = (
			from user in users.ToList()
			from entry in user.Entries
			select entry.Adapt<TrustlistEntry>() with { UserId = user.Id, EmitterId = entry.Emitter.Login }).Distinct().ToList();


		_logger.LogInformation("Detected an estimated {EmitterCount} Emitters and {EntryCount} TrustlistEntries within the API database.", emitters.Length, entries.Count);
	
		// Migrate entities to the PostgreSQL database
		_logger.LogInformation("Performing migration of Emitters...");
		await context.Emitters.UpsertRange(emitters)
			.UpdateIf((e1, e2) => e1.Login == e2.Login)
			.AllowIdentityMatch()
			.RunAsync(ct);

		_logger.LogInformation("Performing migration of TrustlistEntries...");
		await context.TrustlistEntries.UpsertRange(entries)
			.UpdateIf((u1, u2) => u1.UserId == u2.UserId && u1.EmitterId == u2.EmitterId)
			.AllowIdentityMatch()
			.RunAsync(ct);
		
		_logger.LogInformation("Migration of API database complete.");
	}
}