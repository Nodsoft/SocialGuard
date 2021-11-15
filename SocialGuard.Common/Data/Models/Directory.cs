using SocialGuard.Common.Infrastructure.Exceptions;

namespace SocialGuard.Common.Data.Models;

public record Directory
{
	public string? DisplayName { get; set; }

	[DisallowNull, Required]
	public string Domain { get; init; } = string.Empty;
	[DisallowNull, Required]
	public string DiscoveryEndpoint { get; init; } = string.Empty;

	public virtual IReadOnlyList<Directory>? RelativeDirectories { get; init; }


	public Uri GetDirectoryDiscoveryUri()
	{
		if (Uri.TryCreate($"https://{Domain}/{DiscoveryEndpoint}", UriKind.Absolute, out Uri? result))
		{
			return result;
		}
		
		throw new InvalidDirectoryUriException(Domain, DiscoveryEndpoint);
	}

	public static (string domain, string endpoint) GetDirectoryUriComponents(Uri endpointUri) => (endpointUri.Host, endpointUri.Query);
}
