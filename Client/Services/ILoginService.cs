using System.Threading.Tasks;
using WalletPWA.Shared;

namespace WalletPWA.Client.Services
{
    public interface ILoginService
    {
        public string Email { get; set; }
        public string Password { get; set; }

        public Task<AuthenticationResponse> LoginUser(User user);
        public Task LogoutUser();
        public Task<string> SingUp(User user);
    }
}
