using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Astrarium.Plugins.Horizon.ViewModels
{
    public class HorizonSettingsViewModel : SettingsViewModel
    {
        private readonly ITextureManager textureManager;
        private readonly ILandscapesManager landscapesManager;

        public ICommand OpenURLCommand { get; private set; }
        public ICommand AddLandscapeCommand { get; private set; }
        public ICommand DeleteLandscapeCommand { get; private set; }

        public HorizonSettingsViewModel(ISettings settings, ITextureManager textureManager, ILandscapesManager landscapesManager) : base(settings)
        {
            this.textureManager = textureManager;
            this.landscapesManager = landscapesManager;

            OpenURLCommand = new Command(() =>
            {
                try
                {
                    Process.Start(new ProcessStartInfo(SelectedLandscape.URL));
                }
                catch (Exception ex)
                {
                    Log.Error("Unable to open browser: " + ex);
                }
            });

            AddLandscapeCommand = new Command(() =>
            {
                string[] files = ViewManager.ShowOpenFileDialog("Choose the landscape", "(PNG files)|*.png", multiSelect: false, out int index);
                if (files != null && files.Any())
                {
                    // TODO: checks

                    var landscape = landscapesManager.CreateLandscape(files[0]);
                    NotifyPropertyChanged(nameof(Landscapes));
                    SelectedLandscape = landscape;
                }
            });

            DeleteLandscapeCommand = new Command<Landscape>((Landscape landscape) => 
            {
                // do not allow remove non-user-defined landscapes
                if (landscape?.UserDefined != true) return;

                if (ViewManager.ShowMessageBox("$Warning", "Do you really want to delete the landscape?", System.Windows.MessageBoxButton.YesNo) != System.Windows.MessageBoxResult.Yes) return;

                if (File.Exists(landscape.Path))
                {
                    try
                    {
                        File.Delete(landscape.Path);
                    }
                    catch (Exception ex)
                    {
                        ViewManager.ShowMessageBox("$Error", ex.Message);
                    }
                }

                landscapesManager.Landscapes.Remove(landscape);
                NotifyPropertyChanged(nameof(Landscapes));
                SelectedLandscape = Landscapes.FirstOrDefault();
            });
        }

        public ObservableCollection<Landscape> Landscapes => new ObservableCollection<Landscape>(landscapesManager.Landscapes);
    
        public Landscape SelectedLandscape
        {
            get => landscapesManager.Landscapes.FirstOrDefault(x => x.Title == Settings.Get<string>("Landscape"));
            set
            {
                if (value != null)
                {

                    string oldLandscapeName = Settings.Get<string>("Landscape");
                    Landscape oldLandscape = landscapesManager.Landscapes.FirstOrDefault(x => x.Title == oldLandscapeName);

                    if (oldLandscape != null)
                    {
                        textureManager.RemoveTexture(oldLandscape.Path);
                    }

                    Settings.Set("Landscape", value.Title);
                    NotifyPropertyChanged(nameof(SelectedLandscape));
                }
            }
        }
    }
}
