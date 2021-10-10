using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SocialGuard.Common.Services
{
	public static class RestClientExtensions
	{
		public static HttpRequestMessage WithAuthentication(this HttpRequestMessage message, string token)
		{
			message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
			return message;
		}
	}
}
