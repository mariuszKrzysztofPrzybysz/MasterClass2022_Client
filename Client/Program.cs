#region MetadatasApiClient
using Client.ApiClients.Metadata;
using Client.ApiClients.Storage;

var metadatasClient = new HttpClient
{
    BaseAddress = new Uri("https://localhost:7235")
};
var metadataApiClient = new MetadataApiClient(metadatasClient)
{
    Token = string.Empty
};
#endregion

#region StorageApiClient
var storageClient = new HttpClient
{
    BaseAddress = new Uri("https://localhost:7128")
};
var storageApiClient = new StorageApiClient(storageClient);
#endregion
