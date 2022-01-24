using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Astrarium.Types
{
    /// <summary>
    /// Base class for all settings ViewModels.
    /// </summary>
    public class SettingsViewModel : ViewModelBase
    {
        public ISettings Settings { get; private set; }
        public SettingsViewModel(ISettings settings)
        {
            Settings = settings;
        }
    }
}
