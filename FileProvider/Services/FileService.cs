using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Data.Contexts;
using Data.Entities;
using FileProvider.Functions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
namespace FileProvider.Services;

public class FileService(ILogger<FileService> logger, DataContext context, BlobServiceClient client) : IFileService
{
    private readonly ILogger<FileService> _logger = logger;
    private readonly DataContext _context = context;
    private readonly BlobServiceClient _client = client;
    private BlobContainerClient? _container;

    public async Task SetBlobContainerAsync(string containerName)
    {
        try
        {
            _container = _client.GetBlobContainerClient(containerName);
            await _container.CreateIfNotExistsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR: FileService.SetBlobContainerAsync :: {ex.Message}");
        }
    }

    public string SetFileName(IFormFile file)
    {
        try
        {
            var fileName = $"{Guid.NewGuid()}.{file.FileName}";
            return fileName;
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR: FileService.SetFileName :: {ex.Message}");
            throw; 
        }
    }

    public async Task<string> UploadFileAsync(IFormFile file, FileEntity fileEntity)
    {
        try
        {
            BlobHttpHeaders headers = new()
            {
                ContentType = file.ContentType,
            };

            var blobClient = _container!.GetBlobClient(fileEntity.FileName);

            using var stream = file.OpenReadStream();
            await blobClient.UploadAsync(stream, headers);

            return blobClient.Uri.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR: FileService.UploadFileAsync :: {ex.Message}");
            throw; 
        }
    }

    public async Task SaveToDatabaseAsync(FileEntity fileEntity)
    {
        try
        {
            _context.Files.Add(fileEntity);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR: FileService.SaveToDatabaseAsync :: {ex.Message}");
            throw; 
        }
    }

    public async Task RemoveFileIfNotExistsAsync(FileEntity fileEntity)
    {
        try
        {
            var blobClient = _container!.GetBlobClient(fileEntity.FileName);
            var exists = await blobClient.ExistsAsync();

            if (!exists)
            {
                _context.Files.Remove(fileEntity);
                await _context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR: FileService.RemoveFileIfNotExistsAsync :: {ex.Message}");
            throw; 
        }
    }

}
