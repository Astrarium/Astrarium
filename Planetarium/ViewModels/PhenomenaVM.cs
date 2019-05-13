using ADK;
using Planetarium.Objects;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.ViewModels
{
    public class PhenomenaVM : ViewModelBase
    {
        public Command SaveToFileCommand { get; private set; }
        public Command CloseCommand { get; private set; }

        private readonly IViewManager viewManager;
        private readonly Sky sky;

        public ICollection<AstroEvent> AstroEvents { get; set; }

        public PhenomenaVM(IViewManager viewManager, Sky sky)
        {
            this.viewManager = viewManager;
            this.sky = sky;

            SaveToFileCommand = new Command(SaveToFile);
            CloseCommand = new Command(Close);
        }

        private void SaveToFile()
        {
            var result = viewManager.ShowSaveFileDialog("Save to file", "Ephemerides", ".csv", "Text files (*.txt)|*.txt|Comma-separated files (*.csv)|*.csv");
            if (result != null)
            {
                
            }
        }
    }
}
