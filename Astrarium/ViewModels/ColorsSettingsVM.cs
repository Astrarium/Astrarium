using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.ViewModels
{
    public class ColorsSettingsVM : SettingsViewModel
    {
        /// <summary>
        /// Collection of settings of type <see cref="SkyColor"/>,
        /// needed for building dynamic list of color pickers in UI.
        /// </summary>
        public ICollection<ColorSetting> ColorSettings { get; private set; }

        public ColorsSettingsVM(ISettings settings) : base(settings)
        {
            ColorSettings = settings.OfType<Color>().Select(name => new ColorSetting(settings, name)).ToArray();
        }

        public class ColorSetting : ViewModelBase
        {
            private ISettings settings;
            private string name;

            public ColorSetting(ISettings settings, string name)
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

            public Color Value
            {
                get => settings.Get<Color>(name);
                set => settings.Set(name, value);
            }
        }
    }
}
