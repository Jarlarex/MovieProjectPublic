using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MovieProject1.Tests
{
    [TestClass]
    public class MovieViewModelTests
    {
        [TestMethod]
        public void SetSearchPage_UsesApiPageAndResultCount()
        {
            var viewModel = new MovieViewModel();
            var movies = new List<MovieDetail>
            {
                new MovieDetail { Title = "A", imdbID = "tt1" },
                new MovieDetail { Title = "B", imdbID = "tt2" },
                new MovieDetail { Title = "C", imdbID = "tt3" },
                new MovieDetail { Title = "D", imdbID = "tt4" }
            };

            viewModel.SetSearchPage(movies, 2, 25);

            Assert.AreEqual(2, viewModel.CurrentPage);
            Assert.AreEqual(7, viewModel.TotalPages);
            Assert.AreEqual(4, viewModel.Movies.Count);
        }

        [TestMethod]
        public void SetSearchPage_ClampsPageToAvailablePages()
        {
            var viewModel = new MovieViewModel();

            viewModel.SetSearchPage(new List<MovieDetail>(), 99, 0);

            Assert.AreEqual(1, viewModel.CurrentPage);
            Assert.AreEqual(1, viewModel.TotalPages);
            Assert.AreEqual(0, viewModel.Movies.Count);
        }
    }
}
