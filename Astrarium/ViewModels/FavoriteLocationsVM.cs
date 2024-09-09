using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.ViewModels
{
    public class FavoriteLocationsVM : ViewModelBase
    {
        public ObservableCollection<CrdsGeographical> FavoriteLocations { get; private set; }
        public bool IsEmptyList => !FavoriteLocations.Any();

        public Command CloseCommand { get; }

        public Command<CrdsGeographical> EditCommand { get; }
        public Command<CrdsGeographical> DeleteCommand { get; }
        public Command AddCommand { get; }

        private ISettings settings;

        private const string FAVORITE_LOCATIONS_SETTING = "FavoriteLocations";

        public FavoriteLocationsVM(ISettings settings)
        {
            this.settings = settings;
            FavoriteLocations = new ObservableCollection<CrdsGeographical>(settings.Get(FAVORITE_LOCATIONS_SETTING, new List<CrdsGeographical>()));
            FavoriteLocations.CollectionChanged += FavoriteLocations_CollectionChanged;

            CloseCommand = new Command(Close);
            AddCommand = new Command(AddLocation);
            EditCommand = new Command<CrdsGeographical>(EditLocation);
            DeleteCommand = new Command<CrdsGeographical>(DeleteLocation);
        }

        private void FavoriteLocations_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            NotifyPropertyChanged(nameof(IsEmptyList));
        }

        private void AddLocation()
        {
            var location = ViewManager.ShowLocationDialog(settings.Get<CrdsGeographical>("ObserverLocation"));
            if (location != null)
            {
                FavoriteLocations.Add(location);
                settings.SetAndSave(FAVORITE_LOCATIONS_SETTING, FavoriteLocations.ToList());
            }
        }

        private void EditLocation(CrdsGeographical location)
        {
            var edited = ViewManager.ShowLocationDialog(location);
            if (edited != null)
            {
                int index = FavoriteLocations.IndexOf(location);
                FavoriteLocations.RemoveAt(index);
                FavoriteLocations.Insert(index, edited);
                settings.SetAndSave(FAVORITE_LOCATIONS_SETTING, FavoriteLocations.ToList());
            }
        }

        private void DeleteLocation(CrdsGeographical location)
        {
            if (ViewManager.ShowMessageBox("$Warning", "$FavoriteLocationsWindow.DeleteConfirm", System.Windows.MessageBoxButton.YesNo) == System.Windows.MessageBoxResult.Yes)
            {
                FavoriteLocations.Remove(location);
                settings.SetAndSave(FAVORITE_LOCATIONS_SETTING, FavoriteLocations.ToList());
            }
        }
    }
}
