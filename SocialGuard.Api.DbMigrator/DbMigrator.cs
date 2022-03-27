using AspNetCore.Identity.Mongo.Model;
using Mapster;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using SocialGuard.Api.Data;
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
		await Task.WhenAll(MigrateAuthDatabase(ct), MigrateApiDatabase(ct));
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
		await context.Users.UpsertRange(users.Adapt<IEnumerable<ApplicationUser>>()).RunAsync(ct);
		_logger.LogInformation("Migrating roles...");
		await context.Roles.UpsertRange(roles.Adapt<UserRole>()).RunAsync(ct);
		
		_logger.LogInformation("Migration of Auth database complete.");
	}

	/// <summary>
	/// Migrates all Application-related entities from the MongoDB database to the PostgreSQL database.
	/// </summary>
	/// <param name="ct"></param>
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
		MongoTrustlistUser[] users = mongoDatabase.GetCollection<MongoTrustlistUser>(nameof(TrustlistUser)).AsQueryable().ToArray();
		
		_logger.LogInformation("Detected an estimated {EmitterCount} Emitters and {UserCount} TrustlistUsers within the API database.", emitters.Length, users.Length);
	
		// Migrate entities to the PostgreSQL database
		_logger.LogInformation("Performing migration of Emitters...");
		await context.Emitters.UpsertRange(emitters).RunAsync(ct);
		
		_logger.LogInformation("Performing migration of TrustlistUsers...");
		await context.TrustlistUsers.UpsertRange(users.Adapt<IEnumerable<TrustlistUser>>()).RunAsync(ct);
		
		_logger.LogInformation("Migration of API database complete.");
	}
}