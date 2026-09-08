using Azure.Storage.Blobs.Models;
using BeeQ.FileStorage.Service;

namespace BeeQ.FileStorage.AzureBlob;

/// <summary>
/// Azure Blob storage service interface using <see cref="Guid"/> as the identifier type.
/// </summary>
public interface IAzureBlobService : IAzureBlobService<Guid> { }

/// <summary>
/// Azure Blob storage service interface for operations on files using an identifier of type <typeparamref name="TId"/>.
/// </summary>
/// <typeparam name="TId">The type used to identify files.</typeparam>
public interface IAzureBlobService<TId> : IFileStorage<TId>
{
    /// <summary>
    /// Gets files whose names start with the specified prefix.
    /// </summary>
    /// <param name="prefix">Filter for file names; only files starting with this prefix will be returned.</param>
    /// <param name="containerName">Optional container name. If null, the default container is used.</param>
    /// <returns>An array of <see cref="BlobItem"/> matching the prefix.</returns>
    Task<BlobItem[]> GetFiles(string prefix, string? containerName = null);

    /// <summary>
    /// Gets a temporary URL for accessing the file identified by <paramref name="id"/>.
    /// </summary>
    /// <param name="id">Identifier of the file.</param>
    /// <param name="duracion">Duration the temporary URL will remain valid.</param>
    /// <param name="containerName">Optional container name. If null, the default container is used.</param>
    /// <returns>A temporary URL string if available; otherwise null.</returns>
    Task<string?> GetTemporalyUrl(TId id, TimeSpan duracion, string? containerName = null);

    /// <summary>
    /// Retrieves the properties/metadata of the specified file.
    /// </summary>
    /// <param name="id">Identifier of the file.</param>
    /// <param name="containerName">Optional container name. If null, the default container is used.</param>
    /// <returns>The <see cref="BlobProperties"/> for the file, or null if not found.</returns>
    Task<BlobProperties?> GetFileMetadata(TId id, string? containerName = null);

    /// <summary>
    /// Copies a blob within the default container from <paramref name="sourceBlobName"/> to <paramref name="destinationBlobName"/>.
    /// </summary>
    /// <param name="sourceBlobName">Name of the source blob.</param>
    /// <param name="destinationBlobName">Name for the destination blob.</param>
    Task CopyFile(string sourceBlobName, string destinationBlobName);

    /// <summary>
    /// Copies a blob from a source container to a destination container.
    /// </summary>
    /// <param name="sourceContainer">Optional source container name. If null, the default container is used.</param>
    /// <param name="sourceBlobName">Name of the source blob.</param>
    /// <param name="destinationContainer">Optional destination container name. If null, the default container is used.</param>
    /// <param name="destinationBlobName">Name for the destination blob.</param>
    Task CopyFile(string? sourceContainer, string sourceBlobName, string? destinationContainer, string destinationBlobName);

    /// <summary>
    /// Moves (renames) a blob within the default container from <paramref name="sourceBlobName"/> to <paramref name="destinationBlobName"/>.
    /// </summary>
    /// <param name="sourceBlobName">Name of the source blob.</param>
    /// <param name="destinationBlobName">Name for the destination blob.</param>
    Task MoveFile(string sourceBlobName, string destinationBlobName);

    /// <summary>
    /// Moves (renames) a blob from a source container to a destination container.
    /// </summary>
    /// <param name="sourceContainer">Optional source container name. If null, the default container is used.</param>
    /// <param name="sourceBlobName">Name of the source blob.</param>
    /// <param name="destinationContainer">Optional destination container name. If null, the default container is used.</param>
    /// <param name="destinationBlobName">Name for the destination blob.</param>
    Task MoveFile(string? sourceContainer, string sourceBlobName, string? destinationContainer, string destinationBlobName);

    /// <summary>
    /// Sets the specified tags on the file identified by <paramref name="id"/>.
    /// </summary>
    /// <param name="id">Identifier of the file.</param>
    /// <param name="tags">Dictionary containing tag key/value pairs to set on the file.</param>
    Task SetFileTags(TId id, IDictionary<string, string> tags);

    /// <summary>
    /// Gets the tags associated with the file identified by <paramref name="id"/>.
    /// </summary>
    /// <param name="id">Identifier of the file.</param>
    /// <returns>Dictionary of tag key/value pairs associated with the file.</returns>
    Task<IDictionary<string, string>> GetFileTags(TId id);
}
