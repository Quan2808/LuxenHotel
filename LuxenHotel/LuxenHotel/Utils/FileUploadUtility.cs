namespace LuxenHotel.Utils;

public static class FileUploadUtility
{
    public static async Task<List<string>> UploadFilesAsync(List<IFormFile> files, IWebHostEnvironment environment)
    {
        var mediaPaths = new List<string>();
        if (files == null || !files.Any())
            return mediaPaths;

        var uploadsFolder = Path.Combine(environment.WebRootPath, "media");
        Directory.CreateDirectory(uploadsFolder);

        foreach (var file in files)
        {
            if (file.Length > 0)
            {
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                mediaPaths.Add($"/media/{fileName}");
            }
        }

        return mediaPaths;
    }

    public static async Task<string?> UploadSingleFileAsync(IFormFile file, IWebHostEnvironment environment)
    {
        if (file == null || file.Length == 0)
            return null;

        var uploadsFolder = Path.Combine(environment.WebRootPath, "media");
        Directory.CreateDirectory(uploadsFolder);

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return $"/media/{fileName}";
    }
}