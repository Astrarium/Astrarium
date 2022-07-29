using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Journal.Types
{
    public class Eyepiece : PropertyChangedBase
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

        public double FocalLength
        {
            get => GetValue(nameof(FocalLength), 10.0);
            set => SetValue(nameof(FocalLength), value);
        }

        public double MaxFocalLength
        {
            get => GetValue(nameof(MaxFocalLength), 20.0);
            set => SetValue(nameof(MaxFocalLength), value);
        }

        public bool IsZoomEyepiece
        {
            get => GetValue<bool>(nameof(IsZoomEyepiece));
            set => SetValue(nameof(IsZoomEyepiece), value);
        }

        public double ApparentFOV
        {
            get => GetValue(nameof(ApparentFOV), 50.0);
            set => SetValue(nameof(ApparentFOV), value);
        }

        public bool ApparentFOVSpecified
        {
            get => GetValue<bool>(nameof(ApparentFOVSpecified));
            set => SetValue(nameof(ApparentFOVSpecified), value);
        }
    }
}
