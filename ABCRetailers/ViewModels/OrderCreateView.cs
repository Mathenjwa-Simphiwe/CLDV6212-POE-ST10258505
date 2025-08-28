using ABCRetailers.Models;
using System.ComponentModel.DataAnnotations;

namespace ABCRetailers.ViewModels
{
    public class OrderCreateViewModel
    {
        [Required]
        public string CustomerId { get; set; } = string.Empty;
        
        [Required]
        public string ProductId { get; set; } = string.Empty;
        
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }
        
        // Store as UTC for Azure compatibility
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        
        public string Status { get; set; } = "Pending";
        
        public List<Customer> Customers { get; set; } = new List<Customer>();
        public List<Product> Products { get; set; } = new List<Product>();
    }
}