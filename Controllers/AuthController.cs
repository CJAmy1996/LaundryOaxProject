using Microsoft.AspNetCore.Mvc;

namespace LaundryOaxWebAPI.Controllers
{
    public class AuthController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
