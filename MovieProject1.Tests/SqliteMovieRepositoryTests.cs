using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MovieProject1.Data;

namespace MovieProject1.Tests
{
    [TestClass]
    public class SqliteMovieRepositoryTests
    {
        [TestMethod]
        public async Task InitializeAddDuplicateAndRemoveLikedMovie()
        {
            var databasePath = Path.Combine(
                Path.GetTempPath(),
                "MovieProject1Tests",
                Guid.NewGuid().ToString("N") + ".sqlite");

            try
            {
                var repository = new SqliteMovieRepository(databasePath);
                var cancellationToken = CancellationToken.None;

                await repository.InitializeAsync(cancellationToken);

                var movie = new MovieDetailFull
                {
                    imdbID = "tt1234567",
                    Title = "Test Movie",
                    Year = "2026",
                    Poster = "https://example.test/poster.jpg"
                };

                await repository.AddLikedAsync(movie, cancellationToken);
                await repository.AddLikedAsync(movie, cancellationToken);

                Assert.IsTrue(await repository.ContainsLikedAsync(movie.imdbID, cancellationToken));

                var liked = await repository.GetLikedMoviesAsync(cancellationToken);
                Assert.AreEqual(1, liked.Count);
                Assert.AreEqual("Test Movie", liked.Single().Title);

                await repository.RemoveLikedAsync(movie.imdbID, cancellationToken);

                Assert.IsFalse(await repository.ContainsLikedAsync(movie.imdbID, cancellationToken));
            }
            finally
            {
                if (File.Exists(databasePath))
                    File.Delete(databasePath);

                var directory = Path.GetDirectoryName(databasePath);
                if (!string.IsNullOrEmpty(directory) && Directory.Exists(directory))
                    Directory.Delete(directory, true);
            }
        }
    }
}
