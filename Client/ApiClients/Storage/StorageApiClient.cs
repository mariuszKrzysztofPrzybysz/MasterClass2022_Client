namespace Client.ApiClients.Storage
{
    public sealed class StorageApiClient
    {
        private readonly HttpClient _client;

        public StorageApiClient(HttpClient client)
        {
            _client = client;
        }

        public async Task<byte[]> GetByteArrayAsync(string fileId, CancellationToken cancellationToken = default)
            => await _client.GetByteArrayAsync($"/api/files/v1/{fileId}/content", cancellationToken);
    }
}
