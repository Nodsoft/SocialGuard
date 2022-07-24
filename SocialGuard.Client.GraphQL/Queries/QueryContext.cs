using GraphQLinq;
using SocialGuard.Common.Data.Models;

namespace SocialGuard.Client.GraphQL
{
	public partial class QueryContext : GraphContext
	{
		public QueryContext(string baseUrl) : base(baseUrl, "") { }

		public QueryContext(HttpClient httpClient) : base(httpClient) { }

		public GraphCollectionQuery<TrustlistEntry> Entries(TrustlistEntryFilterInput where, List<TrustlistEntrySortInput> order)
		{
			object[] parameterValues = { where, order };

			return BuildCollectionQuery<TrustlistEntry>(parameterValues, "entries");
		}

		public GraphCollectionQuery<Emitter> Emitters(EmitterFilterInput where, List<EmitterSortInput> order)
		{
			object[] parameterValues = { where, order };

			return BuildCollectionQuery<Emitter>(parameterValues, "emitters");
		}
	}
}