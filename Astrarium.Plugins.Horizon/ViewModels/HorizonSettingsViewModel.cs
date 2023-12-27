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
using System.Windows.Media.Imaging;

namespace Astrarium.Plugins.Horizon.ViewModels
{
    public class HorizonSettingsViewModel : SettingsViewModel
    {
        private readonly ITextureManager textureManager;
        private readonly ILandscapesManager landscapesManager;

        public ICommand OpenURLCommand { get; private set; }
        public ICommand AddLandscapeCommand { get; private set; }
        public ICommand EditLandscapeCommand { get; private set; }
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
                string[] files = ViewManager.ShowOpenFileDialog("$AddLandscapeDialog.Title", Text.Get("AddLandscapeDialog.FilterText"), multiSelect: false, out int index);
                if (files != null && files.Any())
                {
                    string landscapeImageFile = files[0];

                    using (var stream = File.OpenRead(landscapeImageFile))
                    {
                        var decoder = BitmapDecoder.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.Default);

                        if (decoder.CodecInfo.MimeTypes != "image/png")
                        {
                            ViewManager.ShowMessageBox("$Error", "$AddLandscapeDialog.Error.UnknownFormat");
                            return;
                        }

                        int height = decoder.Frames[0].PixelHeight;
                        int width = decoder.Frames[0].PixelWidth;

                        if (width / height != 2)
                        {
                            ViewManager.ShowMessageBox("$Error", "$AddLandscapeDialog.Error.InvalidAspectRatio");
                            return;
                        }

                        if (width > 8192)
                        {
                            ViewManager.ShowMessageBox("$Error", "$AddLandscapeDialog.Error.TooLarge");
                            return;
                        }
                    }

                    var landscape = landscapesManager.CreateLandscape(landscapeImageFile);
                    NotifyPropertyChanged(nameof(Landscapes));
                    SelectedLandscape = landscape;
                }
            });

            EditLandscapeCommand = new Command<Landscape>((Landscape landscape) =>
            {
                var vm = ViewManager.CreateViewModel<LandscapeEditorViewModel>();

                vm.Title = landscape.Title;
                vm.Author = landscape.Author;
                vm.AzimuthShift = (decimal)landscape.AzimuthShift;
                vm.Copyright = landscape.Copyright;
                vm.Description = landscape.Description;
                vm.URL = landscape.URL;

                if (ViewManager.ShowDialog(vm) == true)
                {
                    landscape.Title = vm.Title;
                    landscape.Author = vm.Author;
                    landscape.AzimuthShift = (double)vm.AzimuthShift;
                    landscape.Copyright = vm.Copyright;
                    landscape.Description = vm.Description;
                    landscape.URL = vm.URL;

                    landscapesManager.SaveLandscapeMetadata(landscape);
                    SelectedLandscape = landscape;
                    NotifyPropertyChanged(nameof(Landscapes), nameof(SelectedLandscape));
                }
            });

            DeleteLandscapeCommand = new Command<Landscape>((Landscape landscape) => 
            {
                // do not allow remove non-user-defined landscapes
                if (landscape?.UserDefined != true) return;

                if (ViewManager.ShowMessageBox("$Warning", "$DeleteLandscapeDialog.Confirmation", System.Windows.MessageBoxButton.YesNo) != System.Windows.MessageBoxResult.Yes) return;

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
