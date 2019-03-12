using ADK;
using ADK.Demo;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Planetarium.ViewModels
{
    public class MainVM : ViewModelBase
    {
        private ISkyMap map;
        private Sky sky;

        public string MapEquatorialCoordinatesString { get; private set; }
        public string MapHorizontalCoordinatesString { get; private set; }
        public string MapConstellationNameString { get; private set; }
        public string MapViewAngleString { get; private set; }

        public PointF SkyMousePosition
        {
            set
            {
                var hor = map.Projection.Invert(value);
                var eq = hor.ToEquatorial(sky.Context.GeoLocation, sky.Context.SiderealTime);

                MapEquatorialCoordinatesString = eq.ToString();
                MapHorizontalCoordinatesString = hor.ToString();
                MapConstellationNameString = Constellations.FindConstellation(eq, sky.Context.JulianDay);
                MapViewAngleString = map.ViewAngle.ToString();

                NotifyPropertyChanged(nameof(MapEquatorialCoordinatesString));
                NotifyPropertyChanged(nameof(MapHorizontalCoordinatesString));
                NotifyPropertyChanged(nameof(MapConstellationNameString));
                NotifyPropertyChanged(nameof(MapViewAngleString));
            }
        }

        public MainVM(ISkyMap map, Sky sky)
        {
            this.map = map;
            this.sky = sky;
        }

    }
}
