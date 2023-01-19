using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace Astrarium.ViewModels
{
    public class LocationVM : ViewModelBase
    {
        /// <summary>
        /// Name of the setting to store map's tile server
        /// </summary>
        private const string TILE_SERVER_SETTING_NAME = "ObserverLocationTileServer";

        /// <summary>
        /// Application settings instance.
        /// </summary>
        private readonly ISettings settings;

        /// <summary>
        /// Geo locations manager instance.
        /// </summary>
        private readonly IGeoLocationsManager locationsManager;

        /// <summary>
        /// Formatter used for casting geo coordinates to string
        /// </summary>
        private readonly IEphemFormatter geoCoordinatesFormatter = new Formatters.GeoCoordinatesFormatter();

        /// <summary>
        /// Creates new instance of the ViewModel
        /// </summary>
        public LocationVM(IGeoLocationsManager locationsManager, ISettings settings)
        {
            this.settings = settings;
            this.settings.SettingValueChanged += OnSettingValueChanged;

            this.locationsManager = locationsManager;

            IsDarkMode = settings.Get<ColorSchema>("Schema") == ColorSchema.Red;
            MapZoomLevel = 7;
            CacheFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Astrarium", "MapsCache");

            SetMapColors();

            string userAgent = $"Astrarium/{Application.ProductVersion}";

            TileServers = MapControl.CreateTileServers(userAgent);
            OverlayTileServers = MapControl.CreateOverlayServers(userAgent);

            string tileServerName = settings.Get<string>(TILE_SERVER_SETTING_NAME);
            var tileServer = TileServers.FirstOrDefault(s => s.Name.Equals(tileServerName));
            TileServer = tileServer ?? TileServers.First();
        }

        #region Commands

        /// <summary>
        /// Executed when user clicks on OK button
        /// </summary>
        public ICommand OkCommand => new Command(Ok);

        /// <summary>
        /// Executed when user cancels the view
        /// </summary>
        public ICommand CancelCommand => new Command(Close);

        /// <summary>
        /// Executed when user presses Enter key in search text box
        /// </summary>
        public ICommand EnterCommand => new Command(Enter);

        /// <summary>
        /// Handles keyboard up navigation on search results
        /// </summary>
        public ICommand PrevSearchResultCommand => new Command(PrevSearchResult);

        /// <summary>
        /// Handles keyboard down navigation on search results
        /// </summary>
        public ICommand NextSearchResultCommand => new Command(NextSearchResult);

        /// <summary>
        /// Executed on drawing position marker
        /// </summary>
        public ICommand OnDrawMarkerCommand => new Command<DrawMarkerEventArgs>(OnDrawMarker);

        /// <summary>
        /// Executed when user double clicks on the map
        /// </summary>
        public ICommand OnMouseDoubleClickCommand => new Command(OnMouseDoubleClick);

        /// <summary>
        /// Executed when user right clicks on the map
        /// </summary>
        public ICommand OnMouseRightClickCommand => new Command(OnMouseRightClick);

        /// <summary>
        /// Executed when search mode is off
        /// </summary>
        public ICommand EndSearchModeCommand => new Command(EndSearchMode);

        /// <summary>
        /// Executed when user selects a location from the search results list
        /// </summary>
        public ICommand SelectLocationCommand => new Command(SelectLocation);

        /// <summary>
        /// Executed when user selects current mouse position from context menu
        /// </summary>
        public ICommand SelectCurrentMousePositionCommand => new Command(SelectCurrentMousePosition);

        /// <summary>
        /// Executed when user selects "nearest location" position from context menu
        /// </summary>
        public ICommand SelectNearestLocationCommand => new Command(SelectNearestLocation);

        #endregion Commands

        /// <summary>
        /// Location nearest to the current mouse position on the map
        /// </summary>
        public CrdsGeographical NearestLocation
        {
            get => GetValue<CrdsGeographical>(nameof(NearestLocation));
            protected set => SetValue(nameof(NearestLocation), value);
        }

        /// <summary>
        /// Formatted string representing current mouse coordinates on the map
        /// </summary>
        public string MapMouseString
        {
            get => GetValue<string>(nameof(MapMouseString));
            protected set => SetValue(nameof(MapMouseString), value);
        }

        /// <summary>
        /// Current geographical coordinates of the map center
        /// </summary>
        public GeoPoint MapCenter
        {
            get => GetValue(nameof(MapCenter), new GeoPoint());
            set => SetValue(nameof(MapCenter), value);
        }

        /// <summary>
        /// Collection of markers (points) on the map
        /// </summary>
        public ObservableCollection<Marker> Markers
        {
            get => GetValue(nameof(Markers), new ObservableCollection<Marker>());
            protected set => SetValue(nameof(Markers), value);
        }

        /// <summary>
        /// Directory to store maps cache
        /// </summary>
        public string CacheFolder
        {
            get => GetValue<string>(nameof(CacheFolder));
            protected set => SetValue(nameof(CacheFolder), value);
        }

        /// <summary>
        /// Current zoom level of local view chart
        /// </summary>
        public float MapZoomLevel
        {
            get => GetValue<float>(nameof(MapZoomLevel));
            set => SetValue(nameof(MapZoomLevel), value);
        }

        /// <summary>
        /// Image attributes used to modify the map tiles
        /// </summary>
        public ImageAttributes TileImageAttributes
        {
            get => GetValue<ImageAttributes>(nameof(TileImageAttributes));
            set => SetValue(nameof(TileImageAttributes), value);
        }

        /// <summary>
        /// Tile server of the map
        /// </summary>
        public ITileServer TileServer
        {
            get => GetValue<ITileServer>(nameof(TileServer));
            set
            {
                SetValue(nameof(TileServer), value);
                TileImageAttributes = GetImageAttributes();
                if (settings.Get<string>(TILE_SERVER_SETTING_NAME) != value.Name)
                {
                    settings.SetAndSave(TILE_SERVER_SETTING_NAME, value.Name);
                }
            }
        }

        /// <summary>
        /// Overlay tile server of the map
        /// </summary>
        public ITileServer OverlayTileServer
        {
            get => GetValue<ITileServer>(nameof(OverlayTileServer));
            set
            {
                SetValue(nameof(OverlayTileServer), value);
            }
        }

        /// <summary>
        /// Collection of map tile servers to switch between them
        /// </summary>
        public ICollection<ITileServer> TileServers
        {
            get => GetValue<ICollection<ITileServer>>(nameof(TileServers));
            protected set => SetValue(nameof(TileServers), value);
        }

        /// <summary>
        /// Collection of overlay tile servers to switch between them
        /// </summary>
        public ICollection<ITileServer> OverlayTileServers
        {
            get => GetValue<ICollection<ITileServer>>(nameof(OverlayTileServers));
            protected set => SetValue(nameof(OverlayTileServers), value);
        }

        /// <summary>
        /// Current geographical coordinates of the mouse over the map
        /// </summary>
        public GeoPoint MapMouse
        {
            get => GetValue<GeoPoint>(nameof(MapMouse));
            set
            {
                SetValue(nameof(MapMouse), value);
                MapMouseString = geoCoordinatesFormatter.Format(new CrdsGeographical(-value.Longitude, value.Latitude));
            }
        }

        /// <summary>
        /// Background color for map thumbnails
        /// </summary>
        public Color MapThumbnailBackColor
        {
            get => GetValue<Color>(nameof(MapThumbnailBackColor));
            protected set => SetValue(nameof(MapThumbnailBackColor), value);
        }

        /// <summary>
        /// Foreground color for map thumbnails text
        /// </summary>
        public Color MapThumbnailForeColor
        {
            get => GetValue<Color>(nameof(MapThumbnailForeColor));
            protected set => SetValue(nameof(MapThumbnailForeColor), value);
        }

        /// <summary>
        /// Flag indicating dark mode is used
        /// </summary>
        public bool IsDarkMode
        {
            get => GetValue<bool>(nameof(IsDarkMode));
            protected set => SetValue(nameof(IsDarkMode), value);
        }

        /// <summary>
        /// Brush to draw location name on the map
        /// </summary>
        private Brush MapLocationNameBrush { get; set; } = Brushes.White;

        /// <summary>
        /// Sets observer location
        /// </summary>
        public void SetLocation(CrdsGeographical location)
        {
            ObserverLocation = location;
            SelectLocation();
        }

        /// <summary>
        /// Current observer location chosen in the view. 
        /// Executing the <see cref="OkCommand"/> will propagate the value as dialog result.
        /// </summary>
        public CrdsGeographical ObserverLocation
        {
            get => GetValue<CrdsGeographical>(nameof(ObserverLocation));
            set
            {
                SetValue(nameof(ObserverLocation), value);

                var geoPoint = new GeoPoint(-(float)value.Longitude, (float)value.Latitude);
                Markers.Clear();
                Markers.Add(new Marker(geoPoint, value.Name));

                NotifyPropertyChanged(
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
                    nameof(UtcOffset),
                    nameof(Elevation),
                    nameof(LocationName)
                );
            }
        }

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
                ObserverLocation = new CrdsGeographical(ObserverLocation.Longitude, latitude.ToDecimalAngle(), ObserverLocation.UtcOffset, ObserverLocation.Elevation, ObserverLocation.Name);
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
                ObserverLocation = new CrdsGeographical(ObserverLocation.Longitude, latitude.ToDecimalAngle(), ObserverLocation.UtcOffset, ObserverLocation.Elevation, ObserverLocation.Name);
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
                ObserverLocation = new CrdsGeographical(ObserverLocation.Longitude, latitude.ToDecimalAngle(), ObserverLocation.UtcOffset, ObserverLocation.Elevation, ObserverLocation.Name);
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
                    ObserverLocation = new CrdsGeographical(ObserverLocation.Longitude, -ObserverLocation.Latitude, ObserverLocation.UtcOffset, ObserverLocation.Elevation, ObserverLocation.Name);
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
                    ObserverLocation = new CrdsGeographical(ObserverLocation.Longitude, -ObserverLocation.Latitude, ObserverLocation.UtcOffset, ObserverLocation.Elevation, ObserverLocation.Name);
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
                ObserverLocation = new CrdsGeographical(longitude.ToDecimalAngle(), ObserverLocation.Latitude, ObserverLocation.UtcOffset, ObserverLocation.Elevation, ObserverLocation.Name);
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
                ObserverLocation = new CrdsGeographical(longitude.ToDecimalAngle(), ObserverLocation.Latitude, ObserverLocation.UtcOffset, ObserverLocation.Elevation, ObserverLocation.Name);
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
                ObserverLocation = new CrdsGeographical(longitude.ToDecimalAngle(), ObserverLocation.Latitude, ObserverLocation.UtcOffset, ObserverLocation.Elevation, ObserverLocation.Name);
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
                    ObserverLocation = new CrdsGeographical(-ObserverLocation.Longitude, ObserverLocation.Latitude, ObserverLocation.UtcOffset, ObserverLocation.Elevation, ObserverLocation.Name);
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
                    ObserverLocation = new CrdsGeographical(-ObserverLocation.Longitude, ObserverLocation.Latitude, ObserverLocation.UtcOffset, ObserverLocation.Elevation, ObserverLocation.Name);
                }
            }
        }

        #endregion Longitude properties

        #region TimeZone properties

        /// <summary>
        /// Gets timezones list
        /// </summary>
        public ICollection<TimeSpan> TimeZones
        {
            get => TimeZoneInfo.GetSystemTimeZones().Select(x => x.BaseUtcOffset).OrderBy(x => x).Distinct().ToArray();
        }

        /// <summary>
        /// Offset from UTC
        /// </summary>
        public TimeSpan UtcOffset
        {
            get => TimeSpan.FromHours(ObserverLocation.UtcOffset);
            set
            {
                ObserverLocation.UtcOffset = value.TotalHours;
                NotifyPropertyChanged(nameof(UtcOffset));
            }
        }

        /// <summary>
        /// Elevation above sea level
        /// </summary>
        public decimal Elevation
        {
            get => (decimal)ObserverLocation.Elevation;
            set
            {
                ObserverLocation.Elevation = (double)value;
                NotifyPropertyChanged(nameof(Elevation));
            }
        }

        /// <summary>
        /// Gets/sets name of observer location
        /// </summary>
        public string LocationName
        {
            get => ObserverLocation.Name;
            set
            {
                ObserverLocation.Name = value;
                NotifyPropertyChanged(nameof(LocationName));
            }
        }

        #endregion

        /// <summary>
        /// Gets value indicating the search mode is on
        /// </summary>
        public bool SearchMode => !string.IsNullOrWhiteSpace(SearchString);

        /// <summary>
        /// Search string. Triggers searching process.
        /// </summary>
        public string SearchString
        {
            get => GetValue<string>(nameof(SearchString));
            set
            {
                SetValue(nameof(SearchString), value);
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
                else
                {
                    ObserverLocation = new CrdsGeographical(_SavedLocation);
                }

                NotifyPropertyChanged(nameof(NoResults));
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
            SelectedItem = SearchResults.Any() ? SearchResults[0] : null;
            NotifyPropertyChanged(nameof(NoResults));
        }

        /// <summary>
        /// Gets value indicating there are no search results
        /// </summary>
        public bool NoResults => SearchMode && !SearchResults.Any();

        /// <summary>
        /// Collection of found items.
        /// </summary>
        public ObservableCollection<CrdsGeographical> SearchResults { get; private set; } = new ObservableCollection<CrdsGeographical>();

        /// <summary>
        /// Backing field for <see cref="SelectedItem"/>
        /// </summary>
        private CrdsGeographical _SelectedItem;

        /// <summary>
        /// Previously saved location, needed in case when user cancels the search mode
        /// </summary>
        private CrdsGeographical _SavedLocation;

        /// <summary>
        /// Gets or sets location item currently selected in the search list
        /// </summary>
        public CrdsGeographical SelectedItem
        {
            get => _SelectedItem;
            set
            {
                _SelectedItem = value;
                if (_SelectedItem != null)
                {
                    ObserverLocation = _SelectedItem;
                }
                NotifyPropertyChanged(
                    nameof(SelectedItem),
                    nameof(UtcOffset),
                    nameof(Elevation),
                    nameof(LocationName));
            }
        }

        /// <summary>
        /// Searches for geographical locations synchronously.
        /// </summary>
        /// <param name="searchString">String to search</param>
        /// <returns>List of location items matching the specified search string</returns>
        private ICollection<CrdsGeographical> Search(string searchString)
        {
            if (searchString.Length == 0)
            {
                return new CrdsGeographical[0];
            }
            else
            {
                return locationsManager.Search(searchString, 20);
            }
        }

        /// <summary>
        /// Exits from the search mode
        /// </summary>
        private void EndSearchMode()
        {
            SearchString = null;
            ObserverLocation = _SavedLocation;
        }

        /// <summary>
        /// Selects a location from the search results and ends the search mode
        /// </summary>
        private void SelectLocation()
        {
            MapCenter = new GeoPoint(-(float)ObserverLocation.Longitude, (float)ObserverLocation.Latitude);
            _SavedLocation = new CrdsGeographical(ObserverLocation);
            EndSearchMode();
        }

        /// <summary>
        /// Handler for <see cref="SelectCurrentMousePositionCommand"/>
        /// </summary>
        private void SelectCurrentMousePosition()
        {
            OnMouseDoubleClick();
        }

        /// <summary>
        /// Handler for <see cref="SelectNearestLocationCommand"/>
        /// </summary>
        private void SelectNearestLocation()
        {
            ObserverLocation = NearestLocation;
        }

        /// <summary>
        /// Handler for double click on the map
        /// </summary>
        private void OnMouseDoubleClick()
        {
            var mouse = new CrdsGeographical(-MapMouse.Longitude, MapMouse.Latitude);

            CrdsGeographical nearestKnown = locationsManager.Search(mouse, 100).OrderBy(x => x.DistanceTo(mouse)).FirstOrDefault();

            string name = Text.Get("LocationWindow.UnnamedLocation");
            double utcOffset = ObserverLocation.UtcOffset;
            if (nearestKnown != null)
            {
                int dist = (int)nearestKnown.DistanceTo(mouse);
                name = $"{nearestKnown.Name} ({dist} km)";
                utcOffset = nearestKnown.UtcOffset;
            }

            // TODO: use another constructor
            ObserverLocation = new CrdsGeographical(-MapMouse.Longitude, MapMouse.Latitude) { Name = name, UtcOffset = utcOffset };
        }

        /// <summary>
        /// Handler for right click on the map
        /// </summary>
        private void OnMouseRightClick()
        {
            var mouse = new CrdsGeographical(-MapMouse.Longitude, MapMouse.Latitude);
            NearestLocation = locationsManager.Search(mouse, 100).OrderBy(x => x.DistanceTo(mouse)).FirstOrDefault();
        }

        /// <summary>
        /// Custom draw observer location point on the map
        /// </summary>
        /// <param name="e"></param>
        private void OnDrawMarker(DrawMarkerEventArgs e)
        {
            e.Handled = true;

            // vertical line
            if (e.Point.X >= 0 && e.Point.X <= e.Graphics.ClipBounds.Size.Width)
            {
                e.Graphics.DrawLine(Pens.Red, new PointF(e.Point.X, 0), new PointF(e.Point.X, e.Graphics.ClipBounds.Size.Height));
            }

            // horizontal line
            if (e.Point.Y >= 0 && e.Point.Y <= e.Graphics.ClipBounds.Size.Height)
            {
                e.Graphics.DrawLine(Pens.Red, new PointF(0, e.Point.Y), new PointF(e.Graphics.ClipBounds.Size.Width, e.Point.Y));
            }

            if (e.Graphics.IsVisible(e.Point))
            {
                Brush brush = new SolidBrush(Color.FromArgb(150, Color.Black));
                var size = e.Graphics.MeasureString(e.Marker.Label, SystemFonts.DefaultFont);
                e.Graphics.FillRectangle(brush, new RectangleF(e.Point.X, e.Point.Y, size.Width + 2, size.Height + 2));

                e.Graphics.DrawEllipse(Pens.Red, e.Point.X - 7.5f, e.Point.Y - 7.5f, 15f, 15f);
                e.Graphics.DrawString(e.Marker.Label, SystemFonts.DefaultFont, MapLocationNameBrush, e.Point.X + 1, e.Point.Y + 1);
            }
        }

        /// <summary>
        /// Fired when setting value is changed
        /// </summary>
        /// <param name="setting">Setting name</param>
        /// <param name="value">Setting value</param>
        private void OnSettingValueChanged(string setting, object value)
        {
            if (setting == "Schema")
            {
                IsDarkMode = settings.Get<ColorSchema>("Schema") == ColorSchema.Red;
                TileImageAttributes = GetImageAttributes();
                SetMapColors();
            }
        }

        /// <summary>
        /// Sets map colors depending on color schema
        /// </summary>
        private void SetMapColors()
        {
            MapThumbnailBackColor = IsDarkMode ? Color.FromArgb(0xFF, 0x33, 0, 0) : Color.FromArgb(0xFF, 0x33, 0x33, 0x33);
            MapThumbnailForeColor = IsDarkMode ? Color.FromArgb(0xFF, 0x59, 0, 0) : Color.FromArgb(0xFF, 0x59, 0x59, 0x59);
            MapLocationNameBrush = IsDarkMode ? Brushes.Red : Brushes.White;
        }

        /// <summary>
        /// Gets image attributes for color tinting depending on color schema
        /// </summary>
        /// <returns></returns>
        private ImageAttributes GetImageAttributes()
        {
            // make image "red"
            if (settings.Get<ColorSchema>("Schema") == ColorSchema.Red)
            {
                float[][] matrix = {
                    new float[] {0.3f, 0, 0, 0, 0},
                    new float[] {0.3f, 0, 0, 0, 0},
                    new float[] {0.3f, 0, 0, 0, 0},
                    new float[] {0, 0, 0, 1, 0},
                    new float[] {0, 0, 0, 0, 0}
                };
                var colorMatrix = new ColorMatrix(matrix);
                var attr = new ImageAttributes();
                attr.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                return attr;
            }

            // make image lighten
            if (TileServer is OfflineTileServer)
            {
                float gamma = 1;
                float brightness = 1.2f;
                float contrast = 1;
                float alpha = 1;

                float adjustedBrightness = brightness - 1.0f;

                float[][] matrix ={
                    new float[] {contrast, 0, 0, 0, 0}, // scale red
                    new float[] {0, contrast, 0, 0, 0}, // scale green
                    new float[] {0, 0, contrast, 0, 0}, // scale blue
                    new float[] {0, 0, 0, alpha, 0},
                    new float[] {adjustedBrightness, adjustedBrightness, adjustedBrightness, 0, 1}};

                var attr = new ImageAttributes();
                attr.ClearColorMatrix();
                attr.SetColorMatrix(new ColorMatrix(matrix), ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                attr.SetGamma(gamma, ColorAdjustType.Bitmap);

                return attr;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Handler for <see cref="EnterCommand "/>
        /// </summary>
        private void Enter()
        {
            if (SearchResults.Any())
            {
                SelectLocation();
            }
        }

        /// <summary>
        /// Handler for <see cref="PrevSearchResultCommand "/>
        /// </summary>
        private void PrevSearchResult()
        {
            if (SearchResults.Any())
            {
                int index = SearchResults.IndexOf(SelectedItem);
                if (index > 0)
                {
                    SelectedItem = SearchResults.ElementAt(index - 1);
                }
            }
        }

        /// <summary>
        /// Handler for <see cref="NextSearchResultCommand "/>
        /// </summary>
        private void NextSearchResult()
        {
            if (SearchResults.Any())
            {
                int index = SearchResults.IndexOf(SelectedItem);
                if (index < SearchResults.Count - 1)
                {
                    SelectedItem = SearchResults.ElementAt(index + 1);
                }
            }
        }

        /// <summary>
        /// Raised when user presses "Apply" / "Select" button
        /// </summary>
        private void Ok()
        {
            Close(true);
        }

        /// <summary>
        /// Disposes allocated resources
        /// </summary>
        public override void Dispose()
        {
            settings.SettingValueChanged -= OnSettingValueChanged;
            base.Dispose();
            Task.Run(() => GC.Collect());
        }
    }
}
