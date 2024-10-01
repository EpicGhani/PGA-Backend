using Account.API.Models;
using Account.API.Models.Profile;
using Account.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Account.API.Controllers
{
    [ApiController]
    [Route("/api/v1/[Controller]/")]
    public class AccountController : Controller
    {
        private readonly AccountService _accountService;

        public AccountController(AccountService accountService) => _accountService = accountService;

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
    }
}
