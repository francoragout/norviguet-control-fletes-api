using System.IO;
using System.Threading.Tasks;

public interface IBlobStorageService
{
    Task<string> UploadAsync(string fileName, Stream fileStream, string contentType);
    Task DeleteAsync(string fileName);
    string GetBlobSasUrl(string fileName, int minutesValid = 60);
}