using Planetarium.Objects;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.ViewModels
{
    /// <summary>
    /// Defines a ViewModel used by <see cref="Views.SearchWindow"/> view.
    /// </summary>
    public class SearchVM : ViewModelBase
    {
        /// <summary>
        /// Searcher instance. Injected by IoC container.
        /// </summary>
        private readonly ISearcher searcher;

        /// <summary>
        /// Filter used by searching.
        /// </summary>
        private readonly Func<CelestialObject, bool> filter = (b) => true;

        /// <summary>
        /// Creates new instance of the ViewModel.
        /// </summary>
        /// <param name="searcher"></param>
        public SearchVM(ISearcher searcher)
        {
            this.searcher = searcher;
        }

        /// <summary>
        /// Backing field for <see cref="SearchString"/>.
        /// </summary>
        private string _SearchString = null;

        /// <summary>
        /// Search string. Triggers searching process.
        /// </summary>
        public string SearchString
        {
            get
            {
                return _SearchString;
            }
            set
            {
                _SearchString = value;
                NotifyPropertyChanged(nameof(SearchString));
                DoSearch();
            }
        }

        /// <summary>
        /// Backing field for <see cref="SelectedItem"/>.
        /// </summary>
        private SearchResultItem _SelectedItem;

        /// <summary>
        /// Selected item in the searching results.
        /// </summary>
        public SearchResultItem SelectedItem
        {
            get { return _SelectedItem; } 
            set
            {
                _SelectedItem = value;
                NotifyPropertyChanged(nameof(SelectedItem));
            }
        }

        /// <summary>
        /// Collection of found items.
        /// </summary>
        public ObservableCollection<SearchResultItem> SearchResults { get; private set; } = new ObservableCollection<SearchResultItem>();

        /// <summary>
        /// Searches for celestial objects asynchronously.
        /// </summary>
        private async void DoSearch()
        {
            var results = await Task.Run(() => searcher.Search(SearchString, filter));
            SearchResults.Clear();
            foreach (var item in results)
            {
                SearchResults.Add(item);
            }
        }
    }
}
