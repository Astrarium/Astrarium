using Astrarium.Plugins.Journal.Database.Entities;
using Astrarium.Plugins.Journal.Types;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Astrarium.Plugins.Journal.ViewModels
{ 
    public class JournalSettingsVM : ViewModelBase
    {
        public ICommand AddSiteCommand { get; private set; }
        public ICommand DeleteSiteCommand { get; private set; }

        public JournalSettingsVM()
        {
            AddSiteCommand = new Command(AddSite);
            DeleteSiteCommand = new Command<Site>(DeleteSite);
        }

        public Site SelectedSite
        {
            get => GetValue<Site>(nameof(SelectedSite));
            set
            {
                if (value != SelectedSite)
                {
                    if (SelectedSite != null)
                    {
                        SelectedSite.DatabasePropertyChanged -= DatabaseManager.SaveDatabaseEntityProperty;
                    }
                }

                SetValue(nameof(SelectedSite), value);

                if (value != null)
                {
                    value.DatabasePropertyChanged += DatabaseManager.SaveDatabaseEntityProperty;
                }
            }
        }

        private async void AddSite()
        {
            await DatabaseManager.AddSite(new Site() { Id = Guid.NewGuid().ToString(), Name = "New site" });
        }

        private async void DeleteSite(Site site)
        {
            if (ViewManager.ShowMessageBox("$Warning", string.Format("Do you really want to delete the observation place «{0}»? The observation place will be removed from all sessions records.", site.Name), System.Windows.MessageBoxButton.YesNo) == System.Windows.MessageBoxResult.Yes)
            {
                await DatabaseManager.DeleteSite(site.Id);
            }
        }
    }
}
