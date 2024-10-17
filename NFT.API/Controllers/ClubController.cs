using Microsoft.AspNetCore.Mvc;
using NFT.API.Models.Clubs;
using NFT.API.Services;

namespace NFT.API.Controllers
{
    [ApiController]
    [Route("/api/v1/[Controller]/")]
    public class ClubController : Controller
    {
        private readonly NFTService _service;

        public ClubController(NFTService NftService) => _service = NftService;

        [HttpPost("CraftClub")]
        public async Task<ActionResult<ClubDataModel>> CraftClub(string headId, string shaftId, string gripId)
        {
            var head = await _service.GetHeadDataByIdAsync(headId);
            var shaft = await _service.GetShaftDataByIdAsync(shaftId);
            var grip = await _service.GetGripDataByIdAsync(gripId);

            if (head is null) return NotFound(new {message = $"No head part found with ID: {headId}" });
            if (shaft is null) return NotFound(new {message = $"No shaft part found with ID: {shaftId}" });
            if (grip is null) return NotFound(new {message = $"No grip part found with ID: {gripId}" });

            if (!head.brandId.Equals(shaft.brandId) || !shaft.brandId.Equals(grip.brandId) || !grip.brandId.Equals(head.brandId))
                return BadRequest(new { message = "Head, Shaft, and Grip must be of the same Brand" });

            var clubData = new ClubDataModel(head,shaft,grip,head.brandId);

            return Ok(clubData);
        }
    }
}
