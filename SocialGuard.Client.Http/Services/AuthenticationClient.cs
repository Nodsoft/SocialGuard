using System.Net;
using System.Net.Http.Json;
using SocialGuard.Common.Data.Models.Authentication;

namespace SocialGuard.Client.Http.Services;


public class AuthenticationClient : RestClientBase
{
	public AuthenticationClient(HttpClient httpClient) : base(httpClient) { }

	public async Task<Response?> RegisterNewUserAsync(RegisterModel registerDetails, CancellationToken ct)
	{
		using HttpRequestMessage request = new(HttpMethod.Post, "/api/v3/auth/register")
		{
			Content = JsonContent.Create(registerDetails)
		};

		using HttpResponseMessage response = await HttpClient.SendAsync(request, ct);
		return await response.Content.ReadFromJsonAsync<Response>(SerializerOptions, ct);
	}

	public Task<TokenResult?> LoginAsync(string username, string password) => LoginAsync(username, password, CancellationToken.None);
	public async Task<TokenResult?> LoginAsync(string username, string password, CancellationToken ct)
	{
		using HttpRequestMessage request = new(HttpMethod.Post, "/api/v3/auth/login")
		{
			Content = JsonContent.Create(new LoginModel
			{
				Username = username,
				Password = password
			},

			options: SerializerOptions)
		};

		using HttpResponseMessage response = await HttpClient.SendAsync(request, ct);

		if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
		{
			throw new HttpRequestException("Authentication Failed. Please check and update your credentials.", null, response.StatusCode);
		}
		else
		{
			return (await response.Content.ReadFromJsonAsync<Response<TokenResult>>(SerializerOptions, ct))?.Details;
		}
	}
}
