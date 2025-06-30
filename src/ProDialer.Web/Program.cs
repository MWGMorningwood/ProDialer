using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure HttpClient for API calls
if (builder.HostEnvironment.IsDevelopment())
{
    // Local development - point to Azure Functions local runtime
    builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:7071/") });
}
else
{
    // Production - will be configured for deployed Azure Functions URL
    builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
}

await builder.Build().RunAsync();
