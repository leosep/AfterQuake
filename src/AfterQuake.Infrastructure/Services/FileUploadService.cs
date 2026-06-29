using AfterQuake.Application.Interfaces;
using Microsoft.AspNetCore.Hosting;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace AfterQuake.Infrastructure.Services;

public class FileUploadService : IFileUploadService
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp" };
    private const long MaxFileSize = 5 * 1024 * 1024;
    private const int MaxWidth = 800;
    private const int ThumbnailSize = 150;

    private readonly string _uploadsPath;

    public FileUploadService(IWebHostEnvironment env)
    {
        _uploadsPath = Path.GetFullPath(Path.Combine(env.WebRootPath, "uploads", "photos"));
        Directory.CreateDirectory(_uploadsPath);
    }

    public async Task<string> UploadAsync(Stream fileStream, string fileName)
    {
        var ext = Path.GetExtension(fileName);
        if (!AllowedExtensions.Contains(ext))
            throw new InvalidOperationException($"File type '{ext}' is not allowed.");

        if (fileStream.Length > MaxFileSize)
            throw new InvalidOperationException("File size exceeds the maximum allowed size of 5MB.");

        fileStream.Position = 0;

        using var image = await Image.LoadAsync(fileStream);

        if (image.Width > MaxWidth)
        {
            var ratio = (double)MaxWidth / image.Width;
            image.Mutate(x => x.Resize(MaxWidth, (int)(image.Height * ratio)));
        }

        var uniqueName = $"{Guid.NewGuid()}.jpg";
        var filePath = Path.Combine(_uploadsPath, uniqueName);

        await image.SaveAsJpegAsync(filePath, new JpegEncoder { Quality = 75 });

        var thumbnailName = $"thumb_{uniqueName}";
        var thumbnailPath = Path.Combine(_uploadsPath, thumbnailName);
        using (var thumbnail = image.Clone(ctx => ctx.Resize(ThumbnailSize, ThumbnailSize)))
        {
            await thumbnail.SaveAsJpegAsync(thumbnailPath, new JpegEncoder { Quality = 75 });
        }

        return $"/uploads/photos/{uniqueName}";
    }

    public Task<bool> DeleteAsync(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            return Task.FromResult(false);

        var fileName = Path.GetFileName(relativePath);
        var fullPath = Path.GetFullPath(Path.Combine(_uploadsPath, fileName));

        if (!fullPath.StartsWith(_uploadsPath, StringComparison.OrdinalIgnoreCase))
            return Task.FromResult(false);

        var thumbnailPath = Path.Combine(_uploadsPath, $"thumb_{fileName}");

        if (File.Exists(fullPath)) File.Delete(fullPath);
        if (File.Exists(thumbnailPath)) File.Delete(thumbnailPath);

        return Task.FromResult(true);
    }
}
