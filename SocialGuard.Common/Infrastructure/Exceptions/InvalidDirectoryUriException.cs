using SocialGuard.Common.Data.Models;
using System.Runtime.Serialization;


namespace SocialGuard.Common.Infrastructure.Exceptions;

[Serializable]
class InvalidDirectoryUriException : ApplicationException
{
	public InvalidDirectoryUriException(Data.Models.Directory directory) 
		: base($"Invalid Directory Uri: {directory.GetDirectoryDiscoveryUri()}") { }

	public InvalidDirectoryUriException(string domain, string discoveryEndpoint) 
		: base($"Invalid Directory Uri Segments: \nDomain: {domain} \nDiscovery Endpoint: {discoveryEndpoint}")	{ }

	protected InvalidDirectoryUriException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
