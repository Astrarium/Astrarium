using Astrarium.Types;
using System;
using System.Diagnostics;
using System.Windows.Input;

namespace Astrarium.Plugins.UCAC4.ViewModels
{
    public class UCAC4SettingsVM : SettingsViewModel
    {
        public UCAC4Catalog Catalog { get; private set; }

        public ICommand OpenCatalogUrlCommand { get; private set; }

        public Func<string, bool> ValidateCatalogPath { get; private set; }

        public UCAC4SettingsVM(ISettings settings, UCAC4Catalog catalog) : base(settings)
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
                Process.Start("https://cdsarc.cds.unistra.fr/ftp/I/322A/UCAC4/");
            }
            catch { }
        }
    }
}
