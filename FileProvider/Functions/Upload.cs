using Data.Contexts;
using Data.Entities;
using FileProvider.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace FileProvider.Functions
{
    public class Upload(ILogger<Upload> logger, IFileService fileService)
    {
        private readonly ILogger<Upload> _logger = logger;
        private readonly IFileService _fileService = fileService;


        [Function("Upload")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
        {

            try
            {
                if (req.Form.Files["file"] is IFormFile file)
                {
                    var containerName = !string.IsNullOrEmpty(req.Query["containerName"]) ? req.Query["containerName"].ToString() : "files";

                    var FileEntity = new FileEntity 
                    { 
                        FileName = _fileService.SetFileName(file),
                        ContentType = file.ContentType,
                        ContainerName = containerName

                    };


                    await _fileService.SetBlobContainerAsync(FileEntity.ContainerName);
                    var filePath = await _fileService.UploadFileAsync(file, FileEntity);
                    FileEntity.FilePath = filePath;

                    await _fileService.SaveToDatabaseAsync(FileEntity);

                    return new OkObjectResult(FileEntity);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR : Upload.Run :: {ex.Message}");
            }

            return new BadRequestResult();

        }
    }
}
