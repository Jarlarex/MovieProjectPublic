using System.Threading;
using System.Threading.Tasks;

namespace MovieProject1.Services
{
    public interface IYouTubeService
    {
        Task<string> FindTrailerUrlAsync(string title, string year, CancellationToken cancellationToken);
    }
}
