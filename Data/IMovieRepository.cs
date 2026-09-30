using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MovieProject1.Data
{
    public interface IMovieRepository
    {
        Task InitializeAsync(CancellationToken cancellationToken);
        Task<IReadOnlyList<MovieDetail>> GetLikedMoviesAsync(CancellationToken cancellationToken);
        Task<IReadOnlyList<MovieDetail>> GetWatchlistMoviesAsync(CancellationToken cancellationToken);
        Task<bool> ContainsLikedAsync(string imdbId, CancellationToken cancellationToken);
        Task<bool> ContainsWatchlistAsync(string imdbId, CancellationToken cancellationToken);
        Task AddLikedAsync(MovieDetailFull movie, CancellationToken cancellationToken);
        Task AddWatchlistAsync(MovieDetailFull movie, CancellationToken cancellationToken);
        Task RemoveLikedAsync(string imdbId, CancellationToken cancellationToken);
        Task RemoveWatchlistAsync(string imdbId, CancellationToken cancellationToken);
    }
}
