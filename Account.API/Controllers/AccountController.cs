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
        public static bool ValidateToken(string token, TokenValidationParameters validationParams)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken validatedToken = null!;

            try
            {
                tokenHandler.ValidateToken(token, validationParams, out validatedToken);
                Console.WriteLine(validatedToken);
                return validatedToken != null;
            }
            catch (Exception)
            {
                Console.WriteLine(validatedToken);
                return false;
            }
        }

        [HttpGet("Authenticate")]
        public async Task AuthenticateToken()
        {
            // SETUP UNITY TOKEN VERIFICATION VIA THEIR PROVIDED ENDPOINT
            string JWKSEndPoint = "https://player-auth.services.api.unity.com/.well-known/jwks.json";

            var response = await _http.GetHttpAsync(JWKSEndPoint);
            response.EnsureSuccessStatusCode();

            var keys = await response.Content.ReadAsStringAsync();

            var token = "eyJhdWQiOlsiaWRkOjY2NzFhNmNkLTdjNDEtNGY4MS1hNmQ2LWU2MGY0ZjhhY2M1OCIsImVudk5hbWU6cHJvZHVjdGlvbiIsImVudklkOmQwNmQ2MTM0LWNlYjQtNGM0OC1hNjZhLWQwNWI5OGYyMDFmMSIsInVwaWQ6ZGE4OGJlNmUtYTQyZC00OGIxLTg5MWEtZDkyMjkzNjU5MDdhIl0sImV4cCI6MTcyOTgyMDYxOCwiaWF0IjoxNzI5ODE3MDE4LCJpZGQiOiI2NjcxYTZjZC03YzQxLTRmODEtYTZkNi1lNjBmNGY4YWNjNTgiLCJpc3MiOiJodHRwczovL3BsYXllci1hdXRoLnNlcnZpY2VzLmFwaS51bml0eS5jb20iLCJqdGkiOiIyZWQ3MzY1Ni03ZjFlLTRjZWItOGUwOC00N2UxMDk2MzFlY2YiLCJuYmYiOjE3Mjk4MTcwMTgsInByb2plY3RfaWQiOiJkYTg4YmU2ZS1hNDJkLTQ4YjEtODkxYS1kOTIyOTM2NTkwN2EiLCJzaWduX2luX3Byb3ZpZGVyIjoiYW5vbnltb3VzIiwic3ViIjoibVBVbnFuSU8wZExJSkl1eDFYOFRjMDFWYllGOCIsInRva2VuX3R5cGUiOiJhdXRoZW50aWNhdGlvbiIsInZlcnNpb24iOiIxIn0";
            var jwks = new JsonWebKeySet(keys);
            var jwk = jwks.Keys.Last();

            var validationParams = new TokenValidationParameters
            {
                IssuerSigningKey = jwk,
                ValidAudience = "",
                ValidIssuer = ""
            };

            var isValid = ValidateToken(token, validationParams);

            Console.WriteLine(isValid);


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
