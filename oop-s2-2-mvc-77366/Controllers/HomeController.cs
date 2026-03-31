using Microsoft.AspNetCore.Mvc;
using FoodInspectionService.Models;
using System.Diagnostics;

namespace FoodInspectionService.Controllers
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
            _logger.LogInformation("Home page viewed.");
            return View();
        }

        public IActionResult Privacy()
        {
            _logger.LogInformation("Privacy page viewed.");
            return View();
        }

        public IActionResult TestError()
        {
            _logger.LogWarning("TestError endpoint triggered deliberately.");
            throw new Exception("Test exception for global error handling");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

            _logger.LogError("Global error page displayed. RequestId: {RequestId}", requestId);

            return View(new ErrorViewModel
            {
                RequestId = requestId
            });
        }
    }
}