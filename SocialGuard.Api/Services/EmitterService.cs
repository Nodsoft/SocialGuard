using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using SocialGuard.Api.Data.Models;
using System.Threading.Tasks;



namespace SocialGuard.Api.Services
{
	public class EmitterService
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
			emitter = emitter with { Login = context.User.Identity.Name };

			if ((await emitters.FindAsync(e => e.Login == emitter.Login)).Any())
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
}