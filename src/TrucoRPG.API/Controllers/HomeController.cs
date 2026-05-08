using Microsoft.AspNetCore.Mvc;
using TrucoDemo.Models;

namespace TrucoDemo.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
