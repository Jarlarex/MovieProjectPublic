using System.Collections.Generic;
using Newtonsoft.Json;

namespace MovieProject1
{
    public class MovieDetailFull
    {
        public string Poster { get; set; }
        public string Title { get; set; }
        public string Year { get; set; }
        public string imdbID { get; set; }
        public string Rated { get; set; }
        public string Runtime { get; set; }
        public string Genre { get; set; }
        public string Director { get; set; }
        public string Writer { get; set; }
        public string Actors { get; set; }
        public string Plot { get; set; }
        public string BoxOffice { get; set; }

        [JsonProperty("Response")]
        public string Response { get; set; }

        [JsonProperty("Error")]
        public string Error { get; set; }

        public List<Rating> Ratings { get; set; } = new List<Rating>();
    }

    public class Rating
    {
        public string Source { get; set; }
        public string Value { get; set; }
    }
}
