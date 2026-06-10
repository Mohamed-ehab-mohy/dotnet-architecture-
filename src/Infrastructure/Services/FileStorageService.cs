using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class FileStorageService
{
    private readonly ILogger<FileStorageService> _logger;

    public FileStorageService(ILogger<FileStorageService> logger)
    {
        _logger = logger;
    }

    public async Task<string> UploadFileAsync(string fileName, Stream content)
    {
        _logger.LogInformation("Uploading file: {FileName}", fileName);

        await Task.CompletedTask;

        return $"https://storage.example.com/{fileName}";
    }

    public async Task DeleteFileAsync(string fileUrl)
    {
        _logger.LogInformation("Deleting file: {FileUrl}", fileUrl);

        await Task.CompletedTask;
    }
}
