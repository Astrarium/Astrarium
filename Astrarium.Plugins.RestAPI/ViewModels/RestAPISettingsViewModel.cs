using Astrarium.Plugins.RestAPI;
using Astrarium.Types;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Input;

namespace Astrarium.Plugins.ASCOM.ViewModels
{
    public class RestAPISettingsViewModel : SettingsViewModel
    {
        public ICommand OpenSwaggerCommand { get; private set; }
        public IRestAPIServer Server { get; private set; }

        public RestAPISettingsViewModel(IRestAPIServer serverState, ISettings settings) : base(settings)
        {
            OpenSwaggerCommand = new Command(OpenBrowser);
            Server = serverState;
        }

        private void OpenBrowser()
        {
            try
            {
                string baseAddress = $"{Utils.GetRestAPIServerBaseAddress(Settings)}/swagger/";
                Process.Start(baseAddress);
            }
            catch { }
        }
    }
}
