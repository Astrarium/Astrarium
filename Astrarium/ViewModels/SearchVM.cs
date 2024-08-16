using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Astrarium.ViewModels
{
    /// <summary>
    /// Defines a ViewModel used by <see cref="Views.SearchWindow"/> view.
    /// </summary>
    public class SearchVM : ViewModelBase
    {
        /// <summary>
        /// Sky instance. Injected by IoC container.
        /// </summary>
        private readonly ISky sky;

        /// <summary>
        /// Filter used by searching.
        /// </summary>
        public Func<CelestialObject, bool> Filter = (b) => true;

        /// <summary>
        /// Creates new instance of the ViewModel.
        /// </summary>
        /// <param name="sky"></param>
        public SearchVM(ISky sky)
        {
            this.sky = sky;
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
                NotifyPropertyChanged(nameof(SearchString), nameof(EmptySearchString), nameof(NothingFound));
                DoSearch();
            }
        }

        public bool NothingFound => !EmptySearchString && SelectedItem == null;

        public bool EmptySearchString => string.IsNullOrEmpty(SearchString);

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
                NotifyPropertyChanged(nameof(SelectedItem), nameof(EmptySearchString), nameof(NothingFound));
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
            ICollection<CelestialObject> results = !string.IsNullOrWhiteSpace(SearchString) ?
                await Task.Run(() => sky.Search(SearchString, Filter)) :
                new CelestialObject[0];

            SearchResults.Clear();
            foreach (var item in results)
            {
                SearchResults.Add(new SearchResultItem(item, string.Join(", ", item.Names)));
            }

            NotifyPropertyChanged(nameof(SearchResults));

            SelectedItem = SearchResults.Any() ? SearchResults[0] : null;
        }
    }

    public class SearchResultItem
    {
        public string Name { get; private set; }
        public CelestialObject Body { get; private set; }

        public SearchResultItem(CelestialObject body, string name)
        {
            Body = body;
            Name = name;
        }
    }
}
