using Client.ApiClients.Metadata.Models;

namespace Client.Models
{
    public sealed record FileItemWithContent(FileItemDto File, byte[]? Content)
    {
        public bool IsDownloaded = Content is not null;
    };
}
