using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Constellations.ViewModels
{
    public class ConstellationsSettingsVM : SettingsViewModel
    {
        public ConstellationsSettingsVM(ISettings settings) : base(settings)
        {
            Settings.SettingValueChanged += (s, v) =>
            {
                if (s == "ConstLabelsType")
                {
                    NotifyConstLabelsTypeChanged();
                }
            };
        }

        public bool IsConstLabelsTypeInternationalCode
        {
            get => Settings.Get<ConstellationsRenderer.LabelType>("ConstLabelsType") == ConstellationsRenderer.LabelType.InternationalCode;
            set
            {
                if (value)
                {
                    Settings.Set("ConstLabelsType", ConstellationsRenderer.LabelType.InternationalCode);
                    NotifyConstLabelsTypeChanged();
                }
            }
        }

        public bool IsConstLabelsTypeInternationalName
        {
            get => Settings.Get<ConstellationsRenderer.LabelType>("ConstLabelsType") == ConstellationsRenderer.LabelType.InternationalName;
            set
            {
                if (value)
                {
                    Settings.Set("ConstLabelsType", ConstellationsRenderer.LabelType.InternationalName);
                    NotifyConstLabelsTypeChanged();
                }
            }
        }

        public bool IsConstLabelsTypeLocalName
        {
            get => Settings.Get<ConstellationsRenderer.LabelType>("ConstLabelsType") == ConstellationsRenderer.LabelType.LocalName;
            set
            {
                if (value)
                {
                    Settings.Set("ConstLabelsType", ConstellationsRenderer.LabelType.LocalName);
                    NotifyConstLabelsTypeChanged();
                }
            }
        }

        public void NotifyConstLabelsTypeChanged()
        {
            NotifyPropertyChanged(
                nameof(IsConstLabelsTypeInternationalCode),
                nameof(IsConstLabelsTypeInternationalName),
                nameof(IsConstLabelsTypeLocalName)
            );
        }
    }
}
