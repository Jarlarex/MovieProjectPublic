using System.Configuration;

namespace MovieProject1.Infrastructure
{
    public static class AppConfiguration
    {
        public static string OmdbApiKey
        {
            get { return ConfigurationManager.AppSettings["OmdbApiKey"] ?? string.Empty; }
        }

        public static string YouTubeApiKey
        {
            get { return ConfigurationManager.AppSettings["YouTubeApiKey"] ?? string.Empty; }
        }
    }
}
