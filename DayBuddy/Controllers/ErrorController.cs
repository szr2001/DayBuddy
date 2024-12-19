using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using MongoDB.Bson.IO;

namespace DayBuddy.Controllers
{
    public class ErrorController : Controller
    {
        public IActionResult HandleError(int code)
        {
            switch (code)
            {
                case StatusCodes.Status404NotFound:
                    return RedirectToAction(nameof(NothingFound));

                case StatusCodes.Status401Unauthorized:
                    return RedirectToAction(nameof(AccessDenied));

                case StatusCodes.Status429TooManyRequests:
                    return RedirectToAction(nameof(TooManyRequests));
            }

            return RedirectToAction(nameof(Error));
        }

        public IActionResult TooManyRequests()
        {
            return View();
        }

        public IActionResult NothingFound()
        {
            return View();
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
