using Microsoft.Extensions.DependencyInjection;

namespace SocialGuard.Client.GraphQL;

public static class DependencyInjectionExtensions
{
	public static IServiceCollection AddSocialGuard(this IServiceCollection services)
	{
		return services;
	}
}