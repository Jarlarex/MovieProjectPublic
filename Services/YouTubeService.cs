using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MovieProject1.Services
{
    public sealed class YouTubeService : IYouTubeService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public YouTubeService(HttpClient httpClient, string apiKey)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _apiKey = apiKey ?? string.Empty;
        }

        public async Task<string> FindTrailerUrlAsync(string title, string year, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(_apiKey) || string.IsNullOrWhiteSpace(title))
                return null;

            var query = title.Trim() + " " + (year ?? string.Empty) + " official trailer";
            var url = "https://www.googleapis.com/youtube/v3/search?part=snippet&type=video&maxResults=5&q="
                + Uri.EscapeDataString(query)
                + "&key=" + Uri.EscapeDataString(_apiKey);

            using (var response = await _httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false))
            {
                if (!response.IsSuccessStatusCode)
                    return null;

                var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var result = JsonConvert.DeserializeObject<YouTubeSearchResponse>(json);

                var item = result?.Items?.FirstOrDefault(x =>
                    x.Id != null && !string.IsNullOrWhiteSpace(x.Id.VideoId));

                return item?.Id?.VideoId == null
                    ? null
                    : "https://www.youtube.com/watch?v=" + item.Id.VideoId;
            }
        }
    }

    public sealed class YouTubeSearchResponse
    {
        [JsonProperty("items")]
        public System.Collections.Generic.List<YouTubeSearchItem> Items { get; set; }
    }

    public sealed class YouTubeSearchItem
    {
        [JsonProperty("id")]
        public YouTubeVideoId Id { get; set; }
    }

    public sealed class YouTubeVideoId
    {
        [JsonProperty("videoId")]
        public string VideoId { get; set; }
    }
}
