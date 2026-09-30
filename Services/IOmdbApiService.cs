using System.Threading;
using System.Threading.Tasks;

namespace MovieProject1.Services
{
    public interface IOmdbApiService
    {
        Task<MovieSearchResult> SearchAsync(string query, int page, CancellationToken cancellationToken);
        Task<MovieDetailFull> GetDetailsAsync(string imdbId, CancellationToken cancellationToken);
    }
}
