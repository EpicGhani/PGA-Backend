using Account.API.Models;
using Account.API.Models.Profile;
using Account.API.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

namespace Account.API.Controllers
{
    [ApiController]
    [Route("/api/v1/[Controller]/")]
    public class AccountController : Controller
    {
        private readonly AccountService _accountService;
        private readonly HttpService _http;

        public AccountController(AccountService accountService, HttpService httpService) 
        { 
            _accountService = accountService;
            _http = httpService;
        }

        #region Token Validation
        public static int ValidateToken(string token, TokenValidationParameters validationParams)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken validatedToken = null!;

            try
            {
                tokenHandler.ValidateToken(token, validationParams, out validatedToken);
                Console.WriteLine(validatedToken);
                return validatedToken != null ? 0 : 1;
            }
            catch (SecurityTokenExpiredException)
            {
                return 2;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Token Validation Failed: {ex.Message} | \nWith Security Token: {validatedToken}.");
                return 3;
            }
        }

        [HttpPost("Authenticate")]
        public async Task<IActionResult> AuthenticateToken([FromBody]string token)
        {
            // SETUP UNITY TOKEN VERIFICATION VIA THEIR PROVIDED ENDPOINT
            string JWKSEndPoint = "https://player-auth.services.api.unity.com/.well-known/jwks.json";

            var response = await _http.GetHttpAsync(JWKSEndPoint);
            response.EnsureSuccessStatusCode();

            var keys = await response.Content.ReadAsStringAsync();
            var jwks = new JsonWebKeySet(keys);

            var validationParams = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = jwks.Keys,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = "https://player-auth.services.api.unity.com",
                ValidAudiences = new List<string>
                {
                    "idd:6671a6cd-7c41-4f81-a6d6-e60f4f8acc58",
                    "envName:production",
                    "envId:d06d6134-ceb4-4c48-a66a-d05b98f201f1",
                    "upid:da88be6e-a42d-48b1-891a-d9229365907a"
                }
            };

            var isValid = ValidateToken(token, validationParams);

            switch(isValid)
            {
                case 0:
                    default:
                    return Ok(new {message = "Token is Valid."});
                case 1:
                    return BadRequest();
                case 2:
                    return Ok(new {message = "Token is expired."});
                case 3:
                    return Ok(new {message = "An error occured with the provided token."});
            }
        }
        #endregion

        #region Profile Accessing Methods
        [HttpGet("{userId}")]
        public async Task<ActionResult<ProfileModel>> GetProfile(string userId) 
        {
            var data = await _accountService.GetProfileAsync(userId);
            if(data is null)
                return NotFound();

            return data;
        }

        [HttpPost]
        public async Task<IActionResult> CreateProfile(ProfileModel newProfileModel)
        {
            await _accountService.CreateProfileAsync(newProfileModel);
            return CreatedAtAction(nameof(GetProfile), new { userId = newProfileModel.Profile.UserId }, newProfileModel);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ProfileModel>> UpdateProfile(string id, ProfileModel updatedProfileModel)
        {
            var profile = await _accountService.GetProfileByIDAsync(id);

            if(profile is null)
                return NotFound();

            updatedProfileModel.id = profile.id;
            await _accountService.UpdateProfileAsync(id, updatedProfileModel);
            var updatedData = await _accountService.GetProfileAsync(updatedProfileModel.Profile.UserId);

            if (updatedData is null)
                return NotFound();

            return updatedData;
        }
        #endregion
    }
}
