using Azure;
using Azure.Data.Tables;

namespace ABCRetailers.Models
{
    public class Customer : ITableEntity
    {
        public string CustomerId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;

        // ITableEntity implementation
        public string PartitionKey { get; set; } = "Customer";
        public string RowKey { get => CustomerId; set => CustomerId = value; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}