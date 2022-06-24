using Akka.Actor;
using Akka.Streams;
using Akka.Streams.Dsl;
using Client.ApiClients.Metadata;
using Client.ApiClients.Metadata.Models;
using Client.ApiClients.Storage;

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
var sink = Sink.ForEach<FileItemDto>(file => Console.WriteLine(file.FileId));
#region Flows
var throttle = Flow.Create<FileItemDto>().Throttle(49, TimeSpan.FromSeconds(10), 0, ThrottleMode.Shaping);
#endregion

using (var system = ActorSystem.Create("system"))
{
    using (var materializer = system.Materializer())
    {
        await source
            .Via(throttle)
            .RunWith(sink, materializer);
    }
}

Console.WriteLine("Press any key...");
Console.ReadLine();