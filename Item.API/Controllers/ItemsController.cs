using Microsoft.AspNetCore.Mvc;

namespace Item.API.Controllers
{
    public class ItemsController : Controller
    {
        public IActionResult Apparel()
        {
            return View();
        }
    }
}
