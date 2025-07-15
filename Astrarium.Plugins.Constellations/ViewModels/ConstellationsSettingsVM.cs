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

        public bool IsConstLinesTypeTraditional
        {
            get => Settings.Get<ConstellationsCalc.LineType>("ConstLinesType") == ConstellationsCalc.LineType.Traditional;
            set
            {
                if (value)
                {
                    Settings.Set("ConstLinesType", ConstellationsCalc.LineType.Traditional);
                    NotifyConstLinesTypeChanged();
                }
            }
        }

        public bool IsConstLinesTypeRey
        {
            get => Settings.Get<ConstellationsCalc.LineType>("ConstLinesType") == ConstellationsCalc.LineType.Rey;
            set
            {
                if (value)
                {
                    Settings.Set("ConstLinesType", ConstellationsCalc.LineType.Rey);
                    NotifyConstLinesTypeChanged();
                }
            }
        }

        public bool IsConstFiguresTypeHevelius
        {
            get => Settings.Get<ConstellationsRenderer.FigureType>("ConstFiguresType") == ConstellationsRenderer.FigureType.Hevelius;
            set
            {
                if (value)
                {
                    Settings.Set("ConstFiguresType", ConstellationsRenderer.FigureType.Hevelius);
                    NotifyConstFiguresTypeChanged();
                }
            }
        }

        public bool IsConstFiguresTypeModern
        {
            get => Settings.Get<ConstellationsRenderer.FigureType>("ConstFiguresType") == ConstellationsRenderer.FigureType.Modern;
            set
            {
                if (value)
                {
                    Settings.Set("ConstFiguresType", ConstellationsRenderer.FigureType.Modern);
                    NotifyConstFiguresTypeChanged();
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

        public void NotifyConstLinesTypeChanged()
        {
            NotifyPropertyChanged(
                nameof(IsConstLinesTypeTraditional),
                nameof(IsConstLinesTypeRey)
            );
        }

        public void NotifyConstFiguresTypeChanged()
        {
            NotifyPropertyChanged(
                nameof(IsConstFiguresTypeHevelius),
                nameof(IsConstFiguresTypeModern)
            );
        }
    }
}
