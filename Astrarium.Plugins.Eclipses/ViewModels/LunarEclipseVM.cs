using Astrarium.Algorithms;
using Astrarium.Plugins.Eclipses.ImportExport;
using Astrarium.Plugins.Eclipses.Types;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
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
    public class LunarEclipseVM : EclipseVM
    {
        /// <summary>
        /// Table of local contacts instants, displayed to the right of eclipse map
        /// </summary>
        public ObservableCollection<LunarEclipseLocalContactsTableItem> LocalContactsTable 
        { 
            get => GetValue(nameof(LocalContactsTable), new ObservableCollection<LunarEclipseLocalContactsTableItem>(Enumerable.Repeat(new LunarEclipseLocalContactsTableItem(null, null), 7)));
            private set => SetValue(nameof(LocalContactsTable), value);
        }

        /// <summary>
        /// Besselian elements table
        /// </summary>
        public ObservableCollection<LunarElementsTableItem> BesselianElementsTable
        {
            get => GetValue(nameof(BesselianElementsTable), new ObservableCollection<LunarElementsTableItem>());
            private set => SetValue(nameof(BesselianElementsTable), value);
        }

        /// <summary>
        /// Table of local circumstances for selected cities
        /// </summary>
        public ObservableCollection<LunarEclipseCitiesListTableItem> CitiesListTable
        {
            get => GetValue(nameof(CitiesListTable), new ObservableCollection<LunarEclipseCitiesListTableItem>());
            private set => SetValue(nameof(CitiesListTable), value);
        }

        /// <summary>
        /// Local circumstance of the eclipse
        /// </summary>
        public LunarEclipseLocalCircumstances LocalCircumstances
        {
            get => GetValue<LunarEclipseLocalCircumstances>(nameof(LocalCircumstances));
            private set => SetValue(nameof(LocalCircumstances), value);
        }

        private readonly PolygonStyle polygonStyle = new PolygonStyle(new SolidBrush(Color.FromArgb(70, Color.Black)));

        public ICommand ClearLocationsTableCommand => new Command(ClearLocationsTable);
        public ICommand LoadLocationsFromFileCommand => new Command(LoadLocationsFromFile);
        public ICommand ExportLocationsTableCommand => new Command(ExportLocationsTable);

        private LunarEclipse eclipse;
        private LunarEclipseMap map;
        private PolynomialLunarEclipseElements elements;

        public LunarEclipseVM(
            ISky sky,
            IEclipsesCalculator eclipsesCalculator,
            IGeoLocationsManager locationsManager,
            ISettings settings)
            : base(sky, eclipsesCalculator, locationsManager, settings) { }

        protected override void CalculateLocalCircumstances(CrdsGeographical g)
        {
            var local = LunarEclipses.LocalCircumstances(eclipse, elements, g);

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                var items = new List<LunarEclipseLocalContactsTableItem>();
                LocalContactsTable[0] = new LunarEclipseLocalContactsTableItem(Text.Get("LunarEclipseView.LocalCircumstances.P1"), local.PenumbralBegin);
                LocalContactsTable[1] = new LunarEclipseLocalContactsTableItem(Text.Get("LunarEclipseView.LocalCircumstances.U1"), local.PartialBegin);
                LocalContactsTable[2] = new LunarEclipseLocalContactsTableItem(Text.Get("LunarEclipseView.LocalCircumstances.U2"), local.TotalBegin);
                LocalContactsTable[3] = new LunarEclipseLocalContactsTableItem(Text.Get("LunarEclipseView.LocalCircumstances.Max"), local.Maximum);
                LocalContactsTable[4] = new LunarEclipseLocalContactsTableItem(Text.Get("LunarEclipseView.LocalCircumstances.U3"), local.TotalEnd);
                LocalContactsTable[5] = new LunarEclipseLocalContactsTableItem(Text.Get("LunarEclipseView.LocalCircumstances.U4"), local.PartialEnd);
                LocalContactsTable[6] = new LunarEclipseLocalContactsTableItem(Text.Get("LunarEclipseView.LocalCircumstances.P4"), local.PenumbralEnd);
            });

            ObserverLocationName = (IsMouseOverMap && !IsMapLocked) ? $"{Text.Get("EclipseView.MouseCoordinates")} ({Format.Geo.Format(FromGeoPoint(MapMouse))})" : $"{observerLocation.LocationName} ({Format.Geo.Format(observerLocation)})";
            LocalVisibilityDescription = eclipsesCalculator.GetLocalVisibilityString(eclipse, local);
            IsVisibleFromCurrentPlace = !local.IsInvisible;
            LocalCircumstances = local;
        }

        protected override async void CalculateCitiesTable()
        {
            if (SelectedTabIndex != 3 ||
                citiesListTableLunationNumber == meeusLunationNumber || 
                !CitiesListTable.Any()) return;

            var cities = CitiesListTable.Select(l => l.Location).ToArray();
            var locals = await Task.Run(() => eclipsesCalculator.FindLocalCircumstancesForCities(eclipse, elements, cities));
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                CitiesListTable.Clear();
                CitiesListTable = new ObservableCollection<LunarEclipseCitiesListTableItem>(locals.Select(c => new LunarEclipseCitiesListTableItem(c, eclipsesCalculator.GetLocalVisibilityString(eclipse, c))));
                citiesListTableLunationNumber = meeusLunationNumber;
            });
        }

        protected override async void CalculateEclipse(bool next, bool saros)
        {
            IsCalculating = true;

            eclipse = eclipsesCalculator.GetNearestLunarEclipse(meeusLunationNumber, next, saros);
            julianDay = eclipse.JulianDayMaximum;
            meeusLunationNumber = eclipse.MeeusLunationNumber;
            elements = eclipsesCalculator.GetLunarEclipseElements(eclipse.JulianDayMaximum);
            string type = Text.Get($"LunarEclipse.Type.{eclipse.EclipseType}");
            EclipseDescription = Text.Get("LunarEclipseView.EclipseDescription", ("type", type));
            EclipseDate = Formatters.Date.Format(new Date(eclipse.JulianDayMaximum, 0));
            PrevSarosEnabled = eclipsesCalculator.GetNearestLunarEclipse(meeusLunationNumber, next: false, saros: true).Saros == eclipse.Saros;
            NextSarosEnabled = eclipsesCalculator.GetNearestLunarEclipse(meeusLunationNumber, next: true, saros: true).Saros == eclipse.Saros;

            await Task.Run(() =>
            {
                map = LunarEclipses.EclipseMap(eclipse, elements);

                Polygons.Clear();
                Tracks.Clear();
                Markers.Clear();

                if (map.PenumbralBegin != null)
                {
                    var polygon = new Polygon(polygonStyle);
                    polygon.AddRange(map.PenumbralBegin.Select(p => ToGeo(p)));
                    Polygons.Add(polygon);
                }

                if (map.PartialBegin != null)
                {
                    var polygon = new Polygon(polygonStyle);
                    polygon.AddRange(map.PartialBegin.Select(p => ToGeo(p)));
                    Polygons.Add(polygon);
                }

                if (map.TotalBegin != null)
                {
                    var polygon = new Polygon(polygonStyle);
                    polygon.AddRange(map.TotalBegin.Select(p => ToGeo(p)));
                    Polygons.Add(polygon);
                }

                if (map.TotalEnd != null)
                {
                    var polygon = new Polygon(polygonStyle);
                    polygon.AddRange(map.TotalEnd.Select(p => ToGeo(p)));
                    Polygons.Add(polygon);
                }

                if (map.PartialEnd != null)
                {
                    var polygon = new Polygon(polygonStyle);
                    polygon.AddRange(map.PartialEnd.Select(p => ToGeo(p)));
                    Polygons.Add(polygon);
                }

                if (map.PenumbralEnd != null)
                {
                    var polygon = new Polygon(polygonStyle);
                    polygon.AddRange(map.PenumbralEnd.Select(p => ToGeo(p)));
                    Polygons.Add(polygon);
                }

                // Brown lunation number
                var lunation = LunarEphem.Lunation(eclipse.JulianDayMaximum);

                var eclipseGeneralDetails = new ObservableCollection<NameValueTableItem>()
                {
                    new NameValueTableItem(Text.Get("LunarEclipseView.EclipseType"), $"{type}"),
                    new NameValueTableItem(Text.Get("LunarEclipseView.EclipseSaros"), $"{eclipse.Saros}"),
                    new NameValueTableItem(Text.Get("LunarEclipseView.EclipseDate"), $"{EclipseDate}"),
                    new NameValueTableItem(Text.Get("LunarEclipseView.EclipseMagnitude"), $"{eclipse.Magnitude.ToString("N5", nf)}"),
                    new NameValueTableItem(Text.Get("LunarEclipseView.EclipseGamma"), $"{eclipse.Gamma.ToString("N5", nf)}"),
                    new NameValueTableItem(Text.Get("LunarEclipseView.EclipsePenumbralDuration"), $"{Format.Time.Format(eclipse.PenumbralDuration)}"),
                    new NameValueTableItem(Text.Get("LunarEclipseView.EclipsePartialDuration"), $"{Format.Time.Format(eclipse.PartialDuration)}"),
                    new NameValueTableItem(Text.Get("LunarEclipseView.EclipseTotalDuration"), $"{Format.Time.Format(eclipse.TotalityDuration)}"),
                    new NameValueTableItem(Text.Get("LunarEclipseView.BrownLunationNumber"), $"{lunation}"),
                    new NameValueTableItem("ΔT", $"{elements.DeltaT.ToString("N1", nf) } s")
                };
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    EclipseGeneralDetails = eclipseGeneralDetails;
                });

                var eclipseContacts = new ObservableCollection<ContactsTableItem>
                {
                    new ContactsTableItem(Text.Get("LunarEclipseView.LocalCircumstances.P1"), eclipse.JulianDayFirstContactPenumbra),
                    new ContactsTableItem(Text.Get("LunarEclipseView.LocalCircumstances.U1"), eclipse.JulianDayFirstContactUmbra),
                    new ContactsTableItem(Text.Get("LunarEclipseView.LocalCircumstances.U2"), eclipse.JulianDayTotalBegin),
                    new ContactsTableItem(Text.Get("LunarEclipseView.LocalCircumstances.Max"), eclipse.JulianDayMaximum),
                    new ContactsTableItem(Text.Get("LunarEclipseView.LocalCircumstances.U3"), eclipse.JulianDayTotalEnd),
                    new ContactsTableItem(Text.Get("LunarEclipseView.LocalCircumstances.U4"), eclipse.JulianDayLastContactUmbra),
                    new ContactsTableItem(Text.Get("LunarEclipseView.LocalCircumstances.P4"), eclipse.JulianDayLastContactPenumbra)
                };

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    EclipseContacts = eclipseContacts;
                });

                var besselianElementsTable = new ObservableCollection<LunarElementsTableItem>();
                for (int i = 0; i < 4; i++)
                {
                    besselianElementsTable.Add(new LunarElementsTableItem(i, elements));
                }
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    BesselianElementsTable = besselianElementsTable;
                });

                // Besselian elements table header
                var beTableHeader = new StringBuilder();

                beTableHeader.AppendLine($"{Text.Get("EclipseView.BesselianElements.HeaderTime")} t\u2080 = {Formatters.DateTime.Format(new Date(elements.JulianDay0))} TDT (JDE = { elements.JulianDay0.ToString("N6", nf)})");
                beTableHeader.AppendLine(Text.Get("LunarEclipseView.BesselianElements.HeaderValid"));

                BesselianElementsTableHeader = beTableHeader.ToString();

                AddLocationMarker();
                CalculateSarosSeries();
                CalculateLocalCircumstances(observerLocation);
                CalculateCitiesTable();

                CitiesListTable.Select(l => l.Location).ToList().ForEach(c => AddCitiesListMarker(c));

                IsCalculating = false;
            });
        }

        protected override async void CalculateSarosSeries()
        {
            await Task.Run(() =>
            {
                if (SelectedTabIndex != 2 || currentSarosSeries == eclipse.Saros) return;

                IsCalculating = true;

                int ln = meeusLunationNumber;
                var eclipses = new List<LunarEclipse>();

                // add current eclipse
                eclipses.Add(eclipse);
                currentSarosSeries = eclipse.Saros;
                SarosSeriesTableTitle = Text.Get("EclipseView.SarosTable.Header", ("currentSarosSeries", currentSarosSeries.ToString()));

                // add previous eclipses
                do
                {
                    var e = eclipsesCalculator.GetNearestLunarEclipse(ln, next: false, saros: true);
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
                    var e = eclipsesCalculator.GetNearestLunarEclipse(ln, next: true, saros: true);
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
                    string type = Text.Get($"LunarEclipse.Type.{e.EclipseType}");
                    var pbe = eclipsesCalculator.GetLunarEclipseElements(e.JulianDayMaximum);
                    var local = LunarEclipses.LocalCircumstances(e, pbe, settingsLocation);
                    sarosSeriesTable.Add(new SarosSeriesTableItem()
                    {
                        Member = $"{++sarosMember}",
                        MeeusLunationNumber = e.MeeusLunationNumber,
                        JulianDay = e.JulianDayMaximum,
                        Date = Formatters.Date.Format(new Date(e.JulianDayMaximum, 0)),
                        Type = $"{type}",
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

        protected override void AddToCitiesList(CrdsGeographical location)
        {
            var local = eclipsesCalculator.FindLocalCircumstancesForCities(eclipse, elements, new[] { location }).First();
            CitiesListTable.Add(new LunarEclipseCitiesListTableItem(local, eclipsesCalculator.GetLocalVisibilityString(eclipse, local)));
            AddCitiesListMarker(location);
            IsCitiesListTableNotEmpty = true;
        }

        private async void LoadLocationsFromFile()
        {
            var tokenSource = new CancellationTokenSource();
            var progress = new Progress<double>();
            ICollection<LunarEclipseLocalCircumstances> locals = null;

            try
            {
                string file = ViewManager.ShowOpenFileDialog("$EclipseView.ImportCitiesList.DialogTitle", $"{Text.Get("EclipseView.ImportCitiesList.FileFormat")}|*.csv");
                if (file != null)
                {
                    ViewManager.ShowProgress("$EclipseView.WaitMessageBox.Title", "$EclipseView.LocalCircumstances.CalculatingCircumstancesProgress", tokenSource);
                    var cities = new CsvLocationsReader().ReadFromFile(file);
                    locals = await Task.Run(() => eclipsesCalculator.FindLocalCircumstancesForCities(eclipse, elements, cities, tokenSource.Token, null));
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
                    CitiesListTable = new ObservableCollection<LunarEclipseCitiesListTableItem>(locals.Select(c => new LunarEclipseCitiesListTableItem(c, eclipsesCalculator.GetLocalVisibilityString(eclipse, c))));
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
                var writer = new LunarEclipseCitiesTableCsvWriter(rawData);
                writer.Write(file, CitiesListTable);

                var answer = ViewManager.ShowMessageBox("$EclipseView.InfoMessageBox.Title", "$EclipseView.ExportDoneMessage", System.Windows.MessageBoxButton.YesNo);
                if (answer == System.Windows.MessageBoxResult.Yes)
                {
                    System.Diagnostics.Process.Start(file);
                }
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
    }
}
