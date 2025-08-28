using ABCRetailers.Models;
using Azure;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using Azure.Storage.Files.Shares;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Http;

namespace ABCRetailers.Services
{
    public class AzureStorageService : IAzureStorageService
    {
        private readonly string _connectionString;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly TableServiceClient _tableServiceClient;
        private readonly QueueServiceClient _queueServiceClient;
        private readonly ShareServiceClient _shareServiceClient;

        public AzureStorageService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("AzureStorage");
            _blobServiceClient = new BlobServiceClient(_connectionString);
            _tableServiceClient = new TableServiceClient(_connectionString);
            _queueServiceClient = new QueueServiceClient(_connectionString);
            _shareServiceClient = new ShareServiceClient(_connectionString);
        }

        public async Task InitializeAsync()
        {
            // Create tables if they don't exist
            await _tableServiceClient.CreateTableIfNotExistsAsync("Customers");
            await _tableServiceClient.CreateTableIfNotExistsAsync("Products");
            await _tableServiceClient.CreateTableIfNotExistsAsync("Orders");

            // Create blob containers if they don't exist
            await _blobServiceClient.CreateBlobContainerAsync("productimages");
            await _blobServiceClient.CreateBlobContainerAsync("paymentproofs");

            // Create queues if they don't exist
            await _queueServiceClient.CreateQueueAsync("orders");
            await _queueServiceClient.CreateQueueAsync("notifications");

            // Create file shares if they don't exist AND create directories
            var shareClient = _shareServiceClient.GetShareClient("contracts");
            await shareClient.CreateIfNotExistsAsync();

            // Create the payment-proofs directory in the contracts share
            var directoryClient = shareClient.GetDirectoryClient("payment-proofs");
            await directoryClient.CreateIfNotExistsAsync();
        }

        #region Table Operations
        public async Task<T> GetEntityAsync<T>(string tableName, string partitionKey, string rowKey) where T : class, ITableEntity, new()
        {
            var tableClient = _tableServiceClient.GetTableClient(tableName);
            return await tableClient.GetEntityAsync<T>(partitionKey, rowKey);
        }

        public async Task<List<T>> GetEntitiesAsync<T>(string tableName, string partitionKey = null) where T : class, ITableEntity, new()
        {
            var tableClient = _tableServiceClient.GetTableClient(tableName);
            var query = partitionKey != null
                ? tableClient.QueryAsync<T>(e => e.PartitionKey == partitionKey)
                : tableClient.QueryAsync<T>();

            var entities = new List<T>();
            await foreach (var entity in query)
            {
                entities.Add(entity);
            }
            return entities;
        }
        public async Task UpsertEntityAsync<T>(string tableName, T entity) where T : ITableEntity
        {
            var tableClient = _tableServiceClient.GetTableClient(tableName);

            // Ensure all DateTime properties are UTC
            if (entity is ITableEntity)
            {
                entity = EnsureUtcDateTime(entity);
            }

            await tableClient.UpsertEntityAsync(entity);
        }



        public async Task DeleteEntityAsync(string tableName, string partitionKey, string rowKey)
        {
            var tableClient = _tableServiceClient.GetTableClient(tableName);
            await tableClient.DeleteEntityAsync(partitionKey, rowKey);
        }

        private T EnsureUtcDateTime<T>(T entity) where T : ITableEntity
        {
            // Use reflection to find all DateTime properties and ensure they're UTC
            var properties = entity.GetType().GetProperties()
                .Where(p => p.PropertyType == typeof(DateTime));

            foreach (var property in properties)
            {
                if (property.CanRead && property.CanWrite)
                {
                    var value = (DateTime)property.GetValue(entity);
                    if (value.Kind != DateTimeKind.Utc)
                    {
                        property.SetValue(entity, DateTime.SpecifyKind(value, DateTimeKind.Utc));
                    }
                }
            }

            return entity;
        }
        // Update the UpsertEntityAsync method to use the helper

            #endregion

            #region Blob Operations
        public async Task<string> UploadFileAsync(IFormFile file, string containerName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(Guid.NewGuid().ToString() + Path.GetExtension(file.FileName));

            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, true);
            }

            return blobClient.Uri.ToString();
        }

        public async Task<bool> DeleteFileAsync(string fileName, string containerName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(fileName);
            return await blobClient.DeleteIfExistsAsync();
        }
        #endregion

        #region Queue Operations
        public async Task SendMessageAsync(string queueName, string message)
        {
            var queueClient = _queueServiceClient.GetQueueClient(queueName);
            await queueClient.SendMessageAsync(message);
        }

        public async Task<string> ReceiveMessageAsync(string queueName)
        {
            var queueClient = _queueServiceClient.GetQueueClient(queueName);
            var response = await queueClient.ReceiveMessageAsync();
            return response.Value?.MessageText;
        }
        #endregion

        #region File Share Operations
        public async Task UploadFileToShareAsync(IFormFile file, string shareName, string directoryPath = "")
        {
            var shareClient = _shareServiceClient.GetShareClient(shareName);

            // Create the share if it doesn't exist
            await shareClient.CreateIfNotExistsAsync();

            // Get the root directory if no directory path is specified
            ShareDirectoryClient directoryClient;

            if (string.IsNullOrEmpty(directoryPath))
            {
                directoryClient = shareClient.GetRootDirectoryClient();
            }
            else
            {
                // Create directory structure if it doesn't exist
                directoryClient = shareClient.GetDirectoryClient(directoryPath);
                await directoryClient.CreateIfNotExistsAsync();
            }

            var fileClient = directoryClient.GetFileClient(file.FileName);

            using (var stream = file.OpenReadStream())
            {
                await fileClient.CreateAsync(stream.Length);
                await fileClient.UploadRangeAsync(new HttpRange(0, stream.Length), stream);
            }
        }

        public async Task<byte[]> DownloadFileFromShareAsync(string fileName, string shareName, string directoryPath = "")
        {
            var shareClient = _shareServiceClient.GetShareClient(shareName);
            var directoryClient = shareClient.GetDirectoryClient(directoryPath);
            var fileClient = directoryClient.GetFileClient(fileName);

            var response = await fileClient.DownloadAsync();
            using (var memoryStream = new MemoryStream())
            {
                await response.Value.Content.CopyToAsync(memoryStream);
                return memoryStream.ToArray();
            }
        }
        #endregion
    }
}
