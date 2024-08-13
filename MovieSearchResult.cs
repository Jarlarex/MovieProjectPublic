using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieProject1
{
    // Represents the result of a movie search from the OMDB API
    public class MovieSearchResult
    {
        // List of movies returned from the search
        [JsonProperty("Search")]
        public List<MovieDetail> Movies { get; set; }

        // Total number of results found
        [JsonProperty("totalResults")]
        public string TotalResults { get; set; }

        // Response status from the API
        [JsonProperty("Response")]
        public string Response { get; set; }
    }
}
