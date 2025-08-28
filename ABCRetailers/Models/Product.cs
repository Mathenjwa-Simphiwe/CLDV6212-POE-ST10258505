using Azure;
using Azure.Data.Tables;

namespace ABCRetailers.Models
{
    public class Product : ITableEntity
    {
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockAvailable { get; set; }
        public string ImageUrl { get; set; } = string.Empty;

        // ITableEntity implementation
        public string PartitionKey { get; set; } = "Product";
        public string RowKey { get => ProductId; set => ProductId = value; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}