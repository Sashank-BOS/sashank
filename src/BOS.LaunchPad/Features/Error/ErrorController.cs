using Microsoft.AspNetCore.Mvc;

namespace BOS.LaunchPad.Features.Error
{
    public class ErrorController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}