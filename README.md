# BeeQ.FileStorage.AzureBlob

Azure Blob Storage adapter for the BeeQ.FileStorage abstraction. This package provides an implementation of IFileStorage backed by Azure Blob Storage and exposes a convenient API via IAzureBlobService and IAzureBlobService<TId>.

Key features
- Simple integration with BeeQ.FileStorage builder extensions
- Support for file listing, metadata, temporary access URLs, copy/move operations and tag management
- Generic identifier support (IAzureBlobService<TId>) and a Guid-based convenience interface (IAzureBlobService)
- Targets .NET 8, .NET 9 and .NET 10

Installation

Install the package from NuGet:

dotnet add package BeeQ.FileStorage.AzureBlob

Basic usage

Register the Azure Blob provider using the IFileStorageBuilder extension.

```csharp
// Using default Guid-based provider
var storage = builder.UseAzureBlob("myschema", options =>
{
	options.ConnectionString = "<AZURE_BLOB_CONNECTION_STRING>";
	options.ContainerName = "my-container";
});

// Using a typed id
var typedStorage = builder.UseAzureBlob<Guid>("myschema", options =>
{
	options.ConnectionString = "<AZURE_BLOB_CONNECTION_STRING>";
	options.ContainerName = "my-container";
});
```

Examples

Listing files with a prefix

```csharp
var files = await storage.GetFiles("invoices/");
foreach (var file in files) Console.WriteLine(file.Name);
```

Get a temporary access URL

```csharp
var url = await storage.GetTemporalyUrl(fileId, TimeSpan.FromMinutes(15));
if (url != null) Console.WriteLine(url);
```

Get file metadata

```csharp
var props = await storage.GetFileMetadata(fileId);
Console.WriteLine(props?.ContentType);
```

Copy or move files

```csharp
await storage.CopyFile("source.txt", "copy.txt");
await storage.MoveFile("temp.txt", "archive/temp.txt");
```

Set and get tags

```csharp
await storage.SetFileTags(fileId, new Dictionary<string,string>{{"owner","team-a"}});
var tags = await storage.GetFileTags(fileId);
```

Configuration

Configure provider options using the AzureBlobConfiguration (or AzureBlobConfiguration<TId>) passed to the UseAzureBlob extension. Typical options include connection string and container name. See the source configuration type for full details.

API

- IAzureBlobService — convenience interface using Guid identifiers
- IAzureBlobService<TId> — primary interface exposing file operations for a generic identifier type

Support & Contributing

Report issues or request features on the GitHub repository. Contributions are welcome via pull requests; please follow the repository coding and contribution guidelines.

License

This project is licensed under the terms in the LICENSE file in the repository.

