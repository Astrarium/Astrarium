using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.SolarSystem.ViewModels
{
    public class MoonSettingsVM : SettingsViewModel
    {
        public MoonSettingsVM(ISettings settings) : base(settings)
        {
            Settings.SettingValueChanged += (s, v) =>
            {
                if (s == "MoonTextureQuality")
                {
                    NotifyMoonTextureQualityChanged();
                }
            };
        }

        public bool IsTextureQualityLow
        {
            get => Settings.Get<TextureQuality>("MoonTextureQuality") == TextureQuality.Low;
            set
            {
                if (value)
                {
                    Settings.Set("MoonTextureQuality", TextureQuality.Low);
                    NotifyMoonTextureQualityChanged();
                }
            }
        }

        public bool IsTextureQualityNormal
        {
            get => Settings.Get<TextureQuality>("MoonTextureQuality") == TextureQuality.Normal;
            set
            {
                if (value)
                {
                    Settings.Set("MoonTextureQuality", TextureQuality.Normal);
                    NotifyMoonTextureQualityChanged();
                }
            }
        }

        public bool IsTextureQualityHigh
        {
            get => Settings.Get<TextureQuality>("MoonTextureQuality") == TextureQuality.High;
            set
            {
                if (value)
                {
                    Settings.Set("MoonTextureQuality", TextureQuality.High);
                    NotifyMoonTextureQualityChanged();
                }
            }
        }

        private void NotifyMoonTextureQualityChanged()
        {
            NotifyPropertyChanged(
                nameof(IsTextureQualityLow),
                nameof(IsTextureQualityNormal),
                nameof(IsTextureQualityHigh)
            );
        }
    }
}
