using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SocialGuard.Common.Data.Models;

namespace SocialGuard.Api.Services;

public interface IEmitterService
{
	Task<Emitter> GetEmitterAsync(HttpContext context);
	Task<Emitter> GetEmitterAsync(string login);
	Task CreateOrUpdateEmitterSelfAsync(Emitter emitter, HttpContext context);
	Task DeleteEmitterAsync(string emitterLogin);
}