using Azure.Data.Tables;

using ABCRetailers.Models;


namespace ABCRetailers.Services
{
    public interface IAzureStorageService
    {
        // Table operations
        Task<T> GetEntityAsync<T>(string tableName, string partitionKey, string rowKey) where T : class, ITableEntity, new();
        Task<List<T>> GetEntitiesAsync<T>(string tableName, string partitionKey = null) where T : class, ITableEntity, new();
        Task UpsertEntityAsync<T>(string tableName, T entity) where T : ITableEntity;
        Task DeleteEntityAsync(string tableName, string partitionKey, string rowKey);

        // Blob operations
        Task<string> UploadFileAsync(IFormFile file, string containerName);
        Task<bool> DeleteFileAsync(string fileName, string containerName);

        // Queue operations
        Task SendMessageAsync(string queueName, string message);
        Task<string> ReceiveMessageAsync(string queueName);

        // File Share operations
        Task UploadFileToShareAsync(IFormFile file, string shareName, string directoryPath = "");
        Task<byte[]> DownloadFileFromShareAsync(string fileName, string shareName, string directoryPath = "");

        // Initialization
        Task InitializeAsync();
    }
}