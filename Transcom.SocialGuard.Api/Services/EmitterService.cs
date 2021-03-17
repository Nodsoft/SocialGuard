using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using System.Threading.Tasks;
using Transcom.SocialGuard.Api.Data.Models;



namespace Transcom.SocialGuard.Api.Services
{
	public class EmitterService
	{
		private readonly IMongoCollection<Emitter> emitters;

		public EmitterService(IMongoDatabase database)
		{
			emitters = database.GetCollection<Emitter>(nameof(Emitter));
		}

		public async Task<Emitter> GetEmitterAsync(HttpContext context) => (await emitters.FindAsync(e => e.Login == context.User.Identity.Name)).FirstOrDefault();

		public async Task CreateOrUpdateEmitterSelfAsync(Emitter emitter, HttpContext context)
		{
			await emitters.InsertOneAsync(emitter with { Login = context.User.Identity.Name });
		}

		public async Task DeleteEmitterAsync(string emitterLogin) => await emitters.FindOneAndDeleteAsync(e => e.Login == emitterLogin);
	}
}