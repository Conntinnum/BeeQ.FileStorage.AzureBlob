using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using BeeQ.FileStorage.Service;
using System.Security.Cryptography;
using System.Text;

namespace BeeQ.FileStorage.AzureBlob;

internal partial class AzureBlobService<TId> : FileStorageService<TId>, IAzureBlobService<TId>
{
    private BlobContainerClient GetContainer(string? containerName)
    {
        var client = GetClient();
        return client.GetBlobContainerClient(containerName ?? this.SchemaKey);
    }

    private BlobClient GetBlobClient(TId id, string? containerName)
    {
        var container = GetContainer(containerName);
        return container.GetBlobClient(id!.ToString());
    }

    private async Task OnDelete(IFileInfo<TId> info)
    {
        var container = GetClient().GetBlobContainerClient(this.SchemaKey);
        await container.CreateIfNotExistsAsync();
        var path = info.FullPath.Replace('\\', '/');

        var blob = container.GetBlobClient(path);

        await blob.DeleteIfExistsAsync();
    }

    private async Task<Stream?> OnGetStream(IFileInfo<TId> info)
    {
        var container = GetClient().GetBlobContainerClient(this.SchemaKey);
        await container.CreateIfNotExistsAsync();
        var path = info.FullPath.Replace('\\', '/');

        var blob = container.GetBlobClient(path);

        return await blob.OpenReadAsync();
    }

    private async Task OnUploadStream(IFileUploadInfo<TId, Stream> info)
    {
        var container = GetClient().GetBlobContainerClient(this.SchemaKey);
        await container.CreateIfNotExistsAsync();
        var path = info.FullPath.Replace('\\', '/');

        var blob = container.GetBlobClient(path);

        await blob.UploadAsync(info.Content, overwrite: true);
    }
    // ---------------------------------------------------------
    // LISTAR ARCHIVOS
    // ---------------------------------------------------------
    public async Task<BlobItem[]> GetFiles(string prefix, string? containerName = null)
    {
        var container = GetContainer(containerName);

        var result = new List<BlobItem>();


#pragma warning disable S3267 // El error es incorrecto, no se puede usar LinQ sobre un AsyncPageable de Azure
        await foreach (var blob in container.GetBlobsAsync())
        {
            if (blob.Name.StartsWith(prefix))
                result.Add(blob);
        }
#pragma warning restore S3267
        return [.. result];
    }

    // ---------------------------------------------------------
    // URL TEMPORAL (SAS)
    // ---------------------------------------------------------
    public async Task<string?> GetTemporalyUrl(TId id, TimeSpan duracion, string? containerName = null)
    {
        var blob = GetBlobClient(id, containerName);

        if (!await blob.ExistsAsync())
            return null;

        var sas = blob.GenerateSasUri(new BlobSasBuilder
        {
            BlobName = blob.Name,
            BlobContainerName = blob.BlobContainerName,
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.Add(duracion)
        });

        return sas.ToString();
    }

    // ---------------------------------------------------------
    // METADATA
    // ---------------------------------------------------------
    public async Task<BlobProperties?> GetFileMetadata(TId id, string? containerName = null)
    {
        var blob = GetBlobClient(id, containerName);

        if (!await blob.ExistsAsync())
            return null;

        var props = await blob.GetPropertiesAsync();
        return props.Value;
    }

    // ---------------------------------------------------------
    // COPIAR ARCHIVOS
    // ---------------------------------------------------------
    public async Task CopyFile(string sourceBlobName, string destinationBlobName)
        => await CopyFile(null, sourceBlobName, null, destinationBlobName);

    public async Task CopyFile(string? sourceContainer, string sourceBlobName, string? destinationContainer, string destinationBlobName)
    {
        var source = GetContainer(sourceContainer).GetBlobClient(sourceBlobName);
        var dest = GetContainer(destinationContainer).GetBlobClient(destinationBlobName);

        var uri = source.GenerateSasUri(new BlobSasBuilder(BlobContainerSasPermissions.Read, DateTimeOffset.UtcNow.AddMinutes(10))
        {
            BlobName = source.Name,
            BlobContainerName = source.BlobContainerName,
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(10)
        });

        await dest.StartCopyFromUriAsync(uri);

        // Esperar a que termine la copia
        while (true)
        {
            var props = await dest.GetPropertiesAsync();
            if (props.Value.CopyStatus != CopyStatus.Pending)
                break;

            await Task.Delay(200);
        }
    }

    // ---------------------------------------------------------
    // MOVER ARCHIVOS
    // ---------------------------------------------------------
    public async Task MoveFile(string sourceBlobName, string destinationBlobName)
        => await MoveFile(null, sourceBlobName, null, destinationBlobName);

    public async Task MoveFile(string? sourceContainer, string sourceBlobName, string? destinationContainer, string destinationBlobName)
    {
        await CopyFile(sourceContainer, sourceBlobName, destinationContainer, destinationBlobName);

        var source = GetContainer(sourceContainer).GetBlobClient(sourceBlobName);
        await source.DeleteIfExistsAsync();
    }

    // ---------------------------------------------------------
    // TAGS
    // ---------------------------------------------------------
    public async Task SetFileTags(TId id, IDictionary<string, string> tags)
    {
        var blob = GetBlobClient(id, null);

        await blob.SetTagsAsync(tags);
    }

    public async Task<IDictionary<string, string>> GetFileTags(TId id)
    {
        var blob = GetBlobClient(id, null);

        var response = await blob.GetTagsAsync();
        return response.Value.Tags;
    }
}

