using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace MovieProject1
{
    // ViewModel class for managing movie data in the UI
    public class MovieViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<MovieDetail> _movies = new ObservableCollection<MovieDetail>();
        private string _errorMessage;
        private int _currentPage = 0;
        private readonly int _itemsPerPage = 4;

        // Number of items per page for pagination
        public int ItemsPerPage => _itemsPerPage;

        // Collection of movies to be displayed
        public ObservableCollection<MovieDetail> Movies
        {
            get => _movies;
            set
            {
                if (_movies != value)
                {
                    _movies = value;
                    OnPropertyChanged(nameof(Movies));  // Notify UI of change
                }
            }
        }

        // Collection of movies liked by the user
        private ObservableCollection<MovieDetail> _likedMovies = new ObservableCollection<MovieDetail>();
        public ObservableCollection<MovieDetail> LikedMovies
        {
            get => _likedMovies;
            set
            {
                if (_likedMovies != value)
                {
                    _likedMovies = value;
                    OnPropertyChanged(nameof(LikedMovies));  // Notify UI of change
                }
            }
        }

        // Collection of movies added to the watchlist by the user
        private ObservableCollection<MovieDetail> _watchlistMovies = new ObservableCollection<MovieDetail>();
        public ObservableCollection<MovieDetail> WatchlistMovies
        {
            get => _watchlistMovies;
            set
            {
                if (_watchlistMovies != value)
                {
                    _watchlistMovies = value;
                    OnPropertyChanged(nameof(WatchlistMovies));  // Notify UI of change
                }
            }
        }

        // Error message to display in the UI
        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                if (_errorMessage != value)
                {
                    _errorMessage = value;
                    OnPropertyChanged(nameof(ErrorMessage));  // Notify UI of change
                }
            }
        }

        // Current page number in pagination
        public int CurrentPage
        {
            get => _currentPage;
            set
            {
                if (_currentPage != value)
                {
                    _currentPage = value;
                    OnPropertyChanged(nameof(CurrentPage));  // Notify UI of change
                    UpdateVisibleMovies();  // Update list of visible movies based on new page
                }
            }
        }

        // List of all movies (source data)
        public List<MovieDetail> AllMovies { get; set; } = new List<MovieDetail>();

        // Event triggered when a property changes
        public event PropertyChangedEventHandler PropertyChanged;

        // Method to trigger the PropertyChanged event
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Updates the movie collection with new data
        public void UpdateMovies(List<MovieDetail> movieDetails)
        {
            AllMovies = movieDetails;
            CurrentPage = 0;
            UpdateVisibleMovies();  // Refresh the displayed movies
        }

        // Updates the collection of movies to be displayed based on pagination
        private void UpdateVisibleMovies()
        {
            Movies.Clear();
            var visibleMovies = AllMovies.Skip(CurrentPage * _itemsPerPage).Take(_itemsPerPage).ToList();
            visibleMovies.ForEach(m => Movies.Add(m));  // Add visible movies to observable collection
        }
    }
}
