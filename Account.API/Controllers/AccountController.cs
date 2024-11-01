using Account.API.Models;
using Account.API.Models.Profile;
using Account.API.Services;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Threading.Tasks;
using Account.API.Models.Profile.Utils;
using MongoDB.Driver;

namespace Account.API.Controllers
{
    [ApiController]
    [Route("/api/v1/Account")]
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
            SecurityToken validatedToken;

            try
            {
                tokenHandler.ValidateToken(token, validationParams, out validatedToken);
                return validatedToken != null;
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion

        #region Profile Accessing Methods

        /// <summary>
        /// Get or create a profile based on user ID.
        /// </summary>
        [HttpPost("GetMyProfile")]
        public async Task<ActionResult<ProfileModel>> GetMyProfileAsync([FromBody] GetProfileRequest request)
        {
            if (string.IsNullOrEmpty(request.UserId))
                return BadRequest("User ID is required.");

            var profile = await _accountService.GetOrCreateProfileAsync(request.UserId);

            if (profile == null)
                return NotFound("Failed to retrieve or create profile.");

            return Ok(profile);
        }


        /// <summary>
        /// Set a username for a profile, if not already set.
        /// </summary>
        [HttpPost("SetUsername")]
        public async Task<IActionResult> SetUsername([FromBody] SetUsernameRequest request)
        {
            var profile = await _accountService.GetProfileAsync(request.UserId);
            if (profile is null)
                return NotFound("Profile not found.");

            if (await _accountService.IsUsernameInUse(request.Username))
                return Conflict("Username is already in use.");

            // Call SetUsernameAsync and capture the response
            var response = await _accountService.SetUsernameAsync(request.UserId, request.Username);

            // Return the SetUsernameResponse object as JSON
            return Ok(response);
        }


        [HttpPost("SetProfilePicture")]
        public async Task<IActionResult> SetProfilePicture([FromBody] SetProfilePictureRequest request)
        {
            if (string.IsNullOrEmpty(request.UserId))
                return BadRequest("User ID is required.");

            var success = await _accountService.UpdateProfilePictureAsync(request.UserId, request.ProfilePictureId);
            return success ? Ok("Profile picture updated successfully.") : BadRequest("Failed to update profile picture.");
        }

        [HttpPost("SetBannerPicture")]
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
        [HttpPost("AddCurrency")]
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
        [HttpPost("ConsumeCurrency")]
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
