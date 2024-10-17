using Item.API.Models.Clubs;
using Item.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Item.API.Controllers
{
    [ApiController]
    [Route("api/v1/[Controller]/")]
    public class ItemsController : Controller
    {
        //// VIEW ACTIONS
        //public IActionResult Apparel()
        //{
        //    return View();
        //}

        // API ACTIONS
        private readonly ItemService _itemService;

        public ItemsController(ItemService service) => _itemService = service;

        [HttpGet]
        public async Task<List<Scaling>> GetScaling() =>
            await _itemService.GetScalingsAsync();
    }
}
