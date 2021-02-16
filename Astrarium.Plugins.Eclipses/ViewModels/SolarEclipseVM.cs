using Astrarium.Algorithms;
using Astrarium.Plugins.Eclipses.Types;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace Astrarium.Plugins.Eclipses.ViewModels
{
    /// <summary>
    /// Implements business logic of Solar Eclopse window.
    /// </summary>
    public class SolarEclipseVM : EclipseVM
    {
        public bool IsCitiesListTableNotEmpty
        {
            get => GetValue<bool>(nameof(IsCitiesListTableNotEmpty));
            private set => SetValue(nameof(IsCitiesListTableNotEmpty), value);
        }

        /// <summary>
        /// Table of local contacts instants, displayed to the right of eclipse map
        /// </summary>
        public ObservableCollection<SolarEclipseLocalContactsTableItem> LocalContactsTable { get; private set; } = new ObservableCollection<SolarEclipseLocalContactsTableItem>();

        /// <summary>
        /// Table of local circumstances, displayed to the right of eclipse map
        /// </summary>
        public ObservableCollection<NameValueTableItem> LocalCircumstancesTable { get; private set; } = new ObservableCollection<NameValueTableItem>();

        /// <summary>
        /// Table of local circumstances for selected cities
        /// </summary>
        public ObservableCollection<CitiesListTableItem> CitiesListTable { get; private set; } = new ObservableCollection<CitiesListTableItem>();

        /// <summary>
        /// Local circumstance of the eclipse
        /// </summary>
        public SolarEclipseLocalCircumstances LocalCircumstances
        {
            get => GetValue<SolarEclipseLocalCircumstances>(nameof(LocalCircumstances));
            private set => SetValue(nameof(LocalCircumstances), value);
        }

        /// <summary>
        /// Title of Saros series table
        /// </summary>
        public string SarosSeriesTableTitle { get; private set; }

        /// <summary>
        /// Saros series table
        /// </summary>
        public ObservableCollection<SarosSeriesTableItem> SarosSeriesTable { get; private set; } = new ObservableCollection<SarosSeriesTableItem>();

        /// <summary>
        /// General eclipse info table
        /// </summary>
        public ObservableCollection<NameValueTableItem> EclipseGeneralDetails { get; private set; } = new ObservableCollection<NameValueTableItem>();

        /// <summary>
        /// Eclipse contacts info table
        /// </summary>
        public ObservableCollection<ContactsTableItem> EclipseContacts { get; private set; } = new ObservableCollection<ContactsTableItem>();

        /// <summary>
        /// Besselian elements table
        /// </summary>
        public ObservableCollection<BesselianElementsTableItem> BesselianElementsTable { get; private set; } = new ObservableCollection<BesselianElementsTableItem>();

        /// <summary>
        /// Table header for Besselian elements table
        /// </summary>
        public string BesselianElementsTableHeader
        {
            get => GetValue<string>(nameof(BesselianElementsTableHeader));
            private set => SetValue(nameof(BesselianElementsTableHeader), value);
        }

        /// <summary>
        /// Table footer for Besselian elements table
        /// </summary>
        public string BesselianElementsTableFooter
        {
            get => GetValue<string>(nameof(BesselianElementsTableFooter));
            private set => SetValue(nameof(BesselianElementsTableFooter), value);
        }

        
        public ICommand FindLocationsOnTotalPathCommand => new Command(FindLocationsOnTotalPath);
        public ICommand ClearLocationsTableCommand => new Command(ClearLocationsTable);
        public ICommand LoadLocationsFromFileCommand => new Command(LoadLocationsFromFile);
        public ICommand ExportLocationsTableCommand => new Command(ExportLocationsTable);

        private int currentSarosSeries;
        private SolarEclipseMap map;
        private PolynomialBesselianElements elements;
        private SolarEclipse eclipse;
        private static NumberFormatInfo nf;
       
        #region Map styles

        private readonly MarkerStyle riseSetMarkerStyle = new MarkerStyle(5, Brushes.Red, null, Brushes.Red, SystemFonts.DefaultFont, StringFormat.GenericDefault);
        private readonly MarkerStyle centralLineMarkerStyle = new MarkerStyle(5, Brushes.Black, null, Brushes.Black, SystemFonts.DefaultFont, StringFormat.GenericDefault);
        private readonly MarkerStyle maxPointMarkerStyle = new MarkerStyle(5, Brushes.Red, null, Brushes.Black, SystemFonts.DefaultFont, StringFormat.GenericDefault);
        private readonly TrackStyle riseSetTrackStyle = new TrackStyle(new Pen(Color.Red, 2));
        private readonly TrackStyle penumbraLimitTrackStyle = new TrackStyle(new Pen(Color.Orange, 2));
        private readonly TrackStyle umbraLimitTrackStyle = new TrackStyle(new Pen(Color.Gray, 2));
        private readonly TrackStyle centralLineTrackStyle = new TrackStyle(new Pen(Color.Black, 2));
        private readonly PolygonStyle umbraPolygonStyle = new PolygonStyle(new SolidBrush(Color.FromArgb(100, Color.Gray)));
        
        #endregion Map styles

        static SolarEclipseVM()
        {
            nf = new NumberFormatInfo();
            nf.NumberDecimalSeparator = ".";
            nf.NumberGroupSeparator = "\u2009";            
        }

        public SolarEclipseVM(IEclipsesCalculator eclipsesCalculator, IGeoLocationsManager locationsManager, CsvLocationsReader locationsReader, ISky sky, ISettings settings) : base(locationsManager, settings)
        {
            this.eclipsesCalculator = eclipsesCalculator;
            this.locationsReader = locationsReader;
            this.settings.PropertyChanged += Settings_PropertyChanged;
            
            SetMapColors();

            for (int i = 0; i < 5; i++) 
            {
                LocalContactsTable.Add(new SolarEclipseLocalContactsTableItem(null, null));
                LocalCircumstancesTable.Add(new NameValueTableItem(null, null));
            }

            JulianDay = sky.Context.JulianDay - LunarEphem.SINODIC_PERIOD;

            CalculateEclipse(next: true, saros: false);
        }

        public override void Dispose()
        {
            CitiesListTable.Clear();
            LocalContactsTable.Clear();
            SarosSeriesTable.Clear();
            EclipseGeneralDetails.Clear();
            LocalCircumstancesTable.Clear();
            EclipseContacts.Clear();
            BesselianElementsTable.Clear();
            Tracks.Clear();
            Markers.Clear();
            Polygons.Clear();
            locationsManager.Unload();
            base.Dispose();
            GC.Collect();
        }

        private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Schema")
            {
                IsDarkMode = settings.Get<ColorSchema>("Schema") == ColorSchema.Red; 
                TileImageAttributes = GetImageAttributes();
                SetMapColors();
                // TODO: need to recalculate eclipse
            }
        }

        protected override void AddToCitiesList(CrdsGeographical location)
        {
            var local = eclipsesCalculator.FindLocalCircumstancesForCities(elements, new[] { location }).First();
            CitiesListTable.Add(new CitiesListTableItem(local, eclipsesCalculator.GetLocalVisibilityString(eclipse, local)));
            IsCitiesListTableNotEmpty = true;
        }

        private void SetMapColors()
        {
            penumbraLimitTrackStyle.Pen = IsDarkMode ? new Pen(Color.DarkRed, 2) : new Pen(Color.Orange, 2);
            umbraLimitTrackStyle.Pen = IsDarkMode ? new Pen(Color.DarkRed, 2) : new Pen(Color.Gray, 2);
            umbraPolygonStyle.Brush = IsDarkMode ? new SolidBrush(Color.FromArgb(50, Color.DarkRed)) : new SolidBrush(Color.FromArgb(100, Color.Gray));
            MapThumbnailBackColor = IsDarkMode ? Color.FromArgb(0xFF, 0x33, 0, 0) : Color.FromArgb(0xFF, 0x33, 0x33, 0x33);
            MapThumbnailForeColor = IsDarkMode ? Color.FromArgb(0xFF, 0x59, 0, 0) : Color.FromArgb(0xFF, 0x59, 0x59, 0x59);
        }

        protected override async void CalculateEclipse(bool next, bool saros)
        {
            IsCalculating = true;

            eclipse =  eclipsesCalculator.GetNearestSolarEclipse(JulianDay, next, saros);
            JulianDay = eclipse.JulianDayMaximum;
            EclipseDate = Formatters.Date.Format(new Date(JulianDay, 0));
            elements = eclipsesCalculator.GetBesselianElements(JulianDay);
            string type = eclipse.EclipseType.ToString();
            string subtype = eclipse.IsNonCentral ? " non-central" : "";
            EclipseDescription = $"{type}{subtype} solar eclipse";
            PrevSarosEnabled = eclipsesCalculator.GetNearestSolarEclipse(JulianDay, next: false, saros: true).Saros == eclipse.Saros;
            NextSarosEnabled = eclipsesCalculator.GetNearestSolarEclipse(JulianDay, next: true, saros: true).Saros == eclipse.Saros;

            await Task.Run(() =>
            {
                map = SolarEclipses.EclipseMap(eclipse, elements);

                var tracks = new List<Track>();
                var polygons = new List<Polygon>();
                var markers = new List<Marker>();

                if (map.P1 != null)
                {
                    markers.Add(new Marker(ToGeo(map.P1), riseSetMarkerStyle, "P1"));
                }
                if (map.P2 != null)
                {
                    markers.Add(new Marker(ToGeo(map.P2), riseSetMarkerStyle, "P2"));
                }
                if (map.P3 != null)
                {
                    markers.Add(new Marker(ToGeo(map.P3), riseSetMarkerStyle, "P3"));
                }
                if (map.P4 != null)
                {
                    markers.Add(new Marker(ToGeo(map.P4), riseSetMarkerStyle, "P4"));
                }
                if (map.U1 != null)
                {
                    markers.Add(new Marker(ToGeo(map.U1), centralLineMarkerStyle, "U1"));
                }
                if (map.U2 != null)
                {
                    markers.Add(new Marker(ToGeo(map.U2), centralLineMarkerStyle, "U2"));
                }

                for (int i = 0; i < 2; i++)
                {
                    if (map.UmbraNorthernLimit[i].Any())
                    {
                        var track = new Track(umbraLimitTrackStyle);
                        track.AddRange(map.UmbraNorthernLimit[i].Select(p => ToGeo(p)));
                        tracks.Add(track);
                    }

                    if (map.UmbraSouthernLimit[i].Any())
                    {
                        var track = new Track(umbraLimitTrackStyle);
                        track.AddRange(map.UmbraSouthernLimit[i].Select(p => ToGeo(p)));
                        tracks.Add(track);
                    }
                }

                if (map.TotalPath.Any())
                {
                    var track = new Track(centralLineTrackStyle);
                    track.AddRange(map.TotalPath.Select(p => ToGeo(p)));
                    tracks.Add(track);
                }

                // central line is divided into 2 ones => draw shadow path as 2 polygons

                if ((map.UmbraNorthernLimit[0].Any() && !map.UmbraNorthernLimit[1].Any()) ||
                    (map.UmbraSouthernLimit[0].Any() && !map.UmbraSouthernLimit[1].Any()))
                {
                    var polygon = new Polygon(umbraPolygonStyle);
                    polygon.AddRange(map.UmbraNorthernLimit[0].Select(p => ToGeo(p)));
                    polygon.AddRange(map.UmbraNorthernLimit[1].Select(p => ToGeo(p)));
                    if (map.U2 != null) polygon.Add(ToGeo(map.U2));
                    polygon.AddRange((map.UmbraSouthernLimit[1] as IEnumerable<CrdsGeographical>).Reverse().Select(p => ToGeo(p)));
                    polygon.AddRange((map.UmbraSouthernLimit[0] as IEnumerable<CrdsGeographical>).Reverse().Select(p => ToGeo(p)));
                    if (map.U1 != null) polygon.Add(ToGeo(map.U1));
                    polygons.Add(polygon);
                }
                else
                {
                    for (int i = 0; i < 2; i++)
                    {
                        if (map.UmbraNorthernLimit[i].Any() && map.UmbraSouthernLimit[i].Any())
                        {
                            var polygon = new Polygon(umbraPolygonStyle);
                            polygon.AddRange(map.UmbraNorthernLimit[i].Select(p => ToGeo(p)));
                            polygon.AddRange((map.UmbraSouthernLimit[i] as IEnumerable<CrdsGeographical>).Reverse().Select(p => ToGeo(p)));
                            polygons.Add(polygon);
                        }
                    }
                }

                foreach (var curve in map.RiseSetCurve)
                {
                    if (curve.Any())
                    {
                        var track = new Track(riseSetTrackStyle);
                        track.AddRange(curve.Select(p => ToGeo(p)));
                        track.Add(track.First());
                        tracks.Add(track);
                    }
                }

                if (map.PenumbraNorthernLimit.Any())
                {
                    var track = new Track(penumbraLimitTrackStyle);
                    track.AddRange(map.PenumbraNorthernLimit.Select(p => ToGeo(p)));
                    tracks.Add(track);
                }

                if (map.PenumbraSouthernLimit.Any())
                {
                    var track = new Track(penumbraLimitTrackStyle);
                    track.AddRange(map.PenumbraSouthernLimit.Select(p => ToGeo(p)));
                    tracks.Add(track);
                }

                if (map.Max != null)
                {
                    markers.Add(new Marker(ToGeo(map.Max), maxPointMarkerStyle, "Max"));
                }

                // Local curcumstances at point of maximum
                var maxCirc = SolarEclipses.LocalCircumstances(elements, map.Max);

                // Brown lunation number
                var lunation = LunarEphem.Lunation(JulianDay);

                var eclipseGeneralDetails = new ObservableCollection<NameValueTableItem>()
                {
                    new NameValueTableItem("Type", $"{type}{subtype}"),
                    new NameValueTableItem("Saros", $"{eclipse.Saros}"),
                    new NameValueTableItem("Date", $"{EclipseDate}"),
                    new NameValueTableItem("Magnitude", $"{eclipse.Magnitude.ToString("N5", nf)}"),
                    new NameValueTableItem("Gamma", $"{eclipse.Gamma.ToString("N5", nf)}"),
                    new NameValueTableItem("Maximal Duration", $"{Format.Time.Format(maxCirc.TotalDuration) }"),
                    new NameValueTableItem("Brown Lunation Number", $"{lunation}"),
                    new NameValueTableItem("ΔT", $"{elements.DeltaT.ToString("N1", nf) } s")
                };
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    EclipseGeneralDetails = eclipseGeneralDetails;
                });

                var eclipseContacts = new ObservableCollection<ContactsTableItem>();

                if (!double.IsNaN(map.P1.JulianDay))
                {
                    eclipseContacts.Add(new ContactsTableItem("P1: First external contact", map.P1));
                }
                if (map.P2 != null)
                {
                    eclipseContacts.Add(new ContactsTableItem("P2: First internal contact", map.P2));
                }
                if (map.U1 != null && !double.IsNaN(map.U1.JulianDay))
                {
                    eclipseContacts.Add(new ContactsTableItem("U1: First umbra contact", map.U1));
                }
                eclipseContacts.Add(new ContactsTableItem("Max: Greatest Eclipse", map.Max));   
                if (map.U2 != null && !double.IsNaN(map.U2.JulianDay))
                {
                    eclipseContacts.Add(new ContactsTableItem("U2: Last umbra contact", map.U2));
                }
                if (map.P3 != null)
                {
                    eclipseContacts.Add(new ContactsTableItem("P3: Last internal contact", map.P3));
                }

                if (!double.IsNaN(map.P4.JulianDay))
                {
                    eclipseContacts.Add(new ContactsTableItem("P4: Last external contact", map.P4));
                }
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    EclipseContacts = eclipseContacts;
                });

                var besselianElementsTable = new ObservableCollection<BesselianElementsTableItem>();
                for (int i=0; i<4; i++)
                {
                    besselianElementsTable.Add(new BesselianElementsTableItem(i, elements));
                }
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    BesselianElementsTable = besselianElementsTable;
                });

                // Besselian elements table header
                var beTableHeader = new StringBuilder();
                beTableHeader.AppendLine($"Elements for t\u2080 = {Formatters.DateTime.Format(new Date(elements.JulianDay0))} TDT (JDE = { elements.JulianDay0.ToString("N6", nf)})");
                beTableHeader.AppendLine($"The Besselian elements are valid over the period t\u2080 - 6h ≤ t\u2080 ≤ t\u2080 + 6h");
                BesselianElementsTableHeader = beTableHeader.ToString();

                // Besselian elements table footer
                var beTableFooter = new StringBuilder();
                beTableFooter.AppendLine($"Tan ƒ1 = {elements.TanF1.ToString("N7", nf)}");
                beTableFooter.AppendLine($"Tan ƒ2 = {elements.TanF2.ToString("N7", nf)}");
                BesselianElementsTableFooter = beTableFooter.ToString();

                Markers = markers;
                Tracks = tracks;
                Polygons = polygons;                
                IsCalculating = false;

                AddLocationMarker();
                CalculateSarosSeries();
                CalculateLocalCircumstances(observerLocation);
                CalculateCitiesTable();
                NotifyPropertyChanged(
                    nameof(EclipseGeneralDetails),
                    nameof(EclipseContacts),
                    nameof(BesselianElementsTable)
                );
            });
        }

        protected override void CalculateLocalCircumstances(CrdsGeographical pos)
        {
            var local = SolarEclipses.LocalCircumstances(elements, pos);

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                var items = new List<SolarEclipseLocalContactsTableItem>();
                LocalContactsTable[0] = new SolarEclipseLocalContactsTableItem("C1: Beginning of partial phase", local.PartialBegin);
                LocalContactsTable[1] = new SolarEclipseLocalContactsTableItem("C2: Beginning of total phase", local.TotalBegin);
                LocalContactsTable[2] = new SolarEclipseLocalContactsTableItem("Max: Local maximum", local.Maximum);
                LocalContactsTable[3] = new SolarEclipseLocalContactsTableItem("C3: End of total phase", local.TotalEnd);
                LocalContactsTable[4] = new SolarEclipseLocalContactsTableItem("C4: End of partial phase", local.PartialEnd);

                LocalCircumstancesTable[0] = new NameValueTableItem("Maximal magnitude", local.MaxMagnitude > 0 ? Format.Mag.Format(local.MaxMagnitude) : "");
                LocalCircumstancesTable[1] = new NameValueTableItem("Moon/Sun diameter ratio", local.MoonToSunDiameterRatio > 0 ? Format.Ratio.Format(local.MoonToSunDiameterRatio) : "");
                LocalCircumstancesTable[2] = new NameValueTableItem("Partial phase duration", !double.IsNaN(local.PartialDuration) && local.PartialDuration > 0 ? Format.Time.Format(local.PartialDuration) : "");
                LocalCircumstancesTable[3] = new NameValueTableItem("Total phase duration", !double.IsNaN(local.TotalDuration) && local.TotalDuration > 0 ? Format.Time.Format(local.TotalDuration) : "");
                LocalCircumstancesTable[4] = new NameValueTableItem("Shadow path width", local.PathWidth > 0 ? Format.PathWidth.Format(local.PathWidth) : "");
            });

            ObserverLocationName = (IsMouseOverMap && !IsMapLocked) ? $"Mouse coordinates ({Format.Geo.Format(FromGeoPoint(MapMouse))})" : $"{observerLocation.LocationName} ({Format.Geo.Format(observerLocation)})";            
            LocalVisibilityDescription = eclipsesCalculator.GetLocalVisibilityString(eclipse, local);
            IsVisibleFromCurrentPlace = !local.IsInvisible;
            LocalCircumstances = local;
        }

        protected override async void CalculateSarosSeries()
        {
            await Task.Run(() =>
            {
                if (SelectedTabIndex != 2 || currentSarosSeries == eclipse.Saros) return;

                IsCalculating = true;

                double jd = JulianDay;
                List<SolarEclipse> eclipses = new List<SolarEclipse>();

                // add current eclipse
                eclipses.Add(eclipse);
                currentSarosSeries = eclipse.Saros;
                SarosSeriesTableTitle = $"List of eclipses of saros series {currentSarosSeries}";

                // add previous eclipses
                do
                {
                    var e = eclipsesCalculator.GetNearestSolarEclipse(jd, next: false, saros: true);
                    jd = e.JulianDayMaximum;
                    if (e.Saros == eclipse.Saros)
                    {
                        eclipses.Insert(0, e);
                    }
                    else
                    {
                        break;
                    }
                }
                while (true);

                jd = JulianDay;
                // add next eclipses
                do
                {
                    var e = eclipsesCalculator.GetNearestSolarEclipse(jd, next: true, saros: true);
                    jd = e.JulianDayMaximum;
                    if (e.Saros == eclipse.Saros)
                    {
                        eclipses.Add(e);
                    }
                    else
                    {
                        break;
                    }
                }
                while (true);

                var settingsLocation = settings.Get<CrdsGeographical>("ObserverLocation");
                ObservableCollection<SarosSeriesTableItem> sarosSeriesTable = new ObservableCollection<SarosSeriesTableItem>();

                int sarosMember = 0;
                foreach (var e in eclipses)
                {
                    string type = e.EclipseType.ToString();
                    string subtype = e.IsNonCentral ? " non-central" : "";
                    var pbe = eclipsesCalculator.GetBesselianElements(e.JulianDayMaximum);
                    var local = SolarEclipses.LocalCircumstances(pbe, settingsLocation);
                    sarosSeriesTable.Add(new SarosSeriesTableItem()
                    {
                        Member = $"{++sarosMember}",
                        JulianDay = e.JulianDayMaximum,
                        Date = Formatters.Date.Format(new Date(e.JulianDayMaximum, 0)),
                        Type = $"{type}{subtype}",
                        Gamma = e.Gamma.ToString("N5", nf),
                        Magnitude = e.Magnitude.ToString("N5", nf),
                        LocalVisibility = eclipsesCalculator.GetLocalVisibilityString(eclipse, local)
                    });
                }

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    SarosSeriesTable = sarosSeriesTable;
                });

                IsCalculating = false;
                NotifyPropertyChanged(
                    nameof(SarosSeriesTable), 
                    nameof(SarosSeriesTableTitle)
                );
            });
        }

        private async void CalculateCitiesTable()
        {
            if (CitiesListTable.Any())
            {
                var cities = CitiesListTable.Select(l => l.Location).ToArray();
                var locals = await Task.Run(() => eclipsesCalculator.FindLocalCircumstancesForCities(elements, cities));
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    CitiesListTable.Clear();
                    CitiesListTable = new ObservableCollection<CitiesListTableItem>(locals.Select(c => new CitiesListTableItem(c, eclipsesCalculator.GetLocalVisibilityString(eclipse, c))));
                });
                NotifyPropertyChanged(nameof(CitiesListTable));
            }
        }

        private void LoadLocationsFromFile()
        {
            FillCitiesTable(fromFile: true);
        }

        private void FindLocationsOnTotalPath()
        {
            if (ViewManager.ShowMessageBox("Warning", "Searching locations on eclipse path could take some time. Proceed?", System.Windows.MessageBoxButton.OKCancel) == System.Windows.MessageBoxResult.OK)
            {
                FillCitiesTable(fromFile: false);
            }
        }

        private void ClearLocationsTable()
        {
            if (ViewManager.ShowMessageBox("Warning", "Do you really want to clear the table?", System.Windows.MessageBoxButton.YesNo) == System.Windows.MessageBoxResult.Yes)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    CitiesListTable.Clear();
                    IsCitiesListTableNotEmpty = false;
                    NotifyPropertyChanged(nameof(CitiesListTable), nameof(IsCitiesListTableNotEmpty));
                });
            }
        }

        private async void FillCitiesTable(bool fromFile)
        {           
            var tokenSource = new CancellationTokenSource();
            var progress = new Progress<double>();
            ICollection<SolarEclipseLocalCircumstances> locals = null;

            try
            {
                if (fromFile)
                {
                    string file = ViewManager.ShowOpenFileDialog("Open cities list", "Comma-separated files (*.csv)|*.csv");
                    if (file != null)
                    {
                        ViewManager.ShowProgress("Please wait", "Calculating circumstances for locations...", tokenSource);
                        var cities = locationsReader.ReadFromFile(file);
                        locals = await Task.Run(() => eclipsesCalculator.FindLocalCircumstancesForCities(elements, cities, tokenSource.Token, null));
                    }
                }
                else
                {
                    ViewManager.ShowProgress("Please wait", "Searching cities on central line of the eclipse...", tokenSource, progress);
                    locals = await Task.Run(() => eclipsesCalculator.FindCitiesOnCentralLine(elements, map.TotalPath, tokenSource.Token, progress));
                }
            }
            catch (Exception ex)
            {
                tokenSource.Cancel();
                ViewManager.ShowMessageBox("$Error", ex.Message);
            }

            if (!tokenSource.IsCancellationRequested && locals != null)
            {
                tokenSource.Cancel();
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    CitiesListTable.Clear();
                    CitiesListTable = new ObservableCollection<CitiesListTableItem>(locals.Select(c => new CitiesListTableItem(c, eclipsesCalculator.GetLocalVisibilityString(eclipse, c))));
                    IsCitiesListTableNotEmpty = CitiesListTable.Any();
                });
            }

            NotifyPropertyChanged(nameof(CitiesListTable));
        }

        private void ExportLocationsTable()
        {
            var formats = new Dictionary<string, string>
            {
                ["Comma-separated files (with formatting) (*.csv)"] = "*.csv",
                ["Comma-separated files (raw data) (*.csv)"] = "*.csv",
            };
            string filter = string.Join("|", formats.Select(kv => $"{kv.Key}|{kv.Value}"));
            var file = ViewManager.ShowSaveFileDialog("Export", "CitiesList", ".csv", filter, out int selectedFilterIndex);
            if (file != null)
            {
                CitiesTableCsvWriter writer = null;
                string ext = Path.GetExtension(file);
                switch (ext)
                {
                    case ".csv":
                        writer = new CitiesTableCsvWriter(file, selectedFilterIndex == 2);
                        break;
                    default:
                        break;
                }

                writer?.Write(CitiesListTable);

                //ViewManager.ShowMessageBox("$SolarEclipseWindow.ExportDoneTitle", "$SolarEclipseWindow.ExportDoneText", MessageBoxButton.OK);
                var answer = ViewManager.ShowMessageBox("Информация", "Экспорт в файл успешно завершён. Окрыть файл?", System.Windows.MessageBoxButton.YesNo);
                if (answer == System.Windows.MessageBoxResult.Yes)
                {
                    System.Diagnostics.Process.Start(file);
                }
            }
        }

        protected override void ExportSarosSeriesTable()
        {
            var formats = new Dictionary<string, string>
            {
                ["Comma-separated files (with formatting) (*.csv)"] = "*.csv",
                ["Comma-separated files (raw data) (*.csv)"] = "*.csv",
            };
            string filter = string.Join("|", formats.Select(kv => $"{kv.Key}|{kv.Value}"));
            var file = ViewManager.ShowSaveFileDialog("Export", $"SarosSeries{currentSarosSeries}", ".csv", filter, out int selectedFilterIndex);
            if (file != null)
            {
                SarosSeriesTableCsvWriter writer = null;
                string ext = Path.GetExtension(file);
                switch (ext)
                {
                    case ".csv":
                        writer = new SarosSeriesTableCsvWriter(file, selectedFilterIndex == 2);
                        break;
                    default:
                        break;
                }

                writer?.Write(SarosSeriesTable);

                var answer = ViewManager.ShowMessageBox("Информация", "Экспорт в файл успешно завершён. Окрыть файл?", System.Windows.MessageBoxButton.YesNo);
                if (answer == System.Windows.MessageBoxResult.Yes)
                {
                    System.Diagnostics.Process.Start(file);
                }
            }
        }
    }
}
