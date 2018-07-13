using Autofac;
using Microsoft.AspNetCore.Mvc;

namespace ServiceApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly IAmPausable _pausible;

        public HomeController(IAmPausable pausible)
        {
            _pausible = pausible;
        }

        public IActionResult Index()
        {
            ViewBag.ServiceStatus = _pausible.CurrentStatus == Status.Paused ? "paused" : "not paused";
            return View();
        }

        [HttpPost]
        public IActionResult Change()
        {
            if(_pausible.CurrentStatus == Status.Paused)
            {
                _pausible.Unpause();
            }
            else
            {
                _pausible.Pause();
            }

            return RedirectToAction("Index");
        }
    }
}