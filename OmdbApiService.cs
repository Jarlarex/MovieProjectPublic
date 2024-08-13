using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

// Namespace for the movie project using OMDB API
namespace MovieProject1
{
    // Class to interact with the OMDB API
    public class OmdbApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey = "";  // API key for OMDB API
        private readonly string _baseUrl = "http://www.omdbapi.com/";  // Base URL for the OMDB API

        // Constructor to initialize the HttpClient
        public OmdbApiService()
        {
            _httpClient = new HttpClient();
        }

        // Asynchronous method to get movie details by title
        public async Task<MovieSearchResult> GetMovieDetailsAsync(string title)
        {
            try
            {
                // Construct URL with title query
                var url = $"{_baseUrl}?apikey={_apiKey}&s={Uri.EscapeDataString(title)}";
                var response = await _httpClient.GetStringAsync(url);  // Send GET request
                var movieSearchResult = JsonConvert.DeserializeObject<MovieSearchResult>(response);  // Deserialize JSON response to object
                return movieSearchResult;  // Return deserialized object
            }
            catch (HttpRequestException httpEx)
            {
                throw new Exception("Network error occurred.", httpEx);  // Handle network errors
            }
            catch (JsonException jsonEx)
            {
                throw new Exception("Error parsing API response.", jsonEx);  // Handle JSON parsing errors
            }
        }

        // Asynchronous method to get detailed movie info by IMDb ID
        public async Task<MovieDetailFull> GetMovieDetailsFullAsync(string imdbID)
        {
            try
            {
                // Construct URL with IMDb ID and request full plot
                var url = $"{_baseUrl}?apikey={_apiKey}&i={imdbID}&plot=full";
                var response = await _httpClient.GetStringAsync(url);  // Send GET request
                var movieDetailFull = JsonConvert.DeserializeObject<MovieDetailFull>(response);  // Deserialize JSON response to object
                return movieDetailFull;  // Return deserialized object
            }
            catch (HttpRequestException httpEx)
            {
                throw new Exception("Network error occurred.", httpEx);  // Handle network errors
            }
            catch (JsonException jsonEx)
            {
                throw new Exception("Error parsing API response.", jsonEx);  // Handle JSON parsing errors
            }
        }
    }
}
