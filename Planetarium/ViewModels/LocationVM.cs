using ADK;
using Planetarium.Calculators;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
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

        private CrdsGeographical _ObserverLocation;
        public CrdsGeographical ObserverLocation
        {
            get
            {
                return _ObserverLocation;
            }
            set
            {
                _ObserverLocation = value;
                NotifyPropertyChanged(
                    nameof(ObserverLocation), 
                    nameof(LatitudeDegrees),
                    nameof(LatitudeMinutes),
                    nameof(LatitudeSeconds),
                    nameof(LatitudeNorth),
                    nameof(LatitudeSouth),
                    nameof(LongitudeDegrees),
                    nameof(LongitudeMinutes),
                    nameof(LongitudeSeconds),
                    nameof(LongitudeEast),
                    nameof(LongitudeWest)
                );
            }
        }
        public double SunHourAngle { get; set; }
        public double SunDeclination { get; set; }

        #region Search properties

        /// <summary>
        /// Collection of found items.
        /// </summary>
        public ObservableCollection<LocationSearchItem> SearchResults { get; private set; } = new ObservableCollection<LocationSearchItem>();

        /// <summary>
        /// Backing field for <see cref="SearchString"/>.
        /// </summary>
        private string _SearchString = null;

        /// <summary>
        /// Search string. Triggers searching process.
        /// </summary>
        public string SearchString
        {
            get
            {
                return _SearchString;
            }
            set
            {
                _SearchString = value;
                NotifyPropertyChanged(nameof(SearchString));
                DoSearch();
            }
        }

        /// <summary>
        /// Searches for geographical locations asynchronously.
        /// </summary>
        private async void DoSearch()
        {
            var results = await Task.Run(() => Search(SearchString.Trim()));
            SearchResults.Clear();
            foreach (var item in results)
            {
                SearchResults.Add(item);
            }            
        }

        private List<LocationSearchItem> Search(string searchString)
        {
            List<LocationSearchItem> results = new List<LocationSearchItem>();
            
            if (FileReader == null)
            {
                var stringPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data", "Cities.dat");
                var fileStream = File.OpenRead(stringPath);
                FileReader = new StreamReader(fileStream, Encoding.UTF8);
            }

            FileReader.BaseStream.Seek(0, SeekOrigin.Begin);

            if (searchString.Length == 0)
            {
                return results;
            }

            string line = null;
            try
            {
                while ((line = FileReader.ReadLine()) != null)
                {
                    string[] chunks = line.Split('\t');

                    var names = new List<string>();
                    names.Add(chunks[1]);
                    names.AddRange(chunks[3].Split(','));
                    var name = names.FirstOrDefault(s => s.Replace("\'", "").StartsWith(searchString, StringComparison.InvariantCultureIgnoreCase));
                    if (name != null)
                    {
                        results.Add(new LocationSearchItem
                        {
                            Name = name,
                            Country = chunks[8],
                            Names = string.Join(", ", names.Distinct().Except(new[] { name }).ToArray())
                        });

                        if (results.Count == 10)
                        {
                            break;
                        }
                    }
                }
            }
            catch { }

            return results.OrderBy(r => r.Name).ToList();
        }

        private StreamReader FileReader;

        #endregion Search properties

        #region Latitude properties

        public int LatitudeDegrees
        {
            get
            {
                return (int)(new DMS(ObserverLocation.Latitude).Degrees);
            }
            set
            {
                var latitude = new DMS(ObserverLocation.Latitude);
                latitude.Degrees = (uint)value;
                ObserverLocation = new CrdsGeographical(latitude.ToDecimalAngle(), ObserverLocation.Longitude, ObserverLocation.UtcOffset, ObserverLocation.Elevation);      
            }
        }

        public int LatitudeMinutes
        {
            get
            {
                return (int)(new DMS(ObserverLocation.Latitude).Minutes);
            }
            set
            {
                var latitude = new DMS(ObserverLocation.Latitude);
                latitude.Minutes = (uint)value;
                ObserverLocation = new CrdsGeographical(latitude.ToDecimalAngle(), ObserverLocation.Longitude, ObserverLocation.UtcOffset, ObserverLocation.Elevation);
            }
        }

        public int LatitudeSeconds
        {
            get
            {
                return (int)(new DMS(ObserverLocation.Latitude).Seconds);
            }
            set
            {
                var latitude = new DMS(ObserverLocation.Latitude);
                latitude.Seconds = (uint)value;
                ObserverLocation = new CrdsGeographical(latitude.ToDecimalAngle(), ObserverLocation.Longitude, ObserverLocation.UtcOffset, ObserverLocation.Elevation);
            }
        }

        public bool LatitudeNorth
        {
            get
            {
                return ObserverLocation.Latitude >= 0;
            }
            set
            {
                if (value != (ObserverLocation.Latitude >= 0))
                {
                    ObserverLocation = new CrdsGeographical(-ObserverLocation.Latitude, ObserverLocation.Longitude, ObserverLocation.UtcOffset, ObserverLocation.Elevation);
                }
            }
        }

        public bool LatitudeSouth
        {
            get
            {
                return ObserverLocation.Latitude < 0;
            }
            set
            {
                if (value != (ObserverLocation.Latitude < 0))
                {
                    ObserverLocation = new CrdsGeographical(-ObserverLocation.Latitude, ObserverLocation.Longitude, ObserverLocation.UtcOffset, ObserverLocation.Elevation);
                }
            }
        }

        #endregion Latitude properties

        #region Longitude properties

        public int LongitudeDegrees
        {
            get
            {
                return (int)(new DMS(ObserverLocation.Longitude).Degrees);
            }
            set
            {
                var longitude = new DMS(ObserverLocation.Longitude);
                longitude.Degrees = (uint)value;
                ObserverLocation = new CrdsGeographical(ObserverLocation.Latitude, longitude.ToDecimalAngle(), ObserverLocation.UtcOffset, ObserverLocation.Elevation);
            }
        }

        public int LongitudeMinutes
        {
            get
            {
                return (int)(new DMS(ObserverLocation.Longitude).Minutes);
            }
            set
            {
                var longitude = new DMS(ObserverLocation.Longitude);
                longitude.Minutes = (uint)value;
                ObserverLocation = new CrdsGeographical(ObserverLocation.Latitude, longitude.ToDecimalAngle(), ObserverLocation.UtcOffset, ObserverLocation.Elevation);
            }
        }

        public int LongitudeSeconds
        {
            get
            {
                return (int)(new DMS(ObserverLocation.Longitude).Seconds);
            }
            set
            {
                var longitude = new DMS(ObserverLocation.Longitude);
                longitude.Seconds = (uint)value;
                ObserverLocation = new CrdsGeographical(ObserverLocation.Latitude, longitude.ToDecimalAngle(), ObserverLocation.UtcOffset, ObserverLocation.Elevation);
            }
        }

        public bool LongitudeEast
        {
            get
            {
                return ObserverLocation.Longitude <= 0;
            }
            set
            {
                if (value != (ObserverLocation.Longitude <= 0))
                {
                    ObserverLocation = new CrdsGeographical(ObserverLocation.Latitude, -ObserverLocation.Longitude, ObserverLocation.UtcOffset, ObserverLocation.Elevation);
                }
            }
        }

        public bool LongitudeWest
        {
            get
            {
                return ObserverLocation.Longitude > 0;
            }
            set
            {
                if (value != (ObserverLocation.Longitude > 0))
                {
                    ObserverLocation = new CrdsGeographical(ObserverLocation.Latitude, -ObserverLocation.Longitude, ObserverLocation.UtcOffset, ObserverLocation.Elevation);
                }
            }
        }

        #endregion Longitude properties

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

    public class LocationSearchItem
    {
        public string Name { get; set; }
        public string Country { get; set; }
        public string Names { get; set; }
    }
}
