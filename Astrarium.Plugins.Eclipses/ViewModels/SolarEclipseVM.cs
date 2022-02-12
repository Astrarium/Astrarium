using Astrarium.Algorithms;
using Astrarium.Plugins.Eclipses.ImportExport;
using Astrarium.Plugins.Eclipses.Types;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
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
        /// <summary>
        /// Table of local contacts instants, displayed to the right of eclipse map
        /// </summary>
        public ObservableCollection<SolarEclipseLocalContactsTableItem> LocalContactsTable
        {
            get => GetValue(nameof(LocalContactsTable), new ObservableCollection<SolarEclipseLocalContactsTableItem>(Enumerable.Repeat(new SolarEclipseLocalContactsTableItem(null, null), 5)));
            private set => SetValue(nameof(LocalContactsTable), value);
        }

        /// <summary>
        /// Table of local circumstances, displayed to the right of eclipse map
        /// </summary>
        public ObservableCollection<NameValueTableItem> LocalCircumstancesTable
        {
            get => GetValue(nameof(LocalCircumstancesTable), new ObservableCollection<NameValueTableItem>(Enumerable.Repeat(new NameValueTableItem(null, null), 5)));
            private set => SetValue(nameof(LocalCircumstancesTable), value);
        }

        /// <summary>
        /// Table of local circumstances for selected cities
        /// </summary>
        public ObservableCollection<SolarEclipseCitiesListTableItem> CitiesListTable
        {
            get => GetValue(nameof(CitiesListTable), new ObservableCollection<SolarEclipseCitiesListTableItem>());
            private set => SetValue(nameof(CitiesListTable), value);
        }

        /// <summary>
        /// Local circumstance of the eclipse
        /// </summary>
        public SolarEclipseLocalCircumstances LocalCircumstances
        {
            get => GetValue<SolarEclipseLocalCircumstances>(nameof(LocalCircumstances));
            private set => SetValue(nameof(LocalCircumstances), value);
        }

        /// <summary>
        /// Besselian elements table
        /// </summary>
        public ObservableCollection<BesselianElementsTableItem> BesselianElementsTable
        {
            get => GetValue(nameof(BesselianElementsTable), new ObservableCollection<BesselianElementsTableItem>());
            private set => SetValue(nameof(BesselianElementsTable), value);
        }

        /// <summary>
        /// Flag indicating searching cities on the total eclipse path is enabled
        /// </summary>
        public bool IsFindLocationsOnPathEnabled
        {
            get => GetValue<bool>(nameof(IsFindLocationsOnPathEnabled));
            private set => SetValue(nameof(IsFindLocationsOnPathEnabled), value);
        }

        public ICommand FindLocationsOnTotalPathCommand => new Command(FindLocationsOnTotalPath);
        public ICommand ClearLocationsTableCommand => new Command(ClearLocationsTable);
        public ICommand ExportLocationsTableCommand => new Command(ExportLocationsTable);
        public ICommand LoadLocationsFromFileCommand => new Command(LoadLocationsFromFile);

        private SolarEclipseMap map;
        private PolynomialBesselianElements elements;
        private SolarEclipse eclipse;

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

        public SolarEclipseVM(
            ISky sky,
            IEclipsesCalculator eclipsesCalculator,
            IGeoLocationsManager locationsManager,
            ISettings settings)
            : base(sky, eclipsesCalculator, locationsManager, settings) { }

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

        protected override void AddToCitiesList(CrdsGeographical location)
        {
            var local = eclipsesCalculator.FindLocalCircumstancesForCities(elements, new[] { location }).First();
            CitiesListTable.Add(new SolarEclipseCitiesListTableItem(local, eclipsesCalculator.GetLocalVisibilityString(eclipse, local)));
            AddCitiesListMarker(location);
            IsCitiesListTableNotEmpty = true;
        }

        protected override void SetMapColors()
        {
            base.SetMapColors();
            penumbraLimitTrackStyle.Pen = IsDarkMode ? new Pen(Color.DarkRed, 2) : new Pen(Color.Orange, 2);
            umbraLimitTrackStyle.Pen = IsDarkMode ? new Pen(Color.DarkRed, 2) : new Pen(Color.Gray, 2);
            umbraPolygonStyle.Brush = IsDarkMode ? new SolidBrush(Color.FromArgb(50, Color.DarkRed)) : new SolidBrush(Color.FromArgb(100, Color.Gray));            
        }

        protected override async void CalculateEclipse(bool next, bool saros)
        {
            IsCalculating = true;

            eclipse = eclipsesCalculator.GetNearestSolarEclipse(meeusLunationNumber, next, saros);
            julianDay = eclipse.JulianDayMaximum;
            meeusLunationNumber = eclipse.MeeusLunationNumber;            
            elements = eclipsesCalculator.GetBesselianElements(eclipse.JulianDayMaximum);
            string type = Text.Get($"SolarEclipse.Type.{eclipse.EclipseType}");
            string subtype = eclipse.IsNonCentral ? $" {Text.Get("SolarEclipse.Type.NoCentral")}" : "";            
            EclipseDescription = Text.Get("SolarEclipseView.EclipseDescription", ("type", type), ("subtype", subtype));
            EclipseDate = Formatters.Date.Format(new Date(eclipse.JulianDayMaximum, 0));
            PrevSarosEnabled = eclipsesCalculator.GetNearestSolarEclipse(meeusLunationNumber, next: false, saros: true).Saros == eclipse.Saros;
            NextSarosEnabled = eclipsesCalculator.GetNearestSolarEclipse(meeusLunationNumber, next: true, saros: true).Saros == eclipse.Saros;
            IsFindLocationsOnPathEnabled = eclipse.EclipseType != SolarEclipseType.Partial && !eclipse.IsNonCentral;

            await Task.Run(() =>
            {
                map = SolarEclipses.EclipseMap(eclipse, elements);

                Polygons.Clear();
                Tracks.Clear();
                Markers.Clear();

                if (map.P1 != null)
                {
                    Markers.Add(new Marker(ToGeo(map.P1), riseSetMarkerStyle, "P1"));
                }
                if (map.P2 != null)
                {
                    Markers.Add(new Marker(ToGeo(map.P2), riseSetMarkerStyle, "P2"));
                }
                if (map.P3 != null)
                {
                    Markers.Add(new Marker(ToGeo(map.P3), riseSetMarkerStyle, "P3"));
                }
                if (map.P4 != null)
                {
                    Markers.Add(new Marker(ToGeo(map.P4), riseSetMarkerStyle, "P4"));
                }
                if (map.U1 != null)
                {
                    Markers.Add(new Marker(ToGeo(map.U1), centralLineMarkerStyle, "U1"));
                }
                if (map.U2 != null)
                {
                    Markers.Add(new Marker(ToGeo(map.U2), centralLineMarkerStyle, "U2"));
                }

                for (int i = 0; i < 2; i++)
                {
                    if (map.UmbraNorthernLimit[i].Any())
                    {
                        var track = new Track(umbraLimitTrackStyle);
                        track.AddRange(map.UmbraNorthernLimit[i].Select(p => ToGeo(p)));
                        Tracks.Add(track);
                    }

                    if (map.UmbraSouthernLimit[i].Any())
                    {
                        var track = new Track(umbraLimitTrackStyle);
                        track.AddRange(map.UmbraSouthernLimit[i].Select(p => ToGeo(p)));
                        Tracks.Add(track);
                    }
                }

                if (map.TotalPath.Any())
                {
                    var track = new Track(centralLineTrackStyle);
                    track.AddRange(map.TotalPath.Select(p => ToGeo(p)));
                    Tracks.Add(track);
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
                    Polygons.Add(polygon);
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
                            Polygons.Add(polygon);
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
                        Tracks.Add(track);
                    }
                }

                if (map.PenumbraNorthernLimit.Any())
                {
                    var track = new Track(penumbraLimitTrackStyle);
                    track.AddRange(map.PenumbraNorthernLimit.Select(p => ToGeo(p)));
                    Tracks.Add(track);
                }

                if (map.PenumbraSouthernLimit.Any())
                {
                    var track = new Track(penumbraLimitTrackStyle);
                    track.AddRange(map.PenumbraSouthernLimit.Select(p => ToGeo(p)));
                    Tracks.Add(track);
                }

                if (map.Max != null)
                {
                    Markers.Add(new Marker(ToGeo(map.Max), maxPointMarkerStyle, "Max"));
                }

                // Local curcumstances at point of maximum
                var maxCirc = SolarEclipses.LocalCircumstances(elements, map.Max);

                // Brown lunation number
                var lunation = LunarEphem.Lunation(eclipse.JulianDayMaximum);

                var eclipseGeneralDetails = new ObservableCollection<NameValueTableItem>()
                {
                    new NameValueTableItem(Text.Get("SolarEclipseView.EclipseType"), $"{type}{subtype}"),
                    new NameValueTableItem(Text.Get("SolarEclipseView.EclipseSaros"), $"{eclipse.Saros}"),
                    new NameValueTableItem(Text.Get("SolarEclipseView.EclipseDate"), $"{EclipseDate}"),
                    new NameValueTableItem(Text.Get("SolarEclipseView.EclipseMagnitude"), $"{eclipse.Magnitude.ToString("N5", nf)}"),
                    new NameValueTableItem(Text.Get("SolarEclipseView.EclipseGamma"), $"{eclipse.Gamma.ToString("N5", nf)}"),
                    new NameValueTableItem(Text.Get("SolarEclipseView.EclipseMaxDuration"), $"{Format.Time.Format(maxCirc.TotalDuration)}"),
                    new NameValueTableItem(Text.Get("SolarEclipseView.BrownLunationNumber"), $"{lunation}"),
                    new NameValueTableItem("ΔT", $"{elements.DeltaT.ToString("N1", nf) } s")
                };
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    EclipseGeneralDetails = eclipseGeneralDetails;
                });

                var eclipseContacts = new ObservableCollection<ContactsTableItem>();

                if (!double.IsNaN(map.P1.JulianDay))
                {
                    eclipseContacts.Add(new ContactsTableItem(Text.Get("SolarEclipseView.EclipseContacts.P1"), map.P1.JulianDay, map.P1));
                }
                if (map.P2 != null)
                {
                    eclipseContacts.Add(new ContactsTableItem(Text.Get("SolarEclipseView.EclipseContacts.P2"), map.P2.JulianDay, map.P2));
                }
                if (map.U1 != null && !double.IsNaN(map.U1.JulianDay))
                {
                    eclipseContacts.Add(new ContactsTableItem(Text.Get("SolarEclipseView.EclipseContacts.U1"), map.U1.JulianDay, map.U2));
                }
                eclipseContacts.Add(new ContactsTableItem(Text.Get("SolarEclipseView.EclipseContacts.Max"), map.Max.JulianDay, map.Max));
                if (map.U2 != null && !double.IsNaN(map.U2.JulianDay))
                {
                    eclipseContacts.Add(new ContactsTableItem(Text.Get("SolarEclipseView.EclipseContacts.U2"), map.U2.JulianDay, map.U2));
                }
                if (map.P3 != null)
                {
                    eclipseContacts.Add(new ContactsTableItem(Text.Get("SolarEclipseView.EclipseContacts.P3"), map.P3.JulianDay, map.P3));
                }

                if (!double.IsNaN(map.P4.JulianDay))
                {
                    eclipseContacts.Add(new ContactsTableItem(Text.Get("SolarEclipseView.EclipseContacts.P4"), map.P4.JulianDay, map.P4));
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
                beTableHeader.AppendLine($"{Text.Get("EclipseView.BesselianElements.HeaderTime")} t\u2080 = {Formatters.DateTime.Format(new Date(elements.JulianDay0))} TDT (JDE = { elements.JulianDay0.ToString("N6", nf)})");
                beTableHeader.AppendLine(Text.Get("SolarEclipseView.BesselianElements.HeaderValid"));
                BesselianElementsTableHeader = beTableHeader.ToString();

                // Besselian elements table footer
                var beTableFooter = new StringBuilder();
                beTableFooter.AppendLine($"Tan ƒ1 = {elements.TanF1.ToString("N7", nf)}");
                beTableFooter.AppendLine($"Tan ƒ2 = {elements.TanF2.ToString("N7", nf)}");
                BesselianElementsTableFooter = beTableFooter.ToString();

                AddLocationMarker();
                CalculateSarosSeries();
                CalculateLocalCircumstances(observerLocation);
                CalculateCitiesTable();

                CitiesListTable.Select(l => l.Location).ToList().ForEach(c => AddCitiesListMarker(c));

                IsCalculating = false;
            });
        }

        protected override void CalculateLocalCircumstances(CrdsGeographical pos)
        {
            var local = SolarEclipses.LocalCircumstances(elements, pos);

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                LocalCircumstancesTable[0] = new NameValueTableItem(Text.Get("SolarEclipseView.LocalCircumstances.MaxMagnitude"), local.MaxMagnitude > 0 ? Format.Mag.Format(local.MaxMagnitude) : "");
                LocalCircumstancesTable[1] = new NameValueTableItem(Text.Get("SolarEclipseView.LocalCircumstances.MoonSunRatio"), local.MoonToSunDiameterRatio > 0 ? Format.Ratio.Format(local.MoonToSunDiameterRatio) : "");
                LocalCircumstancesTable[2] = new NameValueTableItem(Text.Get("SolarEclipseView.LocalCircumstances.PartialDur"), !double.IsNaN(local.PartialDuration) && local.PartialDuration > 0 ? Format.Time.Format(local.PartialDuration) : "");
                LocalCircumstancesTable[3] = new NameValueTableItem(Text.Get("SolarEclipseView.LocalCircumstances.TotalDur"), !double.IsNaN(local.TotalDuration) && local.TotalDuration > 0 ? Format.Time.Format(local.TotalDuration) : "");
                LocalCircumstancesTable[4] = new NameValueTableItem(Text.Get("SolarEclipseView.LocalCircumstances.ShadowWidth"), local.PathWidth > 0 ? Format.PathWidth.Format(local.PathWidth) : "");

                var items = new List<SolarEclipseLocalContactsTableItem>();
                LocalContactsTable[0] = new SolarEclipseLocalContactsTableItem(Text.Get("SolarEclipseView.LocalCircumstances.C1"), local.PartialBegin);
                LocalContactsTable[1] = new SolarEclipseLocalContactsTableItem(Text.Get("SolarEclipseView.LocalCircumstances.C2"), local.TotalBegin);
                LocalContactsTable[2] = new SolarEclipseLocalContactsTableItem(Text.Get("SolarEclipseView.LocalCircumstances.Max"), local.Maximum);
                LocalContactsTable[3] = new SolarEclipseLocalContactsTableItem(Text.Get("SolarEclipseView.LocalCircumstances.C3"), local.TotalEnd);
                LocalContactsTable[4] = new SolarEclipseLocalContactsTableItem(Text.Get("SolarEclipseView.LocalCircumstances.C4"), local.PartialEnd);
            });

            ObserverLocationName = (IsMouseOverMap && !IsMapLocked) ? $"{Text.Get("EclipseView.MouseCoordinates")} ({Format.Geo.Format(FromGeoPoint(MapMouse))})" : $"{observerLocation.LocationName} ({Format.Geo.Format(observerLocation)})";            
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

                int ln = meeusLunationNumber;
                var eclipses = new List<SolarEclipse>();

                // add current eclipse
                eclipses.Add(eclipse);
                currentSarosSeries = eclipse.Saros;
                SarosSeriesTableTitle = Text.Get("EclipseView.SarosTable.Header", ("currentSarosSeries", currentSarosSeries.ToString()));

                // add previous eclipses
                do
                {
                    var e = eclipsesCalculator.GetNearestSolarEclipse(ln, next: false, saros: true);
                    ln = e.MeeusLunationNumber;
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

                ln = meeusLunationNumber;
                // add next eclipses
                do
                {
                    var e = eclipsesCalculator.GetNearestSolarEclipse(ln, next: true, saros: true);
                    ln = e.MeeusLunationNumber;
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
                    string type = Text.Get($"SolarEclipse.Type.{e.EclipseType}");
                    string subtype = e.IsNonCentral ? $" {Text.Get("SolarEclipse.Type.NoCentral")}" : "";
                    var pbe = eclipsesCalculator.GetBesselianElements(e.JulianDayMaximum);
                    var local = SolarEclipses.LocalCircumstances(pbe, settingsLocation);
                    sarosSeriesTable.Add(new SarosSeriesTableItem()
                    {
                        Member = $"{++sarosMember}",
                        MeeusLunationNumber = e.MeeusLunationNumber,
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
            });
        }

        protected override async void CalculateCitiesTable()
        {
            if (SelectedTabIndex != 3 ||
                citiesListTableLunationNumber == meeusLunationNumber ||
                !CitiesListTable.Any()) return;

            var cities = CitiesListTable.Select(l => l.Location).ToArray();
            var locals = await Task.Run(() => eclipsesCalculator.FindLocalCircumstancesForCities(elements, cities));
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                CitiesListTable.Clear();
                CitiesListTable = new ObservableCollection<SolarEclipseCitiesListTableItem>(locals.Select(c => new SolarEclipseCitiesListTableItem(c, eclipsesCalculator.GetLocalVisibilityString(eclipse, c))));
                citiesListTableLunationNumber = meeusLunationNumber;
            });
        }

        private void LoadLocationsFromFile()
        {
            FillCitiesTable(fromFile: true);
        }

        private void FindLocationsOnTotalPath()
        {
            if (ViewManager.ShowMessageBox("$EclipseView.WarnMessageBox.Title", "$SolarEclipseView.LocalCircumstances.SearchingCitiesOnTotalPathProgress.Warning", System.Windows.MessageBoxButton.OKCancel) == System.Windows.MessageBoxResult.OK)
            {
                FillCitiesTable(fromFile: false);
            }
        }

        private void ClearLocationsTable()
        {
            if (ViewManager.ShowMessageBox("$EclipseView.WarnMessageBox.Title", "$EclipseView.LocalCircumstances.ClearTable.Warning", System.Windows.MessageBoxButton.YesNo) == System.Windows.MessageBoxResult.Yes)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    CitiesListTable.Clear();
                    Markers.Clear();
                    AddLocationMarker();
                    IsCitiesListTableNotEmpty = false;
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
                    string file = ViewManager.ShowOpenFileDialog("$EclipseView.ImportCitiesList.DialogTitle", $"{Text.Get("EclipseView.ImportCitiesList.FileFormat")}|*.csv", out int filterIndex);
                    if (file != null)
                    {
                        ViewManager.ShowProgress("$EclipseView.WaitMessageBox.Title", "$EclipseView.LocalCircumstances.CalculatingCircumstancesProgress", tokenSource);
                        var cities = new CsvLocationsReader().ReadFromFile(file);
                        locals = await Task.Run(() => eclipsesCalculator.FindLocalCircumstancesForCities(elements, cities, tokenSource.Token, null));
                    }
                }
                else
                {
                    ViewManager.ShowProgress("$EclipseView.WaitMessageBox.Title", "$SolarEclipseView.LocalCircumstances.SearchingCitiesOnTotalPathProgress", tokenSource, progress);
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
                    CitiesListTable = new ObservableCollection<SolarEclipseCitiesListTableItem>(locals.Select(c => new SolarEclipseCitiesListTableItem(c, eclipsesCalculator.GetLocalVisibilityString(eclipse, c))));
                    var cities = CitiesListTable.Select(l => l.Location).ToList();
                    cities.ForEach(c => AddCitiesListMarker(c));
                    IsCitiesListTableNotEmpty = CitiesListTable.Any();
                });
            }
        }

        private void ExportLocationsTable()
        {
            var formats = new Dictionary<string, string>
            {
                [Text.Get("EclipseView.LocalCircumstances.OutputFormat.CsvWithFormatting")] = "*.csv",
                [Text.Get("EclipseView.LocalCircumstances.OutputFormat.CsvRawData")] = "*.csv",
            };
            string filter = string.Join("|", formats.Select(kv => $"{kv.Key}|{kv.Value}"));
            var file = ViewManager.ShowSaveFileDialog("$EclipseView.Export", "CitiesList", ".csv", filter, out int selectedFilterIndex);
            if (file != null)
            {
                bool rawData = selectedFilterIndex == 2;
                var writer = new SolarEclipseCitiesTableCsvWriter(rawData);
                writer.Write(file, CitiesListTable);

                var answer = ViewManager.ShowMessageBox("$EclipseView.InfoMessageBox.Title", "$EclipseView.ExportDoneMessage", System.Windows.MessageBoxButton.YesNo);
                if (answer == System.Windows.MessageBoxResult.Yes)
                {
                    System.Diagnostics.Process.Start(file);
                }
            }
        }
    }
}
