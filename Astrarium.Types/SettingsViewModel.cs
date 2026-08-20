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
