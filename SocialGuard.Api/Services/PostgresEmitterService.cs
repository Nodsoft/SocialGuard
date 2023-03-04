using Microsoft.EntityFrameworkCore;
using SocialGuard.Api.Data;
using SocialGuard.Common.Data.Models;

namespace SocialGuard.Api.Services;

public class PostgresEmitterService : IEmitterService
{
	private readonly ApiDbContext _context;

	public PostgresEmitterService(ApiDbContext context)
	{
		_context = context;
	}

	public async Task<Emitter> GetEmitterAsync(HttpContext httpContext) => await _context.Emitters.FirstOrDefaultAsync(e => e.Login == httpContext.User.Identity.Name);

	public async Task<Emitter> GetEmitterAsync(string login) => await _context.Emitters.FirstOrDefaultAsync(e => e.Login == login);

	public async Task CreateOrUpdateEmitterSelfAsync(Emitter emitter, HttpContext context) => 
		await _context.Emitters.Upsert(emitter with { Login = context.User.Identity?.Name })
			.UpdateIf((e1, e2) => e1.Login == e2.Login)
			.RunAsync();

	public async Task DeleteEmitterAsync(string emitterLogin)
	{
		Emitter emitter = await _context.Emitters.FirstOrDefaultAsync(e => e.Login == emitterLogin);
		
		if (emitter is not null)
		{
			_context.Emitters.Remove(emitter);
			await _context.SaveChangesAsync();
		}
	}
}