namespace Client.ApiClients.Metadata.Models
{
    public class PagedResult<T>
    {
        public IEnumerable<T> Results { get; set; } = default!;
        public string? NextToken { get; set; }
    }
}
