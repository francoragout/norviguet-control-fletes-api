using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;

public class BlobStorageService : IBlobStorageService
{
    private readonly BlobContainerClient _containerClient;

    public BlobStorageService(IConfiguration configuration)
    {
        var connectionString = configuration["AzureStorage:ConnectionString"];
        var containerName = configuration["AzureStorage:ContainerName"];
        _containerClient = new BlobContainerClient(connectionString, containerName);
    }

    public async Task<string> UploadAsync(string fileName, Stream fileStream, string contentType)
    {
        if (fileStream == null || fileStream.Length == 0)
            throw new ArgumentException("No se recibió ningún archivo.", nameof(fileStream));

        var blobClient = _containerClient.GetBlobClient(fileName);
        await blobClient.UploadAsync(fileStream, overwrite: true);
        await blobClient.SetHttpHeadersAsync(new BlobHttpHeaders { ContentType = contentType });
        return blobClient.Uri.ToString();
    }

    public async Task DeleteAsync(string fileName)
    {
        var blobClient = _containerClient.GetBlobClient(fileName);
        await blobClient.DeleteIfExistsAsync();
    }

    public string GetBlobSasUrl(string fileName, int minutesValid = 60)
    {
        var blobClient = _containerClient.GetBlobClient(fileName);

        if (!blobClient.CanGenerateSasUri)
            throw new InvalidOperationException("No se puede generar SAS URI. Revisa la configuración de credenciales.");

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = _containerClient.Name,
            BlobName = fileName,
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(minutesValid)
        };
        sasBuilder.SetPermissions(BlobSasPermissions.Read);

        return blobClient.GenerateSasUri(sasBuilder).ToString();
    }
}