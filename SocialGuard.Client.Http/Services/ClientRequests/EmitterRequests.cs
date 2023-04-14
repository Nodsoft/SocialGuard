using System.Net;
using System.Net.Http.Json;
using SocialGuard.Common.Data.Models;
using SocialGuard.Common.Data.Models.Authentication;

namespace SocialGuard.Client.Http.Services;

public partial class SocialGuardHttpClient
{
	/// <summary>
	/// Gets the emitter profile for the current user.
	/// </summary>
	/// <param name="ct">The cancellation token.</param>
	/// <returns>The emitter profile for the current user.</returns>
	/// <remarks>
	/// Authentication is required to get a contextually relevant emitter profile.
	/// </remarks>
	public async Task<Emitter?> GetEmitterAsync(CancellationToken ct = default)
	{
		using HttpRequestMessage request = new(HttpMethod.Get, "/api/v3/emitter");
		using HttpResponseMessage response = await HttpClient.SendAsync(await request.WithAuthenticationHandlerAsync(this, true, ct), ct);

		return response.StatusCode is HttpStatusCode.OK
			? await response.Content.ReadFromJsonAsync<Emitter>(SerializerOptions, ct)
			: null;
	}
	
	/// <summary>
	/// Gets the emitter profile for a specific emitter.
	/// </summary>
	/// <param name="emitterId">The emitter to get the profile for.</param>
	/// <param name="ct">The cancellation token.</param>
	/// <returns>The emitter profile for the specified emitter.</returns>
	public async Task<Emitter?> GetEmitterAsync(string emitterId, CancellationToken ct = default)
	{
		using HttpRequestMessage request = new(HttpMethod.Get, $"/api/v3/emitter/{emitterId}");

		using HttpResponseMessage response = await HttpClient.SendAsync(await request.WithAuthenticationHandlerAsync(this, ct: ct), ct);

		return response.StatusCode is HttpStatusCode.OK
			? await response.Content.ReadFromJsonAsync<Emitter>(SerializerOptions, ct)
			: null;
	}
	
	/// <summary>
	/// Sets the emitter profile for the current user.
	/// </summary>
	/// <param name="emitter">The emitter profile to set.</param>
	/// <param name="ct">The cancellation token.</param>
	/// <remarks>
	/// Authentication is required to set an emitter profile.
	/// </remarks>
	public async Task SetEmitterAsync(Emitter emitter, CancellationToken ct)
	{
		using HttpRequestMessage request = new(HttpMethod.Post, "/api/v3/emitter");
		request.Content = JsonContent.Create(emitter);

		using HttpResponseMessage response = await HttpClient.SendAsync(await request.WithAuthenticationHandlerAsync(this, true, ct), ct);
		response.EnsureSuccessStatusCode();
	}
}