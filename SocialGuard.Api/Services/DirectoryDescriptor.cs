
namespace SocialGuard.Api.Services;
public class DirectoryDescriptor
{
	private readonly IConfiguration _configuration;

	public DirectoryDescriptor(IConfiguration configuration)
	{
		_configuration = configuration;
	}

	public Common.Data.Models.Directory GetCurrentDirectoryInfo()
	{

	}
}
