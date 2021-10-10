using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SocialGuard.Common.Services
{
	public abstract class RestClientBase
	{
		protected HttpClient HttpClient { get; init; }

		protected RestClientBase(HttpClient client)
		{
			HttpClient = client;
		}

		public virtual void SetBaseUri(Uri uri) => HttpClient.BaseAddress = uri;
	}
}
