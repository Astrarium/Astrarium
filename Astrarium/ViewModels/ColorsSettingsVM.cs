using Astrarium.Types;
using System;
using System.Collections.Generic;
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
            Settings.SettingValueChanged += (s, v) =>
            {
                if (s == "Schema")
                {
                    NotifySchemaChanged();
                }
            };
            ColorSettings = settings.OfType<SkyColor>().Select(name => new ColorSetting(settings, name)).ToArray();
        }

        public bool IsNightColorSchema
        {
            get => Settings.Get<ColorSchema>("Schema") == ColorSchema.Night;
            set
            {
                if (value)
                {
                    Settings.Set("Schema", ColorSchema.Night);
                    NotifySchemaChanged();
                }
            }
        }

        public bool IsDayNightColorSchema
        {
            get => Settings.Get<ColorSchema>("Schema") == ColorSchema.Day;
            set
            {
                if (value)
                {
                    Settings.Set("Schema", ColorSchema.Day);
                    NotifySchemaChanged();
                }
            }
        }

        public bool IsRedColorSchema
        {
            get => Settings.Get<ColorSchema>("Schema") == ColorSchema.Red;
            set
            {
                if (value)
                {
                    Settings.Set("Schema", ColorSchema.Red);
                    NotifySchemaChanged();
                }
            }
        }

        public bool IsWhiteColorSchema
        {
            get => Settings.Get<ColorSchema>("Schema") == ColorSchema.White;
            set
            {
                if (value)
                {
                    Settings.Set("Schema", ColorSchema.White);
                    NotifySchemaChanged();
                }
            }
        }

        private void NotifySchemaChanged()
        {
            NotifyPropertyChanged(
                nameof(IsNightColorSchema),
                nameof(IsDayNightColorSchema),
                nameof(IsRedColorSchema),
                nameof(IsWhiteColorSchema)
            );
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

            public SkyColor Value
            {
                get => settings.Get<SkyColor>(name);
                set => settings.Set(name, value);
            }
        }
    }
}
