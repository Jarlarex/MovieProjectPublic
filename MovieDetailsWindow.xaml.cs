using System;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Diagnostics;
using System.Data.SqlClient;
using System.Web;
using System.Data.OleDb;

namespace MovieProject1
{
    // Window class to display details of a movie
    public partial class MovieDetailsWindow : Window
    {
        private readonly DatabaseManager _dbManager; // Manages database operations
        private readonly MovieDetailFull _movieDetails; // Holds full details of the movie

        // Constructor initializing components and setting movie details
        public MovieDetailsWindow(MovieDetailFull movieDetails, string trailerUrl = null)
        {
            InitializeComponent(); // Initialize window components
            _movieDetails = movieDetails; // Store movie details
            _dbManager = new DatabaseManager(); // Initialize the database manager
            UpdateMovieDetails(movieDetails, trailerUrl); // Display the movie details
        }

        // Updates the UI with movie details and trailer
        private void UpdateMovieDetails(MovieDetailFull movieDetails, string trailerUrl)
        {
            try
            {
                // Set the poster image if available
                if (!string.IsNullOrWhiteSpace(movieDetails.Poster))
                {
                    Uri posterUri;
                    if (Uri.TryCreate(movieDetails.Poster, UriKind.Absolute, out posterUri))
                    {
                        PosterImage.Source = new BitmapImage(posterUri);
                    }
                    else
                    {
                        PosterImage.Source = null; // Clear poster if URL is invalid
                    }
                }
                else
                {
                    PosterImage.Source = null; // Clear poster if no URL provided
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading poster image: {ex.Message}");
                PosterImage.Source = null; // Clear poster on error
            }

            // Update text blocks with movie details
            TitleTextBlock.Text = movieDetails.Title;
            YearTextBlock.Text = $"Year: {movieDetails.Year}";
            RatedTextBlock.Text = $"Rated: {movieDetails.Rated}";
            RuntimeTextBlock.Text = $"Runtime: {movieDetails.Runtime}";
            GenreTextBlock.Text = $"Genre: {movieDetails.Genre}";
            DirectorTextBlock.Text = $"Director: {movieDetails.Director}";
            WriterTextBlock.Text = $"Writer: {movieDetails.Writer}";
            ActorsTextBlock.Text = $"Actors: {movieDetails.Actors}";
            PlotTextBlock.Text = $"Plot: {movieDetails.Plot}";
            BoxOfficeTextBlock.Text = $"Box Office: {movieDetails.BoxOffice}";

            // Display Rotten Tomatoes rating if available
            var rottenTomatoesRating = movieDetails.Ratings.FirstOrDefault(r => r.Source.Contains("Rotten Tomatoes"));
            if (rottenTomatoesRating != null)
            {
                RatingsItemsControl.ItemsSource = new[] { rottenTomatoesRating };
            }

            // Load the trailer if a URL is provided
            if (!string.IsNullOrEmpty(trailerUrl))
            {
                LoadTrailer(trailerUrl);
            }
        }

        // Loads the movie trailer in a web browser control
        private void LoadTrailer(string trailerUrl)
        {
            var videoId = ExtractVideoIdFromUrl(trailerUrl); // Extract YouTube video ID from URL
            // HTML to embed YouTube video
            string embedHtml = $@"
                <html>
                    <head>
                        <meta http-equiv='X-UA-Compatible' content='IE=edge'/>
                        <style>
                            body, html {{ height: 100%; margin: 0; padding: 0; overflow: hidden; }}
                            .video {{ position: relative; padding-bottom: 56.25%; height: 0; }}
                            .video iframe {{ position: absolute; top: 0; left: 0; width: 100%; height: 100%; }}
                        </style>
                    </head>
                    <body>
                        <div class='video'>
                            <iframe src='https://www.youtube.com/embed/{videoId}?autoplay=0&modestbranding=1&rel=0' frameborder='0' allow='autoplay; encrypted-media' allowfullscreen></iframe>
                        </div>
                    </body>
                </html>";
            TrailerWebBrowser.NavigateToString(embedHtml); // Navigate to the HTML content
        }

        // Extracts the YouTube video ID from the URL
        private string ExtractVideoIdFromUrl(string url)
        {
            var uri = new Uri(url); // Parse URL to URI
            var query = HttpUtility.ParseQueryString(uri.Query); // Parse query string
            return query["v"] ?? url.Split('/').Last(); // Return video ID or last URL segment
        }

        // Event handler for liking a movie
        private async void LikeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Check if the movie is already liked and add/remove accordingly
                if (await _dbManager.MovieExistsInLiked(_movieDetails.imdbID))
                {
                    await _dbManager.RemoveMovieFromLiked(_movieDetails.imdbID);
                    MessageBox.Show("Movie removed from liked movies.");
                }
                else
                {
                    bool addedSuccessfully = await _dbManager.AddMovieToLikedList(_movieDetails);
                    if (addedSuccessfully)
                    {
                        MessageBox.Show("Movie added to liked list!");
                    }
                    else
                    {
                        MessageBox.Show("Failed to add movie to liked list.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }

        // Event handler for adding a movie to the watchlist
        private async void AddToWatchlistButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Check if the movie is already in the watchlist and add/remove accordingly
                if (await _dbManager.MovieExistsInWatchlist(_movieDetails.imdbID))
                {
                    await _dbManager.RemoveMovieFromWatchlist(_movieDetails.imdbID);
                    MessageBox.Show("Movie removed from watchlist.");
                }
                else
                {
                    bool addedSuccessfully = await _dbManager.AddMovieToWatchlist(_movieDetails);
                    if (addedSuccessfully)
                    {
                        MessageBox.Show("Movie added to watchlist!");
                    }
                    else
                    {
                        MessageBox.Show("Failed to add movie to watchlist.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }
    }
}
