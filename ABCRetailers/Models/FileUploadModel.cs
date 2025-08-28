using Microsoft.AspNetCore.Http;

namespace ABCRetailers.Models
{
    public class FileUploadModel
    {
        public IFormFile ProofOfPayment { get; set; }
        public string OrderId { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
    }
}
