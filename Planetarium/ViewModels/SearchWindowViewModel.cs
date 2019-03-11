using ADK.Demo;
using ADK.Demo.Objects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.ViewModels
{
    public class SearchWindowViewModel
    {
        private string _SearchString = null;
        public string SearchString
        {
            get
            {
                return _SearchString;
            }
            set
            {
                _SearchString = value;
                DoSearch();
            }
        }

        public Func<CelestialObject, bool> Filter { get; set; } = (b) => true;

        public ObservableCollection<SearchResultItem> SearchResults { get; set; } = new ObservableCollection<SearchResultItem>();

        public SearchResultItem SelectedItem { get; set; }
        private ISearcher searcher;

        public SearchWindowViewModel(Sky sky)
        {
            this.searcher = sky;
        }

        private async void DoSearch()
        {
            var results = await Task.Run(() => searcher.Search(SearchString, Filter));
            SearchResults.Clear();
            foreach (var item in results)
            {
                SearchResults.Add(item);
            }
        }
    }
}
