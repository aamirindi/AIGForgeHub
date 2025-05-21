using BDR.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BDR.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }


        // super Admin
        public IActionResult SuperAdminDashboard()
        {
            return View();
        }

        // Admin
        public IActionResult AdminDashboard()
        {
            return View();
        }
        // Vendor
        public IActionResult VendorDashboard()
        {
            return View();
        }

        // Buyer
        public IActionResult BuyerDashboard()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
