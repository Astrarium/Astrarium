using Astrarium.Types;
using System;
using System.Globalization;

namespace Astrarium.ViewModels
{
    public class GeneralSettingsVM : SettingsViewModel
    {
        public CultureInfo[] Languages { get; private set; }

        public CultureInfo SelectedLanguage
        {
            get => Text.GetCurrentLocale();
            set
            {
                Settings.Set("Language", value.Name);
                NotifyPropertyChanged(nameof(SelectedLanguage));
            }
        }

        public string[] Themes { get; private set; }

        public string SelectedTheme
        {
            get => Settings.Get("AppTheme", "DeepBlue");
            set
            {
                Settings.Set("AppTheme", value);
                NotifyPropertyChanged(nameof(SelectedTheme));
            }
        }

        public bool IsLocationCheckEnabled => Environment.OSVersion.Version.Major >= 10;

        public GeneralSettingsVM(ISettings settings) : base(settings)
        {
            Languages = Text.GetLocales();
            Themes = new string[] { "DeepBlue", "Graphite", "Marsh" };
            NotifyPropertyChanged(nameof(SelectedLanguage));
            Text.LocaleChanged += () => NotifyPropertyChanged(nameof(SelectedLanguage));
        }
    }
}
