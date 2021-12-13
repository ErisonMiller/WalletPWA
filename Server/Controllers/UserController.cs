using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using WalletPWA.Server.Data;
using WalletPWA.Shared;

namespace WalletPWA.Server.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> logger;
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;
        public UserController(ILogger<UserController> logger, DataContext context, IConfiguration configuration)
        {
            this.logger = logger;
            this._context = context;
            this._configuration = configuration;
        }

        //Authentication Methods
        [HttpPost("registeruser")]
        public async Task<ActionResult<String>> RegisterUser(User user)
        {
            User loggedInUser = await _context.users.Where(u => u.Email == user.Email).FirstOrDefaultAsync();

            if (loggedInUser == null)
            {
                user.Password = Utility.Encrypt(user.Password);
                _context.users.Add(user);
                await _context.SaveChangesAsync();
                return "Success";
            }

            return "Email already used";

        }


        //Authentication Methods
        [HttpPost("loginuser")]
        public async Task<ActionResult<User>> LoginUser(User user)
        {
            User loggedInUser = await _context.users.Where(u => u.Email == user.Email && u.Password == user.Password).FirstOrDefaultAsync();

            if (loggedInUser != null)
            {
                //create a claim
                var claim = new Claim(ClaimTypes.Name, loggedInUser.Email);
                //create claimsIdentity
                var claimsIdentity = new ClaimsIdentity(new[] { claim }, "serverAuth");
                //create claimsPrincipal
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                //Sign In User
                await HttpContext.SignInAsync(claimsPrincipal);
            }

            return await Task.FromResult(loggedInUser);

        }

        [HttpGet("getcurrentuser")]
        public async Task<ActionResult<User>> GetCurrentUser()
        {
            User currentUser = new User();

            if (User.Identity.IsAuthenticated)
            {
                currentUser.Email = User.FindFirstValue(ClaimTypes.Name);

            }
            return await Task.FromResult(currentUser);
        }

        [HttpGet("logoutuser")]
        public async Task<ActionResult<String>> LogOutUser()
        {
            await HttpContext.SignOutAsync();
            return "Success";
        }

        //[HttpPut("updateprofile/{userId}")]
        //public async Task<User> UpdateProfile(int userId, [FromBody] User user)
        //{
        //    User userToUpdate = await _context.Users.Where(u => u.UserId == userId).FirstOrDefaultAsync();
        //
        //    userToUpdate.FirstName = user.FirstName;
        //    userToUpdate.LastName = user.LastName;
        //    userToUpdate.EmailAddress = user.EmailAddress;
        //    userToUpdate.AboutMe = user.AboutMe;
        //
        //    await _context.SaveChangesAsync();
        //
        //    return await Task.FromResult(user);
        //}

        [HttpGet("getprofile/{userId}")]
        public async Task<User> GetProfile(Guid userId)
        {
            return await _context.users.Where(u => u.UserId == userId).FirstOrDefaultAsync();
        }


        public AuthenticationProperties GetAuthenticationProperties(bool isPersistent = false)
        {
            return new AuthenticationProperties()
            {
                IsPersistent = isPersistent,
                //ExpiresUtc = DateTime.UtcNow.AddMinutes(10),
                RedirectUri = "/profile"
            };
        }
        [HttpGet("notauthorized")]
        public IActionResult NotAuthorized()
        {
            return Unauthorized();
        }

        //Migrating to JWT Authorization...
        private string GenerateJwtToken(User user)
        {
            //getting the secret key
            string secretKey = _configuration["JWTSettings:SecretKey"];
            var key = Encoding.ASCII.GetBytes(secretKey);

            //create claims
            var claimEmail = new Claim(ClaimTypes.Email, user.Email);
            var claimNameIdentifier = new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString());

            //create claimsIdentity
            var claimsIdentity = new ClaimsIdentity(new[] { claimEmail, claimNameIdentifier }, "serverAuth");

            // generate token that is valid for 7 days
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = claimsIdentity,
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            //creating a token handler
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            //returning the token back
            return tokenHandler.WriteToken(token);
        }

        [HttpPost("authenticatejwt")]
        public async Task<ActionResult<AuthenticationResponse>> AuthenticateJWT(AuthenticationRequest authenticationRequest)
        {
            string token = string.Empty;

            //checking if the user exists in the database
            authenticationRequest.Password = Utility.Encrypt(authenticationRequest.Password);
            User loggedInUser = await _context.users
                        .Where(u => u.Email == authenticationRequest.Email && u.Password == authenticationRequest.Password)
                        .FirstOrDefaultAsync();

            if (loggedInUser != null)
            {
                //generating the token
                token = GenerateJwtToken(loggedInUser);
            }
            return await Task.FromResult(new AuthenticationResponse() { Token = token });
        }
        [HttpGet("getuserbyjwt/{jwtToken}")]
        public async Task<ActionResult<User>> GetUserByJWT(string jwtToken)
        {
            try
            {
                jwtToken = jwtToken.Trim('"');
                //getting the secret key
                string secretKey = _configuration["JWTSettings:SecretKey"];
                var key = Encoding.ASCII.GetBytes(secretKey);

                //preparing the validation parameters
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
                var tokenHandler = new JwtSecurityTokenHandler();
                SecurityToken securityToken;

                //validating the token
                var principle = tokenHandler.ValidateToken(jwtToken, tokenValidationParameters, out securityToken);
                var jwtSecurityToken = (JwtSecurityToken)securityToken;

                if (jwtSecurityToken != null && jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    //returning the user if found
                    var userId = principle.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    return await _context.users.Where(u => u.UserId == Guid.Parse(userId)).FirstOrDefaultAsync();
                }
            }
            catch (Exception ex)
            {
                //logging the error and returning null
                Console.WriteLine("Exception : " + ex.Message);
                return new User();
            }
            //returning null if token is not validated
            return new User();
        }
    }
}
