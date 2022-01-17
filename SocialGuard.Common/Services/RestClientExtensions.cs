using System.Net.Http;
using System.Net.Http.Headers;



namespace SocialGuard.Common.Services;

public static class RestClientExtensions
{
	public static HttpRequestMessage WithAuthentication(this HttpRequestMessage message, string token)
	{
		message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
		return message;
	}


}
