using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using WalletPWA.Client.Handlers;
using WalletPWA.Client.Services;

namespace WalletPWA.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddOptions();
            builder.Services.AddAuthorizationCore();


            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            builder.Services.AddMudServices();
            builder.Services.AddBlazoredLocalStorage();

            //builder.Services.AddTransient<CustomAuthorizationHandler>();

            builder.Services.AddScoped<CustomAuthorizationHandler>();
            builder.Services.AddHttpClient("ApiClient", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
                .AddHttpMessageHandler<CustomAuthorizationHandler>();
            builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("ApiClient"));

            builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddScoped<ILoginService, LoginService>();

            builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();

            await builder.Build().RunAsync();
        }
    }
}
