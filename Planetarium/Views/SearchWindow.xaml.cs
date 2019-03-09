using ADK.Demo;
using ADK.Demo.Objects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Planetarium.Views
{
    /// <summary>
    /// Interaction logic for SearchWindow.xaml
    /// </summary>
    public partial class SearchWindow : Window
    {
        public SearchWindow(Sky sky)
        {
            InitializeComponent();

            var source = (INotifyCollectionChanged)lstResults.ItemsSource;
            DataContext = new SearchWindowViewModel(sky);

            lstResults.ItemContainerGenerator.StatusChanged += ItemContainerGenerator_StatusChanged; ;
        }

        private void ItemContainerGenerator_StatusChanged(object sender, EventArgs e)
        {
            if (lstResults.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
            {
                if (lstResults.HasItems)
                {
                    var item = lstResults.ItemContainerGenerator.ContainerFromIndex(0) as ListBoxItem;
                    item.IsSelected = true;
                }
            }
        }

        private class SearchWindowViewModel
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

            //public SearchResultItem SelectedItem { get; set; }
            public int SelectedIndex { get; set; }
            private ISearcher searcher;

            public ICommand ItemSelectedCommand { get; private set; }

            public SearchWindowViewModel(ISearcher searcher)
            {
                this.searcher = searcher;
            }

            private async void DoSearch()
            {
                var results = await Task.Run(() => searcher.Search(SearchString, Filter));
                SearchResults.Clear();
                foreach (var item in results)
                {
                    SearchResults.Add(item);
                }

                //SelectedItem = SearchResults.Any() ? SearchResults.First() : new SearchResultItem();
            }


            private void OnItemSelected()
            {

            }
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }


    }
}
