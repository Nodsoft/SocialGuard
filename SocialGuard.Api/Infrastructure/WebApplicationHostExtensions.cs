using System.Configuration;
using System.Net;
using Microsoft.AspNetCore.HttpOverrides;

namespace SocialGuard.Api.Infrastructure;

/// <summary>
/// Defines extension methods for configuring the WebApplication host and services.
/// </summary>
public static class WebApplicationHostExtensions
{
    
        /// <summary>
        /// Configures the application to support reverse proxy headers,
        /// allowing it to correctly identify client IP addresses and protocols when behind a reverse proxy.
        /// </summary>
        /// <param name="app">The IApplicationBuilder to configure.</param>
        /// <param name="allowedProxies">An optional collection of IP addresses representing trusted reverse proxies. If provided, only headers from these proxies will be processed.</param>
        /// <returns>The configured IApplicationBuilder.</returns>
    public static IApplicationBuilder ConfigureReverseProxySupport(this IApplicationBuilder app, IReadOnlyCollection<IPAddress> allowedProxies = null)
    {
        ForwardedHeadersOptions forwardedHeadersOptions = new()
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
        };

        if (allowedProxies is { Count: > 0 })
        {
            forwardedHeadersOptions.KnownProxies.Clear();

            foreach (IPAddress address in allowedProxies)
            {
                forwardedHeadersOptions.KnownProxies.Add(address);
            }
        }
        
        return app.UseForwardedHeaders(forwardedHeadersOptions);
    }
}