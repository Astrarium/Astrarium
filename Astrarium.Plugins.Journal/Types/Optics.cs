using Astrarium.Types;

namespace Astrarium.Plugins.Journal.Types
{
    public class Optics : PropertyChangedBase
    {
        public string Id { get; set; }

        public string Vendor
        {
            get => GetValue<string>(nameof(Vendor));
            set => SetValue(nameof(Vendor), value);
        }

        public string Model
        {
            get => GetValue<string>(nameof(Model));
            set => SetValue(nameof(Model), value);
        }

        public string Scheme
        {
            get => GetValue<string>(nameof(Scheme));
            set => SetValue(nameof(Scheme), value);
        }

        public string Type { get; set; }

        public bool IsTelescope
        {
            get => Type == "Telescope";
            set
            {
                Type = "Telescope";
                NotifyPropertyChanged(nameof(Type), nameof(IsTelescope), nameof(IsFixedOptics));
            }
        }

        public bool IsFixedOptics
        {
            get => Type == "Fixed";
            set
            {
                Type = "Fixed";
                NotifyPropertyChanged(nameof(Type), nameof(IsTelescope), nameof(IsFixedOptics));
            }
        }

        public double Aperture
        {
            get => GetValue(nameof(Aperture), 50.0);
            set => SetValue(nameof(Aperture), value);
        }

        public double FocalLength
        {
            get => GetValue(nameof(FocalLength), 100.0);
            set => SetValue(nameof(FocalLength), value);
        }

        public double Magnification
        {
            get => GetValue(nameof(Magnification), 1.0);
            set => SetValue(nameof(Magnification), value);
        }

        public double TrueField
        {
            get => GetValue(nameof(TrueField), 0.0);
            set => SetValue(nameof(TrueField), value);
        }

        public bool TrueFieldSpecified
        {
            get => GetValue<bool>(nameof(TrueFieldSpecified));
            set => SetValue(nameof(TrueFieldSpecified), value);
        }

        public bool? OrientationErect
        {
            get => GetValue<bool?>(nameof(OrientationErect), null);
            set => SetValue(nameof(OrientationErect), value);
        }

        public bool? OrientationTrueSided
        {
            get => GetValue<bool?>(nameof(OrientationTrueSided), null);
            set => SetValue(nameof(OrientationTrueSided), value);
        }
    }
}
