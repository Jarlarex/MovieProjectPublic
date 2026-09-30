using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace MovieProject1
{
    public sealed class MovieViewModel : INotifyPropertyChanged
    {
        private readonly int _itemsPerPage = 10;
        private ObservableCollection<MovieDetail> _movies = new ObservableCollection<MovieDetail>();
        private ObservableCollection<MovieDetail> _likedMovies = new ObservableCollection<MovieDetail>();
        private ObservableCollection<MovieDetail> _watchlistMovies = new ObservableCollection<MovieDetail>();
        private string _errorMessage;
        private bool _isBusy;
        private int _currentPage = 1;
        private int _totalPages = 1;

        public int ItemsPerPage => _itemsPerPage;
        public IReadOnlyList<MovieDetail> AllMovies { get; private set; } = new List<MovieDetail>();

        public ObservableCollection<MovieDetail> Movies
        {
            get => _movies;
            private set { _movies = value; OnPropertyChanged(nameof(Movies)); }
        }

        public ObservableCollection<MovieDetail> LikedMovies
        {
            get => _likedMovies;
            set { _likedMovies = value ?? new ObservableCollection<MovieDetail>(); OnPropertyChanged(nameof(LikedMovies)); }
        }

        public ObservableCollection<MovieDetail> WatchlistMovies
        {
            get => _watchlistMovies;
            set { _watchlistMovies = value ?? new ObservableCollection<MovieDetail>(); OnPropertyChanged(nameof(WatchlistMovies)); }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set { if (_errorMessage != value) { _errorMessage = value; OnPropertyChanged(nameof(ErrorMessage)); } }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set { if (_isBusy != value) { _isBusy = value; OnPropertyChanged(nameof(IsBusy)); OnPropertyChanged(nameof(CanNavigate)); } }
        }

        public int CurrentPage
        {
            get => _currentPage;
            private set { if (_currentPage != value) { _currentPage = value; OnPropertyChanged(nameof(CurrentPage)); OnPropertyChanged(nameof(CanNavigate)); } }
        }

        public int TotalPages
        {
            get => _totalPages;
            private set { if (_totalPages != value) { _totalPages = value; OnPropertyChanged(nameof(TotalPages)); OnPropertyChanged(nameof(CanNavigate)); } }
        }

        public bool CanNavigate => !IsBusy && TotalPages > 1;

        public event PropertyChangedEventHandler PropertyChanged;

        public void SetSearchPage(IEnumerable<MovieDetail> movies, int page, int totalResults)
        {
            AllMovies = (movies ?? Enumerable.Empty<MovieDetail>()).ToList();
            TotalPages = Math.Max(1, (int)Math.Ceiling(totalResults / (double)_itemsPerPage));
            CurrentPage = Math.Max(1, Math.Min(page, TotalPages));
            Movies = new ObservableCollection<MovieDetail>(AllMovies);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
