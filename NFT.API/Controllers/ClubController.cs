using Microsoft.AspNetCore.Mvc;
using NFT.API.Models.Clubs;
using NFT.API.Services;

namespace NFT.API.Controllers
{
    [ApiController]
    [Route("/api/v1/[Controller]/")]
    public class ClubController
    {
        private readonly NFTService _service;

        public ClubController(NFTService NftService) => _service = NftService;

        [HttpPost("/CraftClub/{rarity}")]
        public async Task<ActionResult<ClubDataModel>> CraftClub(int rarity)
        {
            var club = await _service.CraftClub(rarity);
            return club;
        }
    }
}
