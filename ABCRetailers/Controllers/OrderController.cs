using ABCRetailers.Models;
using ABCRetailers.Services;
using ABCRetailers.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ABCRetailers.Controllers
{
    public class OrderController : Controller
    {
        private readonly IAzureStorageService _storageService;

        public OrderController(IAzureStorageService storageService)
        {
            _storageService = storageService;
        }

        public async Task<IActionResult> Index()
        {
            var orders = await _storageService.GetEntitiesAsync<Order>("Orders");
            return View(orders);
        }

        public async Task<IActionResult> Create()
        {
            var viewModel = new OrderCreateViewModel
            {
                Customers = await _storageService.GetEntitiesAsync<Customer>("Customers"),
                Products = await _storageService.GetEntitiesAsync<Product>("Products")
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
       
        public async Task<IActionResult> Create(OrderCreateViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var product = await _storageService.GetEntityAsync<Product>("Products", "Product", viewModel.ProductId);
                var customer = await _storageService.GetEntityAsync<Customer>("Customers", "Customer", viewModel.CustomerId);

                if (product == null || customer == null)
                {
                    ModelState.AddModelError("", "Invalid product or customer selected.");
                    viewModel.Customers = await _storageService.GetEntitiesAsync<Customer>("Customers");
                    viewModel.Products = await _storageService.GetEntitiesAsync<Product>("Products");
                    return View(viewModel);
                }

                if (viewModel.Quantity > product.StockAvailable)
                {
                    ModelState.AddModelError("Quantity", "Insufficient stock available.");
                    viewModel.Customers = await _storageService.GetEntitiesAsync<Customer>("Customers");
                    viewModel.Products = await _storageService.GetEntitiesAsync<Product>("Products");
                    return View(viewModel);
                }

                var order = new Order
                {
                    OrderId = Guid.NewGuid().ToString(),
                    CustomerId = viewModel.CustomerId,
                    Username = customer.Username,
                    ProductId = viewModel.ProductId,
                    ProductName = product.ProductName,
                    OrderDate = viewModel.OrderDate, // This will be converted to UTC in the setter
                    Quantity = viewModel.Quantity,
                    UnitPrice = product.Price,
                    TotalPrice = product.Price * viewModel.Quantity,
                    Status = viewModel.Status
                };

                // Update product stock
                product.StockAvailable -= viewModel.Quantity;
                await _storageService.UpsertEntityAsync("Products", product);

                await _storageService.UpsertEntityAsync("Orders", order);

                // Send notification
                await _storageService.SendMessageAsync("orders", $"New order created: {order.OrderId}");

                return RedirectToAction(nameof(Index));
            }

            viewModel.Customers = await _storageService.GetEntitiesAsync<Customer>("Customers");
            viewModel.Products = await _storageService.GetEntitiesAsync<Product>("Products");
            return View(viewModel);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var order = await _storageService.GetEntityAsync<Order>("Orders", "Order", id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Order order)
        {
            if (id != order.OrderId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Ensure the order date is in UTC
                order.OrderDate = DateTime.SpecifyKind(order.OrderDate, DateTimeKind.Utc);
                await _storageService.UpsertEntityAsync("Orders", order);
                return RedirectToAction(nameof(Index));
            }
            return View(order);
        }

        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var order = await _storageService.GetEntityAsync<Order>("Orders", "Order", id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            await _storageService.DeleteEntityAsync("Orders", "Order", id);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<JsonResult> GetProductPrice(string productId)
        {
            if (string.IsNullOrEmpty(productId))
            {
                return Json(new { success = false });
            }

            var product = await _storageService.GetEntityAsync<Product>("Products", "Product", productId);
            if (product == null)
            {
                return Json(new { success = false });
            }

            return Json(new { success = true, price = product.Price, stock = product.StockAvailable });
        }
    }
}
