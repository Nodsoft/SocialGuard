﻿using MongoDB.Driver;
using SocialGuard.Common.Data.Models;


namespace SocialGuard.Api.Services;

public class EmitterService : IEmitterService
{
	private readonly IMongoCollection<Emitter> emitters;

	public EmitterService(IMongoDatabase database)
	{
		emitters = database.GetCollection<Emitter>(nameof(Emitter));
	}

	public async Task<Emitter> GetEmitterAsync(HttpContext context) => (await emitters.FindAsync(e => e.Login == context.User.Identity.Name)).FirstOrDefault();
	public async Task<Emitter> GetEmitterAsync(string login) => (await emitters.FindAsync(e => e.Login == login)).FirstOrDefault();

	public async Task CreateOrUpdateEmitterSelfAsync(Emitter emitter, HttpContext context)
	{
		emitter = emitter with { Login = context.User.Identity?.Name };

		if (await (await emitters.FindAsync(e => e.Login == emitter.Login)).AnyAsync())
		{
			await emitters.ReplaceOneAsync(e => e.Login == emitter.Login, emitter);
		}
		else
		{
			await emitters.InsertOneAsync(emitter);
		}
	}

	public async Task DeleteEmitterAsync(string emitterLogin) => await emitters.FindOneAndDeleteAsync(e => e.Login == emitterLogin);
}