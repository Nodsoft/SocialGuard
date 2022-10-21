using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SocialGuard.Client.Http.Services;
using SocialGuard.Web.Viewer;


WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddHttpClient("");

builder.Services.AddSingleton<EmitterClient>();
builder.Services.AddSingleton<TrustlistClient>();

WebAssemblyHost host = builder.Build();
await host.RunAsync();
