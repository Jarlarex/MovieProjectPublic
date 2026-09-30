using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MovieProject1.Data;
using MovieProject1.Infrastructure;
using MovieProject1.Services;

namespace MovieProject1
{
    public partial class MainWindow : Window
    {
        private readonly IOmdbApiService _omdbApiService;
        private readonly IYouTubeService _youTubeService;
        private readonly IMovieRepository _repository;
        private readonly MovieViewModel _viewModel = new MovieViewModel();
        private readonly CancellationTokenSource _lifetimeCts = new CancellationTokenSource();
        private CancellationTokenSource _searchCts;
        private string _lastSearch;

        public MovieViewModel ViewModel => _viewModel;

        public MainWindow()
        {
            InitializeComponent();

            var httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(15)
            };

            _omdbApiService = new OmdbApiService(httpClient, AppConfiguration.OmdbApiKey);
            _youTubeService = new YouTubeService(httpClient, AppConfiguration.YouTubeApiKey);

            var databasePath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MovieProject1",
                "MovieProject.sqlite");

            _repository = new SqliteMovieRepository(databasePath);
            DataContext = _viewModel;
            Loaded += MainWindow_Loaded;
            Closed += MainWindow_Closed;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await _repository.InitializeAsync(_lifetimeCts.Token);
                await LoadLibraryAsync(_lifetimeCts.Token);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                _viewModel.ErrorMessage = "Could not initialize the local library: " + ex.Message;
            }
        }

        private async Task LoadLibraryAsync(CancellationToken cancellationToken)
        {
            var liked = await _repository.GetLikedMoviesAsync(cancellationToken);
            var watchlist = await _repository.GetWatchlistMoviesAsync(cancellationToken);

            _viewModel.LikedMovies = new ObservableCollection<MovieDetail>(liked.ToList());
            _viewModel.WatchlistMovies = new ObservableCollection<MovieDetail>(watchlist.ToList());
        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            await SearchAsync(1);
        }

        private async Task SearchAsync(int page)
        {
            var query = SearchBox.Text?.Trim();

            if (string.IsNullOrWhiteSpace(query))
            {
                _viewModel.ErrorMessage = "Enter a movie title to search.";
                return;
            }

            _searchCts?.Cancel();
            _searchCts?.Dispose();
            _searchCts = CancellationTokenSource.CreateLinkedTokenSource(_lifetimeCts.Token);
            var token = _searchCts.Token;

            _viewModel.IsBusy = true;
            _viewModel.ErrorMessage = null;

            try
            {
                var result = await _omdbApiService.SearchAsync(query, page, token);
                token.ThrowIfCancellationRequested();

                var totalResults = 0;
                int.TryParse(result.TotalResults, out totalResults);

                _lastSearch = query;
                _viewModel.SetSearchPage(result.Movies, page, totalResults);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                _viewModel.ErrorMessage = ex.Message;
            }
            finally
            {
                if (!token.IsCancellationRequested)
                    _viewModel.IsBusy = false;
            }
        }

        private async void PrevPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.CurrentPage <= 1 || string.IsNullOrWhiteSpace(_lastSearch))
                return;

            await SearchAsync(_viewModel.CurrentPage - 1);
        }

        private async void NextPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.CurrentPage >= _viewModel.TotalPages || string.IsNullOrWhiteSpace(_lastSearch))
                return;

            await SearchAsync(_viewModel.CurrentPage + 1);
        }

        private async void MoviesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await OpenSelectedMovieAsync(sender, e);
        }

        private async void LikedMoviesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await OpenSelectedMovieAsync(sender, e);
        }

        private async void WatchlistMoviesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await OpenSelectedMovieAsync(sender, e);
        }

        private async Task OpenSelectedMovieAsync(object sender, SelectionChangedEventArgs e)
        {
            if (!(sender is ListView listView) || e.AddedItems.Count == 0)
                return;

            var movie = e.AddedItems[0] as MovieDetail;
            listView.SelectedItem = null;

            if (movie == null || string.IsNullOrWhiteSpace(movie.imdbID))
                return;

            try
            {
                var detailTask = _omdbApiService.GetDetailsAsync(movie.imdbID, _lifetimeCts.Token);
                var trailerTask = _youTubeService.FindTrailerUrlAsync(movie.Title, movie.Year, _lifetimeCts.Token);

                await Task.WhenAll(detailTask, trailerTask);

                var window = new MovieDetailsWindow(
                    detailTask.Result,
                    trailerTask.Result,
                    _repository);

                window.Owner = this;
                window.Show();
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                _viewModel.ErrorMessage = "Could not load movie details: " + ex.Message;
            }
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            _searchCts?.Cancel();
            _searchCts?.Dispose();
            _lifetimeCts.Cancel();
            _lifetimeCts.Dispose();
        }
    }
}
