using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialGuard.Common.Services
{
	class TrustlistClient : RestClientBase
	{
		public TrustlistClient(HttpClient httpClient) : base(httpClient) { }
	}
}
