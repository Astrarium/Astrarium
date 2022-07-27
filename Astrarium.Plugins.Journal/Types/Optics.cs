using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public string Type
        {
            get => GetValue<string>(nameof(Type));
            set => SetValue(nameof(Type), value);
        }

        public string OpticsType
        {
            get => GetValue<string>(nameof(OpticsType));
            set => SetValue(nameof(OpticsType), value);
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
