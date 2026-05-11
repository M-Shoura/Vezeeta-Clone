using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers
{
    public class MiddlewareTestController : Controller
    {
        [HttpGet("ok")]
        public IActionResult OK()
        {
            return Json(new { message = "Middleware pipeline is working." });
        }

        [HttpGet("throw")]
        public IActionResult Throw()
        {
            throw new InvalidOperationException("Test exception from middleware endpoint.");
        }
    }
}
