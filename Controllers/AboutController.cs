using Microsoft.AspNetCore.Mvc;

namespace MyMvcApp.Controllers
{
    public class AboutController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
