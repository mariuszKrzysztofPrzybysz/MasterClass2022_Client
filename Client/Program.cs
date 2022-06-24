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
        byte[]? content = null;
        try
        {
            content = await storageApiClient.GetByteArrayAsync(file.FileId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"File {file.FileId} not found. Message {ex.Message}");
        }

        return new FileItemWithContent(file, content);
    });

var partition = Flow.FromGraph(GraphDsl.Create(builder =>
{
    var input = builder.Add(new Partition<FileItemWithContent>(2, fileWithContent => fileWithContent.IsDownloaded ? 1 : 0));
    var error = Sink.ForEach<FileItemWithContent>(fileWithoutContent => Console.WriteLine($"Could not download file {fileWithoutContent.File.FileId}"));
    var output = builder.Add(new Merge<FileItemWithContent>(1));

    builder.From(input.Out(0)).To(error);
    builder.From(input.Out(1)).To(output.In(0));

    return new FlowShape<FileItemWithContent, FileItemWithContent>(input.In, output.Out);
}));
#endregion

using (var system = ActorSystem.Create("system"))
{
    using (var materializer = system.Materializer())
    {
        await source
            .Via(throttle)
            .Via(download)
            .Via(partition)
            .RunWith(sink, materializer);
    }
}

Console.WriteLine("Press any key...");
Console.ReadLine();