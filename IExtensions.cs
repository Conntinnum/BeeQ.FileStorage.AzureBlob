using BeeQ.FileStorage.AzureBlob;
using BeeQ.FileStorage.Builder;

namespace BeeQ.FileStorage;

public static class IExtensions
{
    /// <summary>
    /// Extension to use the Local Disk to storage the Files
    /// </summary>
    /// <param name="builder">Empty File Storage Builder</param>
    /// <param name="schemaName">Optional schema name used to distinguish storage namespaces</param>
    /// <param name="options">Configuration options for the Local Disk storage</param>
    /// <returns>The configured file storage</returns>
    public static IAzureBlobService UseAzureBlob(this IFileStorageBuilder builder, string schemaName, Action<AzureBlobConfiguration> options)
    {
        var opt = new AzureBlobConfiguration();
        options.Invoke(opt);
        return builder.Use<Guid>(schemaName)
            .CustomBuild<AzureBlobService, IAzureBlobService>(() => new AzureBlobService(opt));
    }

    /// <summary>
    /// Extension to use the Local Disk to storage the Files
    /// </summary>
    /// <typeparam name="TId">The type of the Id used to identify the files</typeparam>
    /// <param name="builder">Empty File Storage Builder</param>
    /// <param name="schemaName">Optional schema name used to distinguish storage namespaces</param>
    /// <param name="options">Configuration options for the Local Disk storage</param>
    /// <returns>The configured file storage</returns>
    public static IAzureBlobService<TId> UseAzureBlob<TId>(this IFileStorageBuilder builder, string schemaName, Action<AzureBlobConfiguration<TId>> options)
    {
        var opt = new AzureBlobConfiguration<TId>();
        options.Invoke(opt);
        return builder.Use<TId>(schemaName)
            .CustomBuild<AzureBlobService<TId>, IAzureBlobService<TId>>(() => new AzureBlobService<TId>(opt));
    }

}
