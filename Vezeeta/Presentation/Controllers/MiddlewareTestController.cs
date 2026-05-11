using Microsoft.AspNetCore.Mvc;
using System;

namespace Presentation.Controllers
{
    [Route("middleware-test")]
    public class MiddlewareTestController : Controller
    {
        [HttpGet("ok")]
        public IActionResult Ok()
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
