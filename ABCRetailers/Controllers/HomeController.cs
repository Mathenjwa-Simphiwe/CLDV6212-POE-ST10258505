using ABCRetailers.Models;
using ABCRetailers.Services;
using ABCRetailers.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ABCRetailers.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IAzureStorageService _storageService;

        public HomeController(ILogger<HomeController> logger, IAzureStorageService storageService)
        {
            _logger = logger;
            _storageService = storageService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var viewModel = new HomeViewModel
                {
                    FeaturedProducts = (await _storageService.GetEntitiesAsync<Product>("Products"))
                                        ?.OrderByDescending(p => p.Price)
                                        .Take(5)
                                        .ToList() ?? new List<Product>(),
                    CustomerCount = (await _storageService.GetEntitiesAsync<Customer>("Customers"))?.Count ?? 0,
                    ProductCount = (await _storageService.GetEntitiesAsync<Product>("Products"))?.Count ?? 0,
                    OrderCount = (await _storageService.GetEntitiesAsync<Order>("Orders"))?.Count ?? 0
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading home page data");

                // Return a view with empty data instead of crashing
                var emptyViewModel = new HomeViewModel
                {
                    FeaturedProducts = new List<Product>(),
                    CustomerCount = 0,
                    ProductCount = 0,
                    OrderCount = 0
                };

                return View(emptyViewModel);
            }
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