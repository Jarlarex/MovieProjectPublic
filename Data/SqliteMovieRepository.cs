using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MovieProject1.Data
{
    public sealed class SqliteMovieRepository : IMovieRepository
    {
        private readonly string _connectionString;

        public SqliteMovieRepository(string databasePath)
        {
            if (string.IsNullOrWhiteSpace(databasePath))
                throw new ArgumentException("A database path is required.", nameof(databasePath));

            var directory = Path.GetDirectoryName(databasePath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            _connectionString = "Data Source=" + databasePath + ";Version=3;foreign keys=true;";
        }

        public async Task InitializeAsync(CancellationToken cancellationToken)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

                const string sql = @"
CREATE TABLE IF NOT EXISTS LikedMovies (
    imdbID TEXT NOT NULL PRIMARY KEY,
    Title TEXT NOT NULL,
    Year TEXT,
    Poster TEXT
);

CREATE TABLE IF NOT EXISTS Watchlist (
    imdbID TEXT NOT NULL PRIMARY KEY,
    Title TEXT NOT NULL,
    Year TEXT,
    Poster TEXT
);

CREATE INDEX IF NOT EXISTS IX_LikedMovies_Title ON LikedMovies(Title);
CREATE INDEX IF NOT EXISTS IX_Watchlist_Title ON Watchlist(Title);";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                }
            }
        }

        public Task<IReadOnlyList<MovieDetail>> GetLikedMoviesAsync(CancellationToken cancellationToken)
        {
            return GetMoviesAsync("LikedMovies", cancellationToken);
        }

        public Task<IReadOnlyList<MovieDetail>> GetWatchlistMoviesAsync(CancellationToken cancellationToken)
        {
            return GetMoviesAsync("Watchlist", cancellationToken);
        }

        public Task<bool> ContainsLikedAsync(string imdbId, CancellationToken cancellationToken)
        {
            return ContainsAsync("LikedMovies", imdbId, cancellationToken);
        }

        public Task<bool> ContainsWatchlistAsync(string imdbId, CancellationToken cancellationToken)
        {
            return ContainsAsync("Watchlist", imdbId, cancellationToken);
        }

        public Task AddLikedAsync(MovieDetailFull movie, CancellationToken cancellationToken)
        {
            return AddAsync("LikedMovies", movie, cancellationToken);
        }

        public Task AddWatchlistAsync(MovieDetailFull movie, CancellationToken cancellationToken)
        {
            return AddAsync("Watchlist", movie, cancellationToken);
        }

        public Task RemoveLikedAsync(string imdbId, CancellationToken cancellationToken)
        {
            return RemoveAsync("LikedMovies", imdbId, cancellationToken);
        }

        public Task RemoveWatchlistAsync(string imdbId, CancellationToken cancellationToken)
        {
            return RemoveAsync("Watchlist", imdbId, cancellationToken);
        }

        private async Task<IReadOnlyList<MovieDetail>> GetMoviesAsync(string table, CancellationToken cancellationToken)
        {
            var movies = new List<MovieDetail>();

            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                using (var command = new SQLiteCommand(
                    "SELECT Title, Year, Poster, imdbID FROM " + table + " ORDER BY Title COLLATE NOCASE",
                    connection))
                using (var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false))
                {
                    while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                    {
                        movies.Add(new MovieDetail
                        {
                            Title = reader["Title"] as string,
                            Year = reader["Year"] as string,
                            Poster = reader["Poster"] as string,
                            imdbID = reader["imdbID"] as string
                        });
                    }
                }
            }

            return movies;
        }

        private async Task<bool> ContainsAsync(string table, string imdbId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(imdbId))
                return false;

            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                using (var command = new SQLiteCommand(
                    "SELECT 1 FROM " + table + " WHERE imdbID = @id LIMIT 1", connection))
                {
                    command.Parameters.AddWithValue("@id", imdbId);
                    var result = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
                    return result != null;
                }
            }
        }

        private async Task AddAsync(string table, MovieDetailFull movie, CancellationToken cancellationToken)
        {
            if (movie == null || string.IsNullOrWhiteSpace(movie.imdbID))
                throw new ArgumentException("A valid movie with an IMDb ID is required.", nameof(movie));

            const string columns = "(imdbID, Title, Year, Poster)";
            const string values = "(@id, @title, @year, @poster)";
            var sql = "INSERT OR IGNORE INTO " + table + " " + columns + " VALUES " + values;

            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", movie.imdbID);
                    command.Parameters.AddWithValue("@title", movie.Title ?? string.Empty);
                    command.Parameters.AddWithValue("@year", movie.Year ?? string.Empty);
                    command.Parameters.AddWithValue("@poster", movie.Poster ?? string.Empty);
                    await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                }
            }
        }

        private async Task RemoveAsync(string table, string imdbId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(imdbId))
                return;

            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                using (var command = new SQLiteCommand(
                    "DELETE FROM " + table + " WHERE imdbID = @id", connection))
                {
                    command.Parameters.AddWithValue("@id", imdbId);
                    await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                }
            }
        }
    }
}
