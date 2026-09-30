using System.Collections.Generic;
using Newtonsoft.Json;

namespace MovieProject1
{
    public class MovieSearchResult
    {
        [JsonProperty("Search")]
        public List<MovieDetail> Movies { get; set; } = new List<MovieDetail>();

        [JsonProperty("totalResults")]
        public string TotalResults { get; set; }

        [JsonProperty("Response")]
        public string Response { get; set; }

        [JsonProperty("Error")]
        public string Error { get; set; }
    }
}
