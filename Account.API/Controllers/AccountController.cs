using Account.API.Models.Profile;
using Account.API.Services;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Account.API.Models.Profile.Utils;

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

        /// <summary>
        /// Get or create a profile based on user ID.
        /// </summary>
        [HttpGet("GetOrCreateMyProfile/{userId}")]
        public async Task<ActionResult<ProfileModel>> GetOrCreateMyProfileAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return BadRequest("User ID is required.");

            var profile = await _accountService.GetOrCreateProfileAsync(userId);

            if (profile == null)
                return NotFound("Failed to retrieve or create profile.");

            return Ok(profile);
        }

        /// <summary>
        /// Set a username for a profile, if not already set.
        /// </summary>
        [HttpPatch("SetUsername")]
        public async Task<IActionResult> SetUsername([FromBody] SetUsernameRequest request)
        {
            var profile = await _accountService.GetProfileAsync(request.UserId);
            if (profile == null)
                return NotFound("Profile not found.");

            if (await _accountService.IsUsernameInUse(request.Username))
                return Conflict("Username is already in use.");

            // Call SetUsernameAsync and capture the response
            var response = await _accountService.SetUsernameAsync(request.UserId, request.Username);

            // Return the SetUsernameResponse object as JSON
            return Ok(response);
        }


        /// <summary>
        /// Update the profile picture for a user.
        /// </summary>
        [HttpPatch("SetProfilePicture")]
        public async Task<IActionResult> SetProfilePicture([FromBody] SetProfilePictureRequest request)
        {
            if (string.IsNullOrEmpty(request.UserId))
                return BadRequest("User ID is required.");

            var success = await _accountService.UpdateProfilePictureAsync(request.UserId, request.ProfilePictureId);
            return success ? Ok("Profile picture updated successfully.") : BadRequest("Failed to update profile picture.");
        }

        /// <summary>
        /// Update the banner picture for a user.
        /// </summary>
        [HttpPatch("SetBannerPicture")]
        public async Task<IActionResult> SetBannerPicture([FromBody] SetBannerPictureRequest request)
        {
            if (string.IsNullOrEmpty(request.UserId))
                return BadRequest("User ID is required.");

            var success = await _accountService.UpdateBannerPictureAsync(request.UserId, request.BannerPictureId);
            return success ? Ok("Banner picture updated successfully.") : BadRequest("Failed to update banner picture.");
        }

        #endregion

        #region Currency Management

        /// <summary>
        /// Add currency (regular or premium) to the user's profile.
        /// </summary>
        [HttpPatch("AddCurrency")]
        public async Task<IActionResult> AddCurrency([FromBody] AddCurrencyRequest request)
        {
            if (string.IsNullOrEmpty(request.UserId))
                return BadRequest("User ID is required.");

            var success = await _accountService.AddCurrencyAsync(request.UserId, request.Amount, request.IsPremium);
            return success ? Ok("Currency added successfully.") : BadRequest("Failed to add currency.");
        }

        /// <summary>
        /// Consume currency (regular or premium) from the user's profile.
        /// </summary>
        [HttpPatch("ConsumeCurrency")]
        public async Task<IActionResult> ConsumeCurrency([FromBody] ConsumeCurrencyRequest request)
        {
            if (string.IsNullOrEmpty(request.UserId))
                return BadRequest("User ID is required.");

            var success = await _accountService.ConsumeCurrencyAsync(request.UserId, request.Amount, request.IsPremium);
            return success ? Ok("Currency consumed successfully.") : BadRequest("Insufficient currency or failed to consume currency.");
        }

        #endregion
    }
}
