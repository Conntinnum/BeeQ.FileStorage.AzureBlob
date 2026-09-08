namespace BeeQ.FileStorage.AzureBlob;

public class BasePathAzureBlobConfigratedException : System.Exception
{
    public BasePathAzureBlobConfigratedException() : base("Basepath is not configurated") { }
}
public class CreateIdAzureBlobConfigratedException : System.Exception
{
    public CreateIdAzureBlobConfigratedException() : base("CreateId is not configurated") { }
}
public class ConnectionStringConfigurationException : System.Exception
{
    public ConnectionStringConfigurationException() : base("ConnectionString is not configurated") { }
}
