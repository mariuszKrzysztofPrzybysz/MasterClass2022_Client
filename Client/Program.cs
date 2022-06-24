using Akka.Actor;
using Akka.Streams;
using Akka.Streams.Dsl;
using Client.ApiClients.Metadata;
using Client.ApiClients.Metadata.Models;
using Client.ApiClients.Storage;
using Client.Models;

#region MetadatasApiClient
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

var source = Source.From(metadataApiClient.BrowseAsync().ToBlockingEnumerable());
var sink = Sink.ForEach<FileItemWithContent>(file => Console.WriteLine(file.File.FileId));
#region Flows
var throttle = Flow.Create<FileItemDto>().Throttle(49, TimeSpan.FromSeconds(10), 0, ThrottleMode.Shaping);
var download = Flow.Create<FileItemDto>().SelectAsyncUnordered(5,
    async file =>
    {
        byte[] content = await storageApiClient.GetByteArrayAsync(file.FileId);

        return new FileItemWithContent(file, content);
    });
#endregion

using (var system = ActorSystem.Create("system"))
{
    using (var materializer = system.Materializer())
    {
        await source
            .Via(throttle)
            .Via(download)
            .RunWith(sink, materializer);
    }
}

Console.WriteLine("Press any key...");
Console.ReadLine();