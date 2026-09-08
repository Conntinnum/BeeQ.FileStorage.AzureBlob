using Azure.Storage.Blobs;
using BeeQ.FileStorage.Service;

namespace BeeQ.FileStorage.AzureBlob;

public class AzureBlobConfiguration : AzureBlobConfiguration<Guid> { }

public class AzureBlobConfiguration<TId>
{
    public string? ConnectionString { get; set; }
    public BlobClientOptions? CustomOptions { get; set; }
    public Uri? HostUri { get; set; }
    public Azure.Core.TokenCredential? Credentials { get; set; }

    public string? BasePath { get; set; }

    public Func<IFileStorageFullIdentifier<TId>, string>? PathTemplate { get; set; }
    public Func<TId, string>? GetFilename { get; set; }
    public Func<(string? SchemaName, string Filename), Task<TId>>? OnCreateId { get; set; }

    internal virtual void Validate()
    {
        if (PathTemplate == null && string.IsNullOrEmpty(BasePath))
            throw new BasePathAzureBlobConfigratedException();

        if (typeof(TId) != typeof(Guid) && OnCreateId == null)
            throw new CreateIdAzureBlobConfigratedException();

        if (string.IsNullOrEmpty(ConnectionString) && HostUri == null)
            throw new ConnectionStringConfigurationException();
    }
}