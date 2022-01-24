using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.ViewModels
{
    public class FontsSettingsVM : ViewModelBase
    {
        /// <summary>
        /// Settings instance
        /// </summary>
        public ISettings Settings { get; private set; }

        /// <summary>
        /// Collection of settings of type <see cref="Font"/>,
        /// needed for building dynamic list of font pickers in UI.
        /// </summary>
        public ICollection<FontSetting> FontSettings { get; private set; }

        public FontsSettingsVM(ISettings settings)
        {
            Settings = settings;
            FontSettings = settings.OfType<Font>().Select(name => new FontSetting(settings, name)).ToArray();
        }

        public class FontSetting : ViewModelBase
        {
            private ISettings settings;
            private string name;

            public FontSetting(ISettings settings, string name)
            {
                this.settings = settings;
                this.name = name;
                Text.LocaleChanged += () => NotifyPropertyChanged(nameof(Title));
            }

            public string Title
            {
                get => Text.Get($"Settings.{name}");
                set { }
            }

            public Font Value
            {
                get => settings.Get<Font>(name);
                set => settings.Set(name, value);
            }
        }
    }
}
