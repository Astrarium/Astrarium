using Astrarium.Types;
using System;

namespace Astrarium.Plugins.Constellations.ViewModels
{
    public class ConstellationsSettingsVM : SettingsViewModel
    {
        public ConstellationsSettingsVM(ISettings settings) : base(settings) { }

        public bool IsConstLabelsTypeInternationalCode
        {
            get => IsPropertyValueEqual("ConstLabelsType", ConstellationsRenderer.LabelType.InternationalCode);
            set => SetPropertyValue("ConstLabelsType", ConstellationsRenderer.LabelType.InternationalCode, value, NotifyConstLabelsTypeChanged);
        }

        public bool IsConstLabelsTypeInternationalName
        {
            get => IsPropertyValueEqual("ConstLabelsType", ConstellationsRenderer.LabelType.InternationalName);
            set => SetPropertyValue("ConstLabelsType", ConstellationsRenderer.LabelType.InternationalName, value, NotifyConstLabelsTypeChanged);
        }

        public bool IsConstLabelsTypeLocalName
        {
            get => IsPropertyValueEqual("ConstLabelsType", ConstellationsRenderer.LabelType.LocalName);
            set => SetPropertyValue("ConstLabelsType", ConstellationsRenderer.LabelType.LocalName, value, NotifyConstLabelsTypeChanged);
        }

        public bool IsConstLinesTypeTraditional
        {
            get => IsPropertyValueEqual("ConstLinesType", ConstellationsCalc.LineType.Traditional);
            set => SetPropertyValue("ConstLinesType", ConstellationsCalc.LineType.Traditional, value, NotifyConstLinesTypeChanged);
        }

        public bool IsConstLinesTypeRey
        {
            get => IsPropertyValueEqual("ConstLinesType", ConstellationsCalc.LineType.Rey);
            set => SetPropertyValue("ConstLinesType", ConstellationsCalc.LineType.Rey, value, NotifyConstLinesTypeChanged);
        }

        public bool IsConstFiguresTypeHevelius
        {
            get => IsPropertyValueEqual("ConstFiguresType", ConstellationsRenderer.FigureType.Hevelius);
            set => SetPropertyValue("ConstFiguresType", ConstellationsRenderer.FigureType.Hevelius, value, NotifyConstFiguresTypeChanged);
        }

        public bool IsConstFiguresTypeModern
        {
            get => IsPropertyValueEqual("ConstFiguresType", ConstellationsRenderer.FigureType.Modern);
            set => SetPropertyValue("ConstFiguresType", ConstellationsRenderer.FigureType.Modern, value, NotifyConstFiguresTypeChanged);
        }

        public bool IsConstFiguresGroupAll
        {
            get => IsPropertyValueEqual("ConstFiguresGroup", ConstellationsRenderer.FigureGroup.All);
            set => SetPropertyValue("ConstFiguresGroup", ConstellationsRenderer.FigureGroup.All, value, NotifyConstFiguresGroupChanged);
        }

        public bool IsConstFiguresGroupZodiac
        {
            get => IsPropertyValueEqual("ConstFiguresGroup", ConstellationsRenderer.FigureGroup.Zodiac);
            set => SetPropertyValue("ConstFiguresGroup", ConstellationsRenderer.FigureGroup.Zodiac, value, NotifyConstFiguresGroupChanged);
        }

        public bool IsConstFiguresGroupCurrent
        {
            get => IsPropertyValueEqual("ConstFiguresGroup", ConstellationsRenderer.FigureGroup.Current);
            set => SetPropertyValue("ConstFiguresGroup", ConstellationsRenderer.FigureGroup.Current, value, NotifyConstFiguresGroupChanged);
        }

        private bool IsPropertyValueEqual<T>(string name, T value)
        {
            return Settings.Get<T>(name).Equals(value);
        }

        private void SetPropertyValue<T>(string name, T typeValue, bool boolValue, Action notifier)
        {
            if (boolValue)
            {
                Settings.Set(name, typeValue);
                notifier();
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

        public void NotifyConstFiguresGroupChanged()
        {
            NotifyPropertyChanged(
                nameof(IsConstFiguresGroupAll),
                nameof(IsConstFiguresGroupZodiac),
                nameof(IsConstFiguresGroupCurrent)
            );
        }
    }
}
