using Multiplayer.API.Models;
using Multiplayer.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Multiplayer.API.Controllers
{
    [ApiController]
    [Route("api/[Controller]/v1/")]
    public class LongdriveController : Controller
    {
        private readonly LongdriveService _longdriveService;

        public LongdriveController(LongdriveService longdriveService) => _longdriveService = longdriveService;

        // GET: Get list of long drive data
        [HttpGet]
        public async Task<List<LongDriveModel>> Get() =>
            await _longdriveService.GetAsync();

        // GET: Get specific long drive data
        [HttpGet("{id:length(24)}")]
        public async Task<ActionResult<LongDriveModel>> Get(string id)
        {
            var longdrive = await _longdriveService.GetAsync(id);
            if (longdrive is null)
                return NotFound();

            return longdrive;
        }

        // GET: Get random long drive data
        [HttpGet("single")]
        public async Task<ActionResult<LongDriveModel>> GetRandomLongdrive()
        {
            var result = await _longdriveService.GetRandomAsync();
            if(result is null)
                return NotFound();

            return result;
        }

        // POST: Create new long drive data
        [HttpPost]
        public async Task<ActionResult> Post(LongDriveModel newLongdrive)
        {
            await _longdriveService.CreateAsync(newLongdrive);

            return CreatedAtAction(nameof(Get), new {id = newLongdrive.id}, newLongdrive);
        }

        // GET: LongdriveController/Details/5
        //public ActionResult Details(int id)
        //{
        //    return View();
        //}

        // GET: LongdriveController/Create
        //public ActionResult Create()
        //{
        //    return View();
        //}

        //// POST: LongdriveController/Create
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Create(IFormCollection collection)
        //{
        //    try
        //    {
        //        return RedirectToAction(nameof(Index));
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}

        //// GET: LongdriveController/Edit/5
        //public ActionResult Edit(int id)
        //{
        //    return View();
        //}

        //// POST: LongdriveController/Edit/5
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit(int id, IFormCollection collection)
        //{
        //    try
        //    {
        //        return RedirectToAction(nameof(Index));
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}

        //// GET: LongdriveController/Delete/5
        //public ActionResult Delete(int id)
        //{
        //    return View();
        //}

        //// POST: LongdriveController/Delete/5
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Delete(int id, IFormCollection collection)
        //{
        //    try
        //    {
        //        return RedirectToAction(nameof(Index));
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}
    }
}
