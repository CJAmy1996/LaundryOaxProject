using Microsoft.AspNetCore.Mvc;

namespace LaundryOaxWebAPI.Controllers
{
    public class ProductController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
