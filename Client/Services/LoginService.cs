using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using WalletPWA.Shared;

namespace WalletPWA.Client.Services
{
    public class LoginService : ILoginService
    {
        public string Email { get; set; }
        public string Password { get; set; }

        private readonly HttpClient _httpClient;

        public LoginService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<AuthenticationResponse> LoginUser(User user)
        {
            //var result = await _httpClient.PostAsJsonAsync<User>("/user/loginuser", user);
            //return result;

            //creating authentication request
            AuthenticationRequest authenticationRequest = new AuthenticationRequest();
            authenticationRequest.Email = user.Email;
            authenticationRequest.Password = user.Password;

            //authenticating the request
            var httpMessageReponse = await _httpClient.PostAsJsonAsync<AuthenticationRequest>($"user/authenticatejwt", authenticationRequest);
            //sending the token to the client to store
            return await httpMessageReponse.Content.ReadFromJsonAsync<AuthenticationResponse>();
        }

        public async Task LogoutUser()
        {

            await _httpClient.GetFromJsonAsync<User>("/user/logoutuser");
        }

        public async Task<string> SingUp(User user)
        {
            var result = await _httpClient.PostAsJsonAsync("/user/registeruser", user);
            return await result.Content.ReadAsStringAsync();
        }
    }
}
