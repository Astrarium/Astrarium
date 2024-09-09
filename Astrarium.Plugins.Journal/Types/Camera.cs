using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Journal.Types
{
    public class Camera : PropertyChangedBase
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

        public int PixelsX
        {
            get => GetValue<int>(nameof(PixelsX));
            set => SetValue(nameof(PixelsX), value);
        }

        public int PixelsY
        {
            get => GetValue<int>(nameof(PixelsY));
            set => SetValue(nameof(PixelsY), value);
        }

        public double? PixelXSize
        {
            get => GetValue<double?>(nameof(PixelXSize));
            set => SetValue(nameof(PixelXSize), value);
        }

        public double? PixelYSize
        {
            get => GetValue<double?>(nameof(PixelYSize));
            set => SetValue(nameof(PixelYSize), value);
        }

        public int Binning
        {
            get => GetValue<int>(nameof(Binning));
            set => SetValue(nameof(Binning), value);
        }

        public string Remarks
        {
            get => GetValue<string>(nameof(Remarks));
            set => SetValue(nameof(Remarks), value);
        }
    }
}
