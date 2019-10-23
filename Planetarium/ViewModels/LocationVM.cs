using ADK;
using Planetarium.Calculators;
using Planetarium.Types;
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
        /// <summary>
        /// Executed when user clicks on OK button
        /// </summary>
        public Command OkCommand { get; private set; }

        /// <summary>
        /// Executed when user cancels the view
        /// </summary>
        public Command CancelCommand { get; private set; }

        /// <summary>
        /// Executed when search mode is off
        /// </summary>
        public Command EndSearchModeCommand { get; private set; }

        /// <summary>
        /// Executed when user selects a location from the search results list
        /// </summary>
        public Command SelectLocationCommand { get; private set; }

        /// <summary>
        /// Previously saved location, needed in case when user cancels the search mode
        /// </summary>
        private CrdsGeographical _SavedLocation;

        /// <summary>
        /// Backing field for <see cref="ObserverLocation"/>
        /// </summary>
        private CrdsGeographical _ObserverLocation;

        /// <summary>
        /// Current observer location chosen in the view. 
        /// Executing the <see cref="OkCommand"/> will propagate the value as dialog result.
        /// </summary>
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
                    nameof(LongitudeWest),
                    nameof(TimeZone),
                    nameof(LocationName)
                );
            }
        }

        /// <summary>
        /// Flag indicating night mode is on
        /// </summary>
        public bool IsNightMode { get; private set; }

        /// <summary>
        /// Current hour angle of the Sun at Greenwich
        /// </summary>
        public double SunHourAngle { get; private set; }

        /// <summary>
        /// Current declination of the Sun
        /// </summary>
        public double SunDeclination { get; private set; }

        /// <summary>
        /// Gets value indicating the search mode is on
        /// </summary>
        public bool SearchMode
        {
            get
            {
                return !string.IsNullOrWhiteSpace(_SearchString);
            }
        }

        /// <summary>
        /// Gets value indicating there are no search results
        /// </summary>
        public bool NoResults
        {
            get
            {
                return SearchMode && !SearchResults.Any();
            }
        }

        #region Search properties

        /// <summary>
        /// Collection of found items.
        /// </summary>
        public ObservableCollection<LocationSearchItem> SearchResults { get; private set; } = new ObservableCollection<LocationSearchItem>();

        /// <summary>
        /// Backing field for <see cref="SelectedItem"/>
        /// </summary>
        private LocationSearchItem _SelectedItem;

        /// <summary>
        /// Gets or sets location item currently selected in the search list
        /// </summary>
        public LocationSearchItem SelectedItem
        {
            get { return _SelectedItem; }
            set
            {
                _SelectedItem = value;
                if (_SelectedItem != null)
                {
                    ObserverLocation = _SelectedItem.Location;
                }
                NotifyPropertyChanged(nameof(SelectedItem), nameof(TimeZone));
            }
        }

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
                bool searchMode = SearchMode;
                NotifyPropertyChanged(nameof(SearchString), nameof(SearchMode));
                if (SearchMode)
                {
                    if (!searchMode)
                    {
                        _SavedLocation = new CrdsGeographical(ObserverLocation);
                    }
                    DoSearch();
                }

                if (!SearchMode && searchMode)
                {
                    ObserverLocation = new CrdsGeographical(_SavedLocation);
                }

                NotifyPropertyChanged(nameof(NoResults));
            }
        }

        private string _LastSearchString = null;

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
            SelectedItem = SearchResults.Any() ? SearchResults[0] : null;
            NotifyPropertyChanged(nameof(NoResults));
        }

        /// <summary>
        /// Searches for geographical locations synchronously.
        /// </summary>
        /// <param name="searchString">String to search</param>
        /// <returns>List of location items matching the specified search string</returns>
        private List<LocationSearchItem> Search(string searchString)
        {
            List<LocationSearchItem> results = new List<LocationSearchItem>();

            if (searchString.Length == 0)
            {
                return results;
            }


            var stringPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data", "Cities.dat");
            FileStream _FileStream = File.OpenRead(stringPath);

            using (var fileReader = new StreamReader(_FileStream, Encoding.UTF8))
            {
                _LastSearchString = searchString;

                string line = null;
                try
                {
                    while ((line = fileReader.ReadLine()) != null && searchString.Equals(_LastSearchString))
                    {
                        string[] chunks = line.Split('\t');

                        var names = new List<string>();
                        names.Add(chunks[1]);
                        names.AddRange(chunks[3].Split(','));
                        var name = names.FirstOrDefault(s => s.Replace("\'", "").StartsWith(searchString, StringComparison.InvariantCultureIgnoreCase));
                        if (name != null)
                        {
                            double latitude = double.Parse(chunks[4], CultureInfo.InvariantCulture);
                            double longitude = double.Parse(chunks[5], CultureInfo.InvariantCulture);
                            double elevation = double.Parse(string.IsNullOrWhiteSpace(chunks[15]) ? "0" : chunks[15], CultureInfo.InvariantCulture);
                            TimeZoneItem timeZone = TimeZones.FirstOrDefault(tz => tz.TimeZoneId.Equals(chunks[17], StringComparison.InvariantCultureIgnoreCase));

                            results.Add(new LocationSearchItem
                            {
                                Name = name,
                                Country = chunks[8],
                                Names = string.Join(", ", names.Distinct().Except(new[] { name }).ToArray()),
                                Location = new CrdsGeographical(-longitude, latitude, timeZone.UtcOffset, elevation, timeZone.TimeZoneId, name)
                            });

                            if (results.Count == 10)
                            {
                                break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {

                }

                return results.OrderBy(r => r.Name).ToList();
            }
        }

        #endregion Search properties

        #region Latitude properties

        /// <summary>
        /// Gets/sets degrees part of observer location's latitude
        /// </summary>
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
                ObserverLocation = new CrdsGeographical(ObserverLocation.Longitude, latitude.ToDecimalAngle(), ObserverLocation.UtcOffset, ObserverLocation.Elevation, ObserverLocation.TimeZoneId, ObserverLocation.LocationName);      
            }
        }

        /// <summary>
        /// Gets/sets minutes part of observer location's latitude
        /// </summary>
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
                ObserverLocation = new CrdsGeographical(ObserverLocation.Longitude, latitude.ToDecimalAngle(), ObserverLocation.UtcOffset, ObserverLocation.Elevation, ObserverLocation.TimeZoneId, ObserverLocation.LocationName);
            }
        }

        /// <summary>
        /// Gets/sets seconds part of observer location's latitude
        /// </summary>
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
                ObserverLocation = new CrdsGeographical(ObserverLocation.Longitude, latitude.ToDecimalAngle(), ObserverLocation.UtcOffset, ObserverLocation.Elevation, ObserverLocation.TimeZoneId, ObserverLocation.LocationName);
            }
        }

        /// <summary>
        /// Gets/sets value indicating observer location is in North hemisphere
        /// </summary>
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
                    ObserverLocation = new CrdsGeographical(ObserverLocation.Longitude, -ObserverLocation.Latitude, ObserverLocation.UtcOffset, ObserverLocation.Elevation, ObserverLocation.TimeZoneId, ObserverLocation.LocationName);
                }
            }
        }

        /// <summary>
        /// Gets/sets value indicating observer location is in South hemisphere
        /// </summary>
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
                    ObserverLocation = new CrdsGeographical(ObserverLocation.Longitude, -ObserverLocation.Latitude, ObserverLocation.UtcOffset, ObserverLocation.Elevation, ObserverLocation.TimeZoneId, ObserverLocation.LocationName);
                }
            }
        }

        #endregion Latitude properties

        #region Longitude properties

        /// <summary>
        /// Gets/sets degrees part of observer location's longitude
        /// </summary>
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
                ObserverLocation = new CrdsGeographical(longitude.ToDecimalAngle(), ObserverLocation.Latitude, ObserverLocation.UtcOffset, ObserverLocation.Elevation, ObserverLocation.TimeZoneId, ObserverLocation.LocationName);
            }
        }

        /// <summary>
        /// Gets/sets minutes part of observer location's longitude
        /// </summary>
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
                ObserverLocation = new CrdsGeographical(longitude.ToDecimalAngle(), ObserverLocation.Latitude, ObserverLocation.UtcOffset, ObserverLocation.Elevation, ObserverLocation.TimeZoneId, ObserverLocation.LocationName);
            }
        }

        /// <summary>
        /// Gets/sets seconds part of observer location's longitude
        /// </summary>
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
                ObserverLocation = new CrdsGeographical(longitude.ToDecimalAngle(), ObserverLocation.Latitude, ObserverLocation.UtcOffset, ObserverLocation.Elevation, ObserverLocation.TimeZoneId, ObserverLocation.LocationName);
            }
        }

        /// <summary>
        /// Gets/sets value indicating observer location is in East hemisphere
        /// </summary>
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
                    ObserverLocation = new CrdsGeographical(-ObserverLocation.Longitude, ObserverLocation.Latitude, ObserverLocation.UtcOffset, ObserverLocation.Elevation, ObserverLocation.TimeZoneId, ObserverLocation.LocationName);
                }
            }
        }

        /// <summary>
        /// Gets/sets value indicating observer location is in West hemisphere
        /// </summary>
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
                    ObserverLocation = new CrdsGeographical(-ObserverLocation.Longitude, ObserverLocation.Latitude, ObserverLocation.UtcOffset, ObserverLocation.Elevation, ObserverLocation.TimeZoneId, ObserverLocation.LocationName);
                }
            }
        }

        #endregion Longitude properties

        #region TimeZone properties

        /// <summary>
        /// Gets list of available time zones loaded from the file
        /// </summary>
        public ObservableCollection<TimeZoneItem> TimeZones { get; private set; } = new ObservableCollection<TimeZoneItem>();

        /// <summary>
        /// Gets/sets a time zone associated with the observer location
        /// </summary>
        public TimeZoneItem TimeZone
        {
            get
            {
                return TimeZones.FirstOrDefault(tz => tz.TimeZoneId.Equals(ObserverLocation.TimeZoneId, StringComparison.InvariantCultureIgnoreCase));
            }
            set
            {
                if (value != null)
                {
                    ObserverLocation = new CrdsGeographical(ObserverLocation.Longitude, ObserverLocation.Latitude, value.UtcOffset, ObserverLocation.Elevation, value.TimeZoneId, ObserverLocation.LocationName);
                }
                NotifyPropertyChanged(nameof(TimeZone));
            }
        }

        /// <summary>
        /// Gets/sets name of observer location
        /// </summary>
        public string LocationName
        {
            get
            {
                return ObserverLocation.LocationName;
            }
            set
            {
                ObserverLocation.LocationName = value;
                NotifyPropertyChanged(nameof(LocationName));
            }
        }

        #endregion

        /// <summary>
        /// Creates new instance of <see cref="LocationVM"/>
        /// </summary>
        public LocationVM(ISky sky, ISettings settings)
        {
            CrdsEquatorial eqSun = SolarEphem.Ecliptical(sky.Context.JulianDay).ToEquatorial(sky.Context.Epsilon);
            ObserverLocation = new CrdsGeographical(sky.Context.GeoLocation);
            SunHourAngle = Coordinates.HourAngle(sky.Context.SiderealTime, 0, eqSun.Alpha);
            SunDeclination = eqSun.Delta;
            IsNightMode = settings.Get<ColorSchema>("Schema") == ColorSchema.Red;

            OkCommand = new Command(Ok);
            CancelCommand = new Command(Close);
            EndSearchModeCommand = new Command(EndSearchMode);
            SelectLocationCommand = new Command(SelectLocation);

            string line;
            string filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data", "TimeZones.dat");
            using (StreamReader file = new StreamReader(filePath))
            {
                while ((line = file.ReadLine()) != null)
                {
                    // skip first and empty lines
                    if (line.StartsWith("CountryCode") ||
                        string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    string[] chunks = line.Split('\t');
                    TimeZones.Add(new TimeZoneItem() { TimeZoneId = chunks[1], UtcOffset = double.Parse(chunks[4], CultureInfo.InvariantCulture) });
                }
                file.Close();
            }
        }

        /// <summary>
        /// Action for <see cref="OkCommand"/>
        /// </summary>
        private void Ok()
        {
            Close(true);
        }

        /// <summary>
        /// Exits from the search mode
        /// </summary>
        private void EndSearchMode()
        {
            SearchString = null;            
        }

        /// <summary>
        /// Selects a location from the search results and ends the search mode
        /// </summary>
        private void SelectLocation()
        {
            _SavedLocation = new CrdsGeographical(ObserverLocation);
            EndSearchMode();
        }
    }

    /// <summary>
    /// Represents single search result item of geographical locations
    /// </summary>
    public class LocationSearchItem
    {
        /// <summary>
        /// Name of the location
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Country code of the location
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// Other names of the location, comma-separated and joined into single string
        /// </summary>
        public string Names { get; set; }

        /// <summary>
        /// Geographical location
        /// </summary>
        public CrdsGeographical Location { get; set; }
    }

    /// <summary>
    /// Represents time zone information
    /// </summary>
    public class TimeZoneItem
    {
        /// <summary>
        /// Unique time zone id, like "Europe/Moscow"
        /// </summary>
        public string TimeZoneId { get; set; }

        /// <summary>
        /// UTC offset of the zone, in hours
        /// </summary>
        public double UtcOffset { get; set; }

        /// <summary>
        /// Gets displayable name of the time zone
        /// </summary>
        public string Name
        {
            get
            {
                return $"UTC{(UtcOffset >= 0 ? "+" : "-")}{TimeSpan.FromHours(UtcOffset).ToString("hh\\:mm")} ({TimeZoneId.Replace('_', ' ')})";
            }
        }
    }
}
