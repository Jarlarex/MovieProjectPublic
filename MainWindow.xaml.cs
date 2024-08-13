using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Data.OleDb;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MovieProject1
{
    public partial class MainWindow : Window
    {
        private OmdbApiService _omdbApiService = new OmdbApiService(); // Service for interacting with the OMDB API
        private static readonly HttpClient _httpClient = new HttpClient(); // Client for making HTTP requests
        private const string YouTubeApiKey = ""; // API key for YouTube data API
        private OleDbConnection connection; // Connection object for the database
        private DatabaseManager _dbManager; // Manager for database operations

        public MovieViewModel ViewModel { get; private set; } = new MovieViewModel(); // ViewModel for binding data to the UI

        public MainWindow()
        {
            InitializeComponent(); // Initialize the window components
            InitializeDatabaseConnection(); // Setup database connection
            _dbManager = new DatabaseManager();
            ViewModel = new MovieViewModel();
            DataContext = ViewModel; // Set this window's data context for data bindings
            _dbManager.OnLikedMoviesUpdated += LoadLikedMovies; // Subscribe to liked movies update event
            _dbManager.OnWatchlistMoviesUpdated += LoadWatchlistMovies; // Subscribe to watchlist movies update event

            LoadLibraryAsync(); // Load the initial movie library asynchronously
        }

        private async void LoadLibraryAsync()
        {
            var likedMovies = await _dbManager.FetchLikedMovies(); // Asynchronously fetch liked movies
            var watchlistMovies = await _dbManager.FetchWatchlistMovies(); // Asynchronously fetch movies in the watchlist

            ViewModel.LikedMovies = new ObservableCollection<MovieDetail>(likedMovies); // Update ViewModel with liked movies
            ViewModel.WatchlistMovies = new ObservableCollection<MovieDetail>(watchlistMovies); // Update ViewModel with watchlist movies
        }

        private void LoadLikedMovies()
        {
            Application.Current.Dispatcher.Invoke(async () =>
            {
                ViewModel.LikedMovies = new ObservableCollection<MovieDetail>(await _dbManager.FetchLikedMovies()); // Update the liked movies on UI thread
            });
        }

        private void LoadWatchlistMovies()
        {
            Application.Current.Dispatcher.Invoke(async () =>
            {
                ViewModel.WatchlistMovies = new ObservableCollection<MovieDetail>(await _dbManager.FetchWatchlistMovies()); // Update the watchlist movies on UI thread
            });
        }

        private void InitializeDatabaseConnection()
        {
            string connectionString = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=MovieDatabase.mdb";
            connection = new OleDbConnection(connectionString); // Setup the connection string for the database

            try
            {
                connection.Open(); // Try opening the database connection
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to connect to database: " + ex.Message); // Show error message if connection fails
            }
        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var searchResult = await _omdbApiService.GetMovieDetailsAsync(SearchBox.Text); // Search for movies using the OMDB API

                if (searchResult != null && searchResult.Movies != null)
                {
                    ViewModel.UpdateMovies(searchResult.Movies); // Update the ViewModel with the search results
                }
                else
                {
                    ViewModel.ErrorMessage = "Movies not found."; // Set error message if no movies found
                }
            }
            catch (Exception ex)
            {
                ViewModel.ErrorMessage = $"Error: {ex.Message}"; // Set error message on exception
            }
        }

        private void PrevPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.CurrentPage > 0)
            {
                ViewModel.CurrentPage--; // Decrement the current page
            }
        }

        private void NextPageButton_Click(object sender, RoutedEventArgs e)
        {
            int lastMovieIndex = (ViewModel.CurrentPage + 1) * ViewModel.ItemsPerPage;

            if (lastMovieIndex < ViewModel.AllMovies.Count)
            {
                ViewModel.CurrentPage++; // Increment the current page if not at the end
            }
        }

        private async void MoviesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listView = sender as ListView;
            var selectedMovie = listView.SelectedItem as MovieDetail; // Get the selected movie

            if (selectedMovie != null)
            {
                var movieDetailFull = await _omdbApiService.GetMovieDetailsFullAsync(selectedMovie.imdbID);
                string trailerUrl = await FetchYouTubeTrailerUrlAsync($"{selectedMovie.Title} trailer");

                if (movieDetailFull != null)
                {
                    var movieDetailsWindow = new MovieDetailsWindow(movieDetailFull, trailerUrl); // Open new window with full movie details
                    movieDetailsWindow.Show(); // Show the new window
                }
            }
        }

        private async Task<string> FetchYouTubeTrailerUrlAsync(string searchQuery)
        {
            string requestUri = $"https://www.googleapis.com/youtube/v3/search?part=snippet&q={Uri.EscapeDataString(searchQuery)}&maxResults=1&type=video&key={YouTubeApiKey}";

            try
            {
                var response = await _httpClient.GetStringAsync(requestUri); // Send request to YouTube API
                dynamic results = JsonConvert.DeserializeObject(response); // Deserialize the response

                if (results.items.Count > 0)
                {
                    string videoId = results.items[0].id.videoId; // Extract video ID
                    return $"https://www.youtube.com/watch?v={videoId}"; // Return YouTube URL
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"YouTube API Error: {ex.Message}"); // Log YouTube API errors
            }

            return null; // Return null if no video found
        }

        // Handles selection changes in the liked movies list view
        private async void LikedMoviesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(sender is ListView listView)) return;
            if (e.AddedItems.Count == 0) return;

            var selectedMovie = e.AddedItems[0] as MovieDetail; // Get the selected movie
            listView.SelectedItem = null; // Clear the selection immediately

            var movieDetailFull = await _omdbApiService.GetMovieDetailsFullAsync(selectedMovie.imdbID);
            string trailerUrl = await FetchYouTubeTrailerUrlAsync($"{selectedMovie.Title} trailer");

            if (movieDetailFull != null)
            {
                var movieDetailsWindow = new MovieDetailsWindow(movieDetailFull, trailerUrl);
                movieDetailsWindow.Show(); // Show the movie details window
            }
        }

        // Handles selection changes in the watchlist movies list view
        private async void WatchlistMoviesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(sender is ListView listView)) return;
            if (e.AddedItems.Count == 0) return;

            var selectedMovie = e.AddedItems[0] as MovieDetail; // Get the selected movie
            listView.SelectedItem = null; // Clear the selection immediately

            var movieDetailFull = await _omdbApiService.GetMovieDetailsFullAsync(selectedMovie.imdbID);
            string trailerUrl = await FetchYouTubeTrailerUrlAsync($"{selectedMovie.Title} trailer");

            if (movieDetailFull != null)
            {
                var movieDetailsWindow = new MovieDetailsWindow(movieDetailFull, trailerUrl);
                movieDetailsWindow.Show(); // Show the movie details window
            }
        }
    }
}
