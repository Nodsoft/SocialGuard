using SocialGuard.Api.Data;

namespace SocialGuard.Api.Tests.Fixtures;

public class ApiDbContextFixture : IDisposable
{
	public ApiDbContext Context { get; private set; } = new(TestDbContextFactory.GetSqliteInMemoryContextOptions<ApiDbContext>());

	public void Dispose()
	{
		Context.Database.EnsureDeleted();
		Context.Dispose();
		GC.SuppressFinalize(this);
	}
}