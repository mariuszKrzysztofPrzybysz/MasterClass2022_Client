using Client.ApiClients.Metadata.Models;
using System.Runtime.CompilerServices;

namespace Client.ApiClients.Metadata
{
    internal class MetadataApiClient
    {
        public string Token { get; set; }
        private readonly HttpClient _client;

        public MetadataApiClient(HttpClient client)
        {
            _client = client;
        }

        public async IAsyncEnumerable<FileItemDto> BrowseAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            HttpResponseMessage? response = null;
            PagedResult<FileItemDto>? result;
            do
            {
                try
                {
                    Console.WriteLine($"Fetching data... Token {Token}");
                    response = await _client.GetAsync($"api/metadatas/v1?token={Token}", cancellationToken);
                    response.EnsureSuccessStatusCode();
                }
                catch (HttpRequestException ex)
                    when (ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests
                        || ex.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                {
                    if (TryReadDelayFromHeader(response, out TimeSpan delay))
                    {
                        Console.WriteLine($"{DateTime.Now} Retry after {delay}");
                        await Task.Delay(delay, cancellationToken);
                        Console.WriteLine($"{DateTime.Now} Starting...");
                    }

                    continue;
                }

                var jsonResult = await response.Content.ReadAsStringAsync(cancellationToken);
                if (jsonResult != null)
                {
                    result = Newtonsoft.Json.JsonConvert.DeserializeObject<PagedResult<FileItemDto>>(jsonResult)!;
                    foreach (var item in result.Results)
                    {
                        yield return item;
                    }
                    Token = result.NextToken!;
                }
            }
            while (Token != null);
        }

        private static bool TryReadDelayFromHeader(HttpResponseMessage? response, out TimeSpan delay)
        {
            delay = TimeSpan.Zero;
            if (response is null)
            {
                return false;
            }

            response.Headers.TryGetValues("Retry-After", out var values);
            var delayAsString = values?.FirstOrDefault();
            if (delayAsString != null)
            {
                if (TimeSpan.TryParse(delayAsString, out delay))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
