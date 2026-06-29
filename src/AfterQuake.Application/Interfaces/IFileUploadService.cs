namespace AfterQuake.Application.Interfaces;

public interface IFileUploadService
{
    Task<string> UploadAsync(Stream fileStream, string fileName);
    Task<bool> DeleteAsync(string relativePath);
}
