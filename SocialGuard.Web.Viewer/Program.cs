using System.Text.Json.Serialization;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SocialGuard.Client.Http.Services;
using SocialGuard.Web.Viewer;
using SocialGuard.Web.Viewer.Services;


WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddHttpClient("");

builder.Services.AddSingleton<EmitterClient>();
builder.Services.AddSingleton<TrustlistClient>();

builder.Services.AddBlazoredLocalStorage(options =>
	{
		options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
	}
);

builder.Services.AddScoped<ApiAuthenticationService>();

WebAssemblyHost host = builder.Build();
await host.RunAsync();
