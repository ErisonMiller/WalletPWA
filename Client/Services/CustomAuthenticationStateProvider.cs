using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading.Tasks;
using WalletPWA.Shared;

namespace WalletPWA.Client.Services
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorageService;

        public CustomAuthenticationStateProvider(HttpClient httpClient, ILocalStorageService localStorageService)
        {
            _httpClient = httpClient;
            _localStorageService = localStorageService;
        }

        public async override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            User currentUser = await GetUserByJWTAsync(); //_httpClient.GetFromJsonAsync<User>("api/user/getcurrentuser");

            if (currentUser != null && currentUser.Email != null)
            {
                //create a claim
                var claim = new Claim(ClaimTypes.Name, currentUser.Email);
                //create claimsIdentity
                var claimsIdentity = new ClaimsIdentity(new[] { claim }, "serverAuth");
                //create claimsPrincipal
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                return new AuthenticationState(claimsPrincipal);
            }
            else
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        public async Task<User> GetUserByJWTAsync()
        {
            //pulling the token from localStorage
            var jwtToken = await _localStorageService.GetItemAsStringAsync("jwt_token");
            if (jwtToken == null) return null;

            //preparing the http request
            //var requestMessage = new HttpRequestMessage(HttpMethod.Post, "user/getuserbyjwt");
            //requestMessage.Content = new StringContent(jwtToken);
            //
            //requestMessage.Content.Headers.ContentType
            //    = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            ////making the http request
            //var response = await _httpClient.SendAsync(requestMessage);
            var returnedUser = await _httpClient.GetFromJsonAsync<User>("user/getuserbyjwt/" + jwtToken);

            //var responseStatusCode = response.StatusCode;
            //var returnedUser = await response.Content.ReadFromJsonAsync<User>();

            //returning the user if found
            if (returnedUser != null) return await Task.FromResult(returnedUser);
            else return null;
        }
    }
}
