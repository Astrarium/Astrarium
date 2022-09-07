using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.ViewModels
{
    public class GeneralSettingsVM : ViewModelBase
    {
        /// <summary>
        /// Settings instance
        /// </summary>
        public ISettings Settings { get; private set; }

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

        public DateTimeSyncPeriodItem[] DateTimeSyncPeriods { get; private set; }

        public string SelectedTheme
        {
            get => Settings.Get("AppTheme", "DeepBlue");
            set
            {
                Settings.Set("AppTheme", value);
                NotifyPropertyChanged(nameof(SelectedTheme));
            }
        }

        public GeneralSettingsVM(ISettings settings)
        {
            Settings = settings;
            Languages = Text.GetLocales();
            Themes = new string[] { "DeepBlue", "Graphite" };
            DateTimeSyncPeriods = new int[] { 1, 5, 10, 15, 30, 60, 300, 600 }.Select(v => new DateTimeSyncPeriodItem(v)).ToArray();
            NotifyPropertyChanged(nameof(SelectedLanguage));
            Text.LocaleChanged += () => NotifyPropertyChanged(nameof(SelectedLanguage));
        }

        public class DateTimeSyncPeriodItem
        {
            public int Value { get; set; }
            public string Text => Types.Text.Get($"Settings.DateTimeSyncPeriod.{Value}");
            public DateTimeSyncPeriodItem(int val)
            {
                Value = val;
            }
        }
    }
}
