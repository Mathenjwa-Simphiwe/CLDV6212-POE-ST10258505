using System.Linq;
using ABCRetailers.Models;
using ABCRetailers.Services;
using Microsoft.AspNetCore.Mvc;

namespace ABCRetailers.Controllers
{
    public class UploadController : Controller
    {
        private readonly IAzureStorageService _storageService;

        public UploadController(IAzureStorageService storageService)
        {
            _storageService = storageService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(FileUploadModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.ProofOfPayment != null && model.ProofOfPayment.Length > 0)
                {
                    var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
                    var extension = Path.GetExtension(model.ProofOfPayment.FileName).ToLower();

                    if (!allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError("ProofOfPayment", "Only PDF, JPG, JPEG, and PNG files are allowed.");
                        return View(model);
                    }

                    // Upload file to blob storage
                    await _storageService.UploadFileAsync(model.ProofOfPayment, "paymentproofs");

                    // Also store in file share for contracts
                    
                    await _storageService.UploadFileToShareAsync(model.ProofOfPayment, "contracts", "payment-proofs");

                    ViewBag.Message = "File uploaded successfully!";
                    return View();
                }
            }

            return View(model);
        }
    }
}