using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.OleDb;
using System.Threading.Tasks;

namespace MovieProject1
{
    public class DatabaseManager
    {
        private readonly OleDbConnection connection; // Database connection object for operations
        private List<MovieDetail> likedMovies = new List<MovieDetail>(); // List to hold liked movies
        private List<MovieDetail> watchlistMovies = new List<MovieDetail>(); // List to hold movies in the watchlist
        public event Action OnLikedMoviesUpdated; // Event triggered when liked movies are updated
        public event Action OnWatchlistMoviesUpdated; // Event triggered when watchlist movies are updated

        // Constructor initializes the database connection
        public DatabaseManager()
        {
            string connectionString = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=MovieDatabase.mdb";
            connection = new OleDbConnection(connectionString);
        }

        // Adds a movie to the liked list in the database
        public async Task<bool> AddMovieToLikedList(MovieDetailFull movieDetails)
        {
            if (await MovieExistsInLiked(movieDetails.imdbID))
            {
                return false; // Return false if the movie already exists in liked list
            }

            string query = "INSERT INTO LikedMovies ([Title], [Year], [Poster], [imdbID]) VALUES (?, ?, ?, ?)";
            try
            {
                await connection.OpenAsync();
                using (var command = new OleDbCommand(query, connection))
                {
                    command.Parameters.AddWithValue("?", movieDetails.Title);
                    command.Parameters.AddWithValue("?", movieDetails.Year);
                    command.Parameters.AddWithValue("?", movieDetails.Poster);
                    command.Parameters.AddWithValue("?", movieDetails.imdbID);

                    await command.ExecuteNonQueryAsync();
                }
            }
            finally
            {
                connection.Close();
            }

            OnLikedMoviesUpdated?.Invoke(); // Trigger the update event
            return true; // Return true when the movie is added successfully
        }

        // Checks if a movie is already liked
        public async Task<bool> MovieExistsInLiked(string imdbID)
        {
            string query = "SELECT COUNT(*) FROM LikedMovies WHERE imdbID = ?";
            int count = 0;

            await connection.OpenAsync();
            using (var command = new OleDbCommand(query, connection))
            {
                command.Parameters.AddWithValue("?", imdbID);
                count = (int)await command.ExecuteScalarAsync();
            }
            connection.Close();

            return count > 0; // Return true if count is greater than zero, indicating the movie is liked
        }

        // Checks if a movie is in the watchlist
        public async Task<bool> MovieExistsInWatchlist(string imdbID)
        {
            string query = "SELECT COUNT(*) FROM Watchlist WHERE imdbID = ?";
            int count = 0;

            await connection.OpenAsync();
            using (var command = new OleDbCommand(query, connection))
            {
                command.Parameters.AddWithValue("?", imdbID);
                count = (int)await command.ExecuteScalarAsync();
            }
            connection.Close();

            return count > 0; // Return true if count is greater than zero, indicating the movie is in the watchlist
        }

        // Adds a movie to the watchlist in the database
        public async Task<bool> AddMovieToWatchlist(MovieDetailFull movieDetails)
        {
            if (await MovieExistsInWatchlist(movieDetails.imdbID))
            {
                return false; // Return false if the movie already exists in watchlist
            }

            string query = "INSERT INTO Watchlist ([Title], [Year], [Poster], [imdbID]) VALUES (?, ?, ?, ?)";
            bool result = false;

            try
            {
                await connection.OpenAsync();
                using (var command = new OleDbCommand(query, connection))
                {
                    command.Parameters.AddWithValue("?", movieDetails.Title);
                    command.Parameters.AddWithValue("?", movieDetails.Year);
                    command.Parameters.AddWithValue("?", movieDetails.Poster);
                    command.Parameters.AddWithValue("?", movieDetails.imdbID);

                    await command.ExecuteNonQueryAsync();
                    result = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to add movie to Watchlist: " + ex.Message);
                throw;
            }
            finally
            {
                connection.Close();
            }

            if (result)
            {
                OnWatchlistMoviesUpdated?.Invoke(); // Trigger the update event
            }

            return result; // Return true when the movie is added successfully
        }

        // Retrieves and updates the ViewModel with liked movies from the database
        public async Task LoadLikedMovies(MovieViewModel viewModel)
        {
            var likedMovies = await FetchLikedMovies();
            viewModel.LikedMovies = new ObservableCollection<MovieDetail>(likedMovies);
        }

        // Retrieves and updates the ViewModel with watchlist movies from the database
        public async Task LoadWatchlistMovies(MovieViewModel viewModel)
        {
            var watchlistMovies = await FetchWatchlistMovies();
            viewModel.WatchlistMovies = new ObservableCollection<MovieDetail>(watchlistMovies);
        }

        // Fetches liked movies from the database
        public async Task<List<MovieDetail>> FetchLikedMovies()
        {
            List<MovieDetail> likedMovies = new List<MovieDetail>();
            string query = "SELECT Title, Year, Poster, imdbID FROM LikedMovies";

            await connection.OpenAsync();

            using (var command = new OleDbCommand(query, connection))
            {
                var reader = await command.ExecuteReaderAsync();

                while (reader.Read())
                {
                    likedMovies.Add(new MovieDetail
                    {
                        Title = reader["Title"].ToString(),
                        Year = reader["Year"].ToString(),
                        Poster = reader["Poster"].ToString(),
                        imdbID = reader["imdbID"].ToString()
                    });
                }
            }

            connection.Close();

            return likedMovies; // Return the list of liked movies
        }

        // Fetches watchlist movies from the database
        public async Task<List<MovieDetail>> FetchWatchlistMovies()
        {
            List<MovieDetail> watchlistMovies = new List<MovieDetail>();
            string query = "SELECT Title, Year, Poster, imdbID FROM Watchlist";

            await connection.OpenAsync();

            using (var command = new OleDbCommand(query, connection))
            {
                var reader = await command.ExecuteReaderAsync();

                while (reader.Read())
                {
                    watchlistMovies.Add(new MovieDetail
                    {
                        Title = reader["Title"].ToString(),
                        Year = reader["Year"].ToString(),
                        Poster = reader["Poster"].ToString(),
                        imdbID = reader["imdbID"].ToString()
                    });
                }
            }

            connection.Close();

            return watchlistMovies; // Return the list of watchlist movies
        }

        // Removes a movie from the liked list in the database
        public async Task RemoveMovieFromLiked(string imdbID)
        {
            string query = "DELETE FROM LikedMovies WHERE imdbID = ?";
            await connection.OpenAsync();
            using (var command = new OleDbCommand(query, connection))
            {
                command.Parameters.AddWithValue("?", imdbID);
                await command.ExecuteNonQueryAsync();
            }
            connection.Close();
            OnLikedMoviesUpdated?.Invoke(); // Trigger the liked movies update event
        }

        // Removes a movie from the watchlist in the database
        public async Task RemoveMovieFromWatchlist(string imdbID)
        {
            string query = "DELETE FROM Watchlist WHERE imdbID = ?";
            await connection.OpenAsync();
            using (var command = new OleDbCommand(query, connection))
            {
                command.Parameters.AddWithValue("?", imdbID);
                await command.ExecuteNonQueryAsync();
            }
            connection.Close();
            OnWatchlistMoviesUpdated?.Invoke(); // Trigger the watchlist movies update event
        }

        // Checks if a movie is already liked based on the title
        public async Task<bool> CheckMovieLiked(string title)
        {
            List<MovieDetail> likedMovies = await FetchLikedMovies();
            return likedMovies.Exists(m => m.Title == title); // Check if the title exists in the liked list
        }

        // Checks if a movie is already in the watchlist based on the title
        public async Task<bool> CheckMovieInWatchlist(string title)
        {
            List<MovieDetail> watchlistMovies = await FetchWatchlistMovies();
            return watchlistMovies.Exists(m => m.Title == title); // Check if the title exists in the watchlist
        }
    }
}
