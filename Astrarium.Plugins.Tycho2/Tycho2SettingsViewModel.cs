using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Astrarium.Plugins.Tycho2
{
    public class Tycho2SettingsViewModel : SettingsViewModel
    {
        public Tycho2Calc Catalog { get; private set; }

        public ICommand OpenCatalogUrlCommand { get; private set; }

        public Func<string, bool> ValidateCatalogPath { get; private set; }

        public Tycho2SettingsViewModel(ISettings settings, Tycho2Calc catalog) : base(settings)
        {
            Catalog = catalog;
            OpenCatalogUrlCommand = new Command(OpenCatalogUrl);
            ValidateCatalogPath = (string path) =>
            {
                return Catalog.Validate(path, verbose: true);
            };
        }

        private void OpenCatalogUrl()
        {
            try
            {
                Process.Start("https://github.com/Astrarium/Tycho2");
            }
            catch { }
        }
    }
}
