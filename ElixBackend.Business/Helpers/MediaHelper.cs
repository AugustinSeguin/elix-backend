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
            // If no new file provided, return existing stored value (which may be filename or null)
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

        // Delete old file if exists. existingMediaPath may be a filename or an absolute path.
        if (!string.IsNullOrEmpty(existingMediaPath))
        {
            try
            {
                var oldFileName = Path.GetFileName(existingMediaPath);
                var oldFilePath = Path.Combine(uploadsFolder, oldFileName);
                if (File.Exists(oldFilePath))
                {
                    File.Delete(oldFilePath);
                }
            }
            catch
            {
                // swallow any deletion errors; not critical
            }
        }

        // Return only the file name so controllers/views can build a public URL like /uploads/{fileName}
        return uniqueFileName;
    }
}