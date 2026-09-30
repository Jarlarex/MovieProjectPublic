using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using MovieProject1.Data;

namespace MovieProject1
{
    public partial class MovieDetailsWindow : Window
    {
        private readonly IMovieRepository _repository;
        private readonly MovieDetailFull _movieDetails;
        private readonly string _trailerUrl;

        public MovieDetailsWindow(
            MovieDetailFull movieDetails,
            string trailerUrl,
            IMovieRepository repository)
        {
            if (movieDetails == null) throw new ArgumentNullException(nameof(movieDetails));
            InitializeComponent();

            _movieDetails = movieDetails;
            _trailerUrl = trailerUrl;
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));

            UpdateMovieDetails();
        }

        private void UpdateMovieDetails()
        {
            if (Uri.TryCreate(_movieDetails.Poster, UriKind.Absolute, out var posterUri))
                PosterImage.Source = new BitmapImage(posterUri);

            TitleTextBlock.Text = _movieDetails.Title ?? string.Empty;
            YearTextBlock.Text = "Year: " + (_movieDetails.Year ?? "N/A");
            RatedTextBlock.Text = "Rated: " + (_movieDetails.Rated ?? "N/A");
            RuntimeTextBlock.Text = "Runtime: " + (_movieDetails.Runtime ?? "N/A");
            GenreTextBlock.Text = "Genre: " + (_movieDetails.Genre ?? "N/A");
            DirectorTextBlock.Text = "Director: " + (_movieDetails.Director ?? "N/A");
            WriterTextBlock.Text = "Writer: " + (_movieDetails.Writer ?? "N/A");
            ActorsTextBlock.Text = "Actors: " + (_movieDetails.Actors ?? "N/A");
            PlotTextBlock.Text = "Plot: " + (_movieDetails.Plot ?? "N/A");
            BoxOfficeTextBlock.Text = "Box Office: " + (_movieDetails.BoxOffice ?? "N/A");

            var rating = (_movieDetails.Ratings ?? new System.Collections.Generic.List<Rating>())
                .FirstOrDefault(r => r != null &&
                    string.Equals(r.Source, "Rotten Tomatoes", StringComparison.OrdinalIgnoreCase));
            RatingsItemsControl.ItemsSource = rating == null ? null : new[] { rating };

            if (!string.IsNullOrWhiteSpace(_trailerUrl))
                LoadTrailer(_trailerUrl);
        }

        private void LoadTrailer(string trailerUrl)
        {
            if (!Uri.TryCreate(trailerUrl, UriKind.Absolute, out var uri))
                return;

            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
            var videoId = query["v"];

            if (string.IsNullOrWhiteSpace(videoId))
                videoId = uri.Segments.LastOrDefault()?.Trim('/');

            if (string.IsNullOrWhiteSpace(videoId))
                return;

            var embedHtml = "<html><head><meta http-equiv='X-UA-Compatible' content='IE=edge'/></head>"
                + "<body style='margin:0;overflow:hidden'>"
                + "<iframe width='100%' height='100%' src='https://www.youtube.com/embed/"
                + Uri.EscapeDataString(videoId)
                + "?autoplay=0&modestbranding=1&rel=0' frameborder='0' allow='autoplay; encrypted-media' allowfullscreen></iframe>"
                + "</body></html>";

            TrailerWebBrowser.NavigateToString(embedHtml);
        }

        private async void LikeButton_Click(object sender, RoutedEventArgs e)
        {
            await ToggleAsync(
                () => _repository.ContainsLikedAsync(_movieDetails.imdbID, CancellationToken.None),
                () => _repository.RemoveLikedAsync(_movieDetails.imdbID, CancellationToken.None),
                () => _repository.AddLikedAsync(_movieDetails, CancellationToken.None),
                "liked movies");
        }

        private async void AddToWatchlistButton_Click(object sender, RoutedEventArgs e)
        {
            await ToggleAsync(
                () => _repository.ContainsWatchlistAsync(_movieDetails.imdbID, CancellationToken.None),
                () => _repository.RemoveWatchlistAsync(_movieDetails.imdbID, CancellationToken.None),
                () => _repository.AddWatchlistAsync(_movieDetails, CancellationToken.None),
                "watchlist");
        }

        private async Task ToggleAsync(
            Func<Task<bool>> contains,
            Func<Task> remove,
            Func<Task> add,
            string listName)
        {
            try
            {
                if (await contains())
                {
                    await remove();
                    MessageBox.Show("Movie removed from " + listName + ".");
                }
                else
                {
                    await add();
                    MessageBox.Show("Movie added to " + listName + ".");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not update " + listName + ": " + ex.Message);
            }
        }
    }
}
