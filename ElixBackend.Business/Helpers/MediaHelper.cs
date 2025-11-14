using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace ElixBackend.Business.Helpers;

public static class MediaHelper
{
    public static async Task<string?> HandleMediaUploadAsync(
        IFormFile? mediaFile, 
        IConfiguration configuration,
        string? existingMediaPath = null)
    {
        if (mediaFile == null || mediaFile.Length == 0)
        {
            return existingMediaPath;
        }

        var uploadsPath = configuration["FileStorage:UploadsPath"] ?? "wwwroot/uploads";
        
        var uploadsFolder = Path.IsPathRooted(uploadsPath) 
            ? uploadsPath 
            : Path.Combine(Directory.GetCurrentDirectory(), uploadsPath);
            
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(mediaFile.FileName);
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await mediaFile.CopyToAsync(stream);
        }

        if (!string.IsNullOrEmpty(existingMediaPath))
        {
            var oldFileName = Path.GetFileName(existingMediaPath);
            var oldFilePath = Path.IsPathRooted(uploadsPath)
                ? Path.Combine(uploadsPath, oldFileName)
                : Path.Combine(Directory.GetCurrentDirectory(), uploadsPath, oldFileName);
                
            if (File.Exists(oldFilePath))
            {
                File.Delete(oldFilePath);
            }
        }

        return filePath;
    }
}