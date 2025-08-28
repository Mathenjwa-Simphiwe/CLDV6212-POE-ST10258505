using Azure;
using Azure.Data.Tables;
using System;

namespace ABCRetailers.Models
{
    public class Order : ITableEntity
    {
        public string OrderId { get; set; } = string.Empty;
        public string CustomerId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;

        // Store as UTC for Azure Table Storage
        private DateTime _orderDate;
        public DateTime OrderDate
        {
            get => _orderDate;
            set => _orderDate = DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Processing, Completed, Cancelled

        // ITableEntity implementation
        public string PartitionKey { get; set; } = "Order";
        public string RowKey { get => OrderId; set => OrderId = value; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        // Constructor to ensure UTC time
        public Order()
        {
            _orderDate = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
        }
    }
}