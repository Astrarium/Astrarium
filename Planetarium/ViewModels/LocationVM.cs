using ADK;
using Planetarium.Calculators;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.ViewModels
{
    /// <summary>
    /// Defines ViewModel for the <see cref="Views.LocationWindow"/> View. 
    /// </summary>
    public class LocationVM : ViewModelBase
    {
        public Command OkCommand { get; private set; }
        public Command CancelCommand { get; private set; }


        public CrdsGeographical ObserverLocation { get; set; }
        public double SunHourAngle { get; set; }
        public double SunDeclination { get; set; }

        public LocationVM(Sky sky, ISolarProvider solarProvider)
        {
            ObserverLocation = new CrdsGeographical(sky.Context.GeoLocation);
            SunHourAngle = Coordinates.HourAngle(sky.Context.SiderealTime, 0, solarProvider.Sun.Equatorial.Alpha);
            SunDeclination = solarProvider.Sun.Equatorial.Delta;

            OkCommand = new Command(Ok);
            CancelCommand = new Command(Close);
        }

        public void Ok()
        {
            Close(true);
        }
    }
}
