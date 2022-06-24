using Client.ApiClients.Metadata.Models;

namespace Client.Models
{
    public enum Status
    {
        Ok,
        Failed
    }

    public sealed record FileWithStatus(FileItemDto File, Status Status);
}
