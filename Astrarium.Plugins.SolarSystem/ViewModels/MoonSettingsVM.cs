using Astrarium.Types;
using System.Runtime;

namespace Astrarium.Plugins.SolarSystem.ViewModels
{
    public class MoonSettingsVM : SettingsViewModel
    {
        public MoonSettingsVM(ISettings settings) : base(settings)
        {
            Settings.SettingValueChanged += Settings_SettingValueChanged;
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

        private void Settings_SettingValueChanged(string settingName, object newValue, object oldValue)
        {
            if (settingName == "MoonTextureQuality")
            {
                NotifyMoonTextureQualityChanged();
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

        public override void Dispose()
        {
            Settings.SettingValueChanged -= Settings_SettingValueChanged;
            base.Dispose();
        }
    }
}
