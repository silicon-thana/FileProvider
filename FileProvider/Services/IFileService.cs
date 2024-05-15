
using Data.Entities;
using Microsoft.AspNetCore.Http;

namespace FileProvider.Services
{
    public interface IFileService
    {
        Task SetBlobContainerAsync(string containerName);
        string SetFileName(IFormFile file);
        Task<string> UploadFileAsync(IFormFile file, FileEntity fileEntity);

        Task SaveToDatabaseAsync(FileEntity fileEntity);
    }
}