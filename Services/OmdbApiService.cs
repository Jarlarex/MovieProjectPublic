using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MovieProject1.Services
{
    public sealed class OmdbApiService : IOmdbApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly ConcurrentDictionary<string, MovieDetailFull> _detailsCache =
            new ConcurrentDictionary<string, MovieDetailFull>(StringComparer.OrdinalIgnoreCase);

        public OmdbApiService(HttpClient httpClient, string apiKey)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _apiKey = apiKey ?? string.Empty;
        }

        public async Task<MovieSearchResult> SearchAsync(string query, int page, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException("Enter a movie title to search.", nameof(query));

            EnsureApiKey();
            page = Math.Max(1, page);

            var url = "https://www.omdbapi.com/?apikey=" + Uri.EscapeDataString(_apiKey)
                + "&s=" + Uri.EscapeDataString(query.Trim())
                + "&type=movie&page=" + page;

            using (var response = await _httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false))
            {
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var result = JsonConvert.DeserializeObject<MovieSearchResult>(json);

                if (result == null)
                    throw new InvalidOperationException("OMDb returned an empty response.");

                if (!string.Equals(result.Response, "True", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException(result.Error ?? "OMDb could not find matching movies.");

                result.Movies = result.Movies ?? new List<MovieDetail>();
                return result;
            }
        }

        public async Task<MovieDetailFull> GetDetailsAsync(string imdbId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(imdbId))
                throw new ArgumentException("A valid IMDb ID is required.", nameof(imdbId));

            if (_detailsCache.TryGetValue(imdbId, out var cached))
                return cached;

            EnsureApiKey();

            var url = "https://www.omdbapi.com/?apikey=" + Uri.EscapeDataString(_apiKey)
                + "&i=" + Uri.EscapeDataString(imdbId) + "&plot=full";

            using (var response = await _httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false))
            {
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var result = JsonConvert.DeserializeObject<MovieDetailFull>(json);

                if (result == null || !string.Equals(result.Response, "True", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException(result != null && !string.IsNullOrWhiteSpace(result.Error)
                        ? result.Error
                        : "OMDb could not load this movie.");

                result.Ratings = result.Ratings ?? new List<Rating>();
                _detailsCache[imdbId] = result;
                return result;
            }
        }

        private void EnsureApiKey()
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
                throw new InvalidOperationException("OMDb API key is not configured. Add OmdbApiKey to App.config.");
        }
    }
}
