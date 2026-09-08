using Azure.Storage.Blobs;
using BeeQ.FileStorage.Service;
using System.Security.Cryptography;
using System.Text;

namespace BeeQ.FileStorage.AzureBlob;

internal class AzureBlobService : AzureBlobService<Guid>, IAzureBlobService
{
    public AzureBlobService(AzureBlobConfiguration<Guid> options) : base(options)
    {
        base.Interceptors.OnCreateId = options.OnCreateId ?? OnCreateId;
        base.FullPathTemplate = options.PathTemplate ?? (info => AzureBlobService.UseCustomFullPath(info, options.BasePath!));
    }

    private static async Task<Guid> OnCreateId((string? SchemaName, string Filename) cfg)
    {
        var key = $"{cfg.SchemaName}.{cfg.Filename}";
        var bytes = Encoding.UTF8.GetBytes(key);
#pragma warning disable S4790
        var hash = MD5.HashData(bytes);
#pragma warning restore S4790

        return new Guid(hash);
    }

    private static string UseCustomFullPath(IFileStorageFullIdentifier<Guid> info, string basePath)
    {
        var id = $"{info.Id}";
        return Path.Combine(basePath, id[..2], id[2..4], info.Id.ToString());
    }
}

internal partial class AzureBlobService<TId> : FileStorageService<TId>, IAzureBlobService<TId>
{
    public AzureBlobConfiguration<TId> Options { get; set; }
    private BlobServiceClient? _Client = null;

    public AzureBlobService(AzureBlobConfiguration<TId> options)
    {
        options.Validate();
        this.Options = options;
        base.Interceptors.OnCreateId = options.OnCreateId;
        base.Interceptors.OnGetFilename = options.GetFilename ?? GetFilename;
        base.FullPathTemplate = options.PathTemplate ?? (info => AzureBlobService<TId>.UseCustomFullPath(info, options.BasePath!));

        base.Interceptors.OnUploadStream = OnUploadStream;
        base.Interceptors.OnGetStream = OnGetStream;
        base.Interceptors.OnDelete = OnDelete;
    }

    private static string GetFilename(TId id)
    {
        return $"{id}";
    }

    private static string UseCustomFullPath(IFileStorageFullIdentifier<TId> info, string basePath)
    {
        return Path.Combine(basePath, info.Filename);
    }

    private BlobServiceClient GetClient()
    {
        if (_Client is not null)
            return _Client;

        if (!string.IsNullOrEmpty(this.Options.ConnectionString))
        {
            if (this.Options.CustomOptions != null)
                return _Client = new BlobServiceClient(this.Options.ConnectionString, this.Options.CustomOptions);
            return _Client = new BlobServiceClient(this.Options.ConnectionString);
        }

        if (this.Options.HostUri != null)
        {
            if (this.Options.Credentials != null)
                return _Client = new BlobServiceClient(this.Options.HostUri, this.Options.Credentials, this.Options.CustomOptions);
            return _Client = new BlobServiceClient(this.Options.HostUri, this.Options.CustomOptions);
        }

        throw new ConnectionStringConfigurationException();
    }
}

