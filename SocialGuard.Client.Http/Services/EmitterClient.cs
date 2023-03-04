using System.Net;
using System.Net.Http.Json;
using SocialGuard.Common.Data.Models;
using SocialGuard.Common.Data.Models.Authentication;

namespace SocialGuard.Client.Http.Services
{
	public class EmitterClient : RestClientBase
	{
		public EmitterClient(HttpClient httpClient) : base(httpClient) { }


		public Task<Emitter?> GetEmitterAsync(TokenResult token) => GetEmitterAsync(token, CancellationToken.None);
		public async Task<Emitter?> GetEmitterAsync(TokenResult token, CancellationToken ct)
		{
			using HttpRequestMessage request = new(HttpMethod.Get, "/api/v3/emitter");
			request.Headers.Authorization = new("bearer", token.Token);

			using HttpResponseMessage response = await HttpClient.SendAsync(request, ct);

			return response.StatusCode is HttpStatusCode.OK
				? await response.Content.ReadFromJsonAsync<Emitter>(SerializerOptions, ct)
				: null;
		}

		public Task<Emitter?> GetEmitterAsync(string emitterId) => GetEmitterAsync(emitterId, CancellationToken.None);
		public async Task<Emitter?> GetEmitterAsync(string emitterId, CancellationToken ct)
		{
			using HttpRequestMessage request = new(HttpMethod.Get, $"/api/v3/emitter/{emitterId}");

			using HttpResponseMessage response = await HttpClient.SendAsync(request, ct);

			return response.StatusCode is HttpStatusCode.OK
				? await response.Content.ReadFromJsonAsync<Emitter>(SerializerOptions, ct)
				: null;
		}

		public Task SetEmitterAsync(Emitter emitter, TokenResult token) => SetEmitterAsync(emitter, token, CancellationToken.None);
		public async Task SetEmitterAsync(Emitter emitter, TokenResult token, CancellationToken ct)
		{
			using HttpRequestMessage request = new(HttpMethod.Post, "/api/v3/emitter");
			request.Content = JsonContent.Create(emitter);
			request.Headers.Authorization = new("bearer", token.Token);

			using HttpResponseMessage response = await HttpClient.SendAsync(request, ct);
			response.EnsureSuccessStatusCode();
		}
	}
}
