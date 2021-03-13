namespace Transcom.SocialGuard.Api.Data.Models
{
	public record AccessKey
	{
		public string Id { get; init; }

		public string OwnerName { get; set; }

		public string[] Scopes { get; set; }

		public bool Active { get; set; }
	}
}
