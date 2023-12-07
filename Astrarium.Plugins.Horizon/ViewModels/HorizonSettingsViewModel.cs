using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Horizon.ViewModels
{
    public class HorizonSettingsViewModel : SettingsViewModel
    {
        private readonly ITextureManager textureManager;
        private readonly ILandscapesManager landscapesManager;

        public HorizonSettingsViewModel(ISettings settings, ITextureManager textureManager, ILandscapesManager landscapesManager) : base(settings)
        {
            this.textureManager = textureManager;
            this.landscapesManager = landscapesManager;
        }

        public ObservableCollection<Landscape> Landscapes => new ObservableCollection<Landscape>(landscapesManager.Landscapes);
    
        public Landscape SelectedLandscape
        {
            get => landscapesManager.Landscapes.FirstOrDefault(x => x.Title == Settings.Get<string>("Landscape"));
            set
            {
                string oldLandscapeName = Settings.Get<string>("Landscape");
                Landscape oldLandscape = landscapesManager.Landscapes.FirstOrDefault(x => x.Title == oldLandscapeName);
                textureManager.RemoveTexture(oldLandscape.Path);
                Settings.Set("Landscape", value.Title);
            }
        }
    }
}
