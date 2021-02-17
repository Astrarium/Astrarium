using Astrarium.Algorithms;
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
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace Astrarium.Plugins.Eclipses.ViewModels
{
    public class LunarEclipseVM : EclipseVM
    {
        private readonly PolygonStyle polygonStyle = new PolygonStyle(new SolidBrush(Color.FromArgb(70, Color.Black)));

        private LunarEclipse eclipse;
        private LunarEclipseMap map;
        private PolynomialLunarEclipseElements elements;
        private static NumberFormatInfo nf;

        /// <summary>
        /// Table of local contacts instants, displayed to the right of eclipse map
        /// </summary>
        public ObservableCollection<LunarEclipseLocalContactsTableItem> LocalContactsTable { get; private set; } = new ObservableCollection<LunarEclipseLocalContactsTableItem>();

        /// <summary>
        /// Besselian elements table
        /// </summary>
        public ObservableCollection<LunarElementsTableItem> BesselianElementsTable { get; private set; } = new ObservableCollection<LunarElementsTableItem>();

        /// <summary>
        /// Table of local circumstances for selected cities
        /// </summary>
        public ObservableCollection<LunarEclipseCitiesListTableItem> CitiesListTable { get; private set; } = new ObservableCollection<LunarEclipseCitiesListTableItem>();

        /// <summary>
        /// Local circumstance of the eclipse
        /// </summary>
        public LunarEclipseLocalCircumstances LocalCircumstances
        {
            get => GetValue<LunarEclipseLocalCircumstances>(nameof(LocalCircumstances));
            private set => SetValue(nameof(LocalCircumstances), value);
        }

        public ICommand ClearLocationsTableCommand => new Command(ClearLocationsTable);

        static LunarEclipseVM()
        {
            nf = new NumberFormatInfo();
            nf.NumberDecimalSeparator = ".";
            nf.NumberGroupSeparator = "\u2009";
        }

        public LunarEclipseVM(IEclipsesCalculator eclipsesCalculator, IGeoLocationsManager locationsManager, ISky sky, ISettings settings) : base(locationsManager, settings)
        {
            this.eclipsesCalculator = eclipsesCalculator;
            this.settings.PropertyChanged += Settings_PropertyChanged;
            
            SetMapColors();

            for (int i = 0; i < 7; i++)
            {
                LocalContactsTable.Add(new LunarEclipseLocalContactsTableItem(null, null));
            }

            meeusLunationNumber = LunarEphem.Lunation(sky.Context.JulianDay, LunationSystem.Meeus);

            CalculateEclipse(next: true, saros: false);
        }

        private void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Schema")
            {
                IsDarkMode = settings.Get<ColorSchema>("Schema") == ColorSchema.Red;
                TileImageAttributes = GetImageAttributes();
                SetMapColors();
                // TODO: need to recalculate eclipse
            }
        }

        protected override void CalculateLocalCircumstances(CrdsGeographical g)
        {
            var local = LunarEclipses.LocalCircumstances(eclipse, elements, g);

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                var items = new List<LunarEclipseLocalContactsTableItem>();
                LocalContactsTable[0] = new LunarEclipseLocalContactsTableItem("P1: Penumbral begins", local.PenumbralBegin);
                LocalContactsTable[1] = new LunarEclipseLocalContactsTableItem("U1: Partial begins", local.PartialBegin);
                LocalContactsTable[2] = new LunarEclipseLocalContactsTableItem("U2: Total begins", local.TotalBegin);
                LocalContactsTable[3] = new LunarEclipseLocalContactsTableItem("Max: Greatest eclipse", local.Maximum);
                LocalContactsTable[4] = new LunarEclipseLocalContactsTableItem("U3: Total ends", local.TotalEnd);
                LocalContactsTable[5] = new LunarEclipseLocalContactsTableItem("U4: Partial ends", local.PartialEnd);
                LocalContactsTable[6] = new LunarEclipseLocalContactsTableItem("P4: Penumbral ends", local.PenumbralEnd);
            });

            LocalVisibilityDescription = eclipsesCalculator.GetLocalVisibilityString(eclipse, local);

            ObserverLocationName = (IsMouseOverMap && !IsMapLocked) ? $"Mouse coordinates ({Format.Geo.Format(FromGeoPoint(MapMouse))})" : $"{observerLocation.LocationName} ({Format.Geo.Format(observerLocation)})";
            LocalCircumstances = local;
            IsVisibleFromCurrentPlace = true;
        }

        private async void CalculateCitiesTable()
        {
            if (CitiesListTable.Any())
            {
                var cities = CitiesListTable.Select(l => l.Location).ToArray();
                var locals = await Task.Run(() => eclipsesCalculator.FindLocalCircumstancesForCities(eclipse, elements, cities));
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    CitiesListTable.Clear();
                    CitiesListTable = new ObservableCollection<LunarEclipseCitiesListTableItem>(locals.Select(c => new LunarEclipseCitiesListTableItem(c, eclipsesCalculator.GetLocalVisibilityString(eclipse, c))));
                });
                NotifyPropertyChanged(nameof(CitiesListTable));
            }
        }

        private void SetMapColors()
        {
            MapThumbnailBackColor = IsDarkMode ? Color.FromArgb(0xFF, 0x33, 0, 0) : Color.FromArgb(0xFF, 0x33, 0x33, 0x33);
            MapThumbnailForeColor = IsDarkMode ? Color.FromArgb(0xFF, 0x59, 0, 0) : Color.FromArgb(0xFF, 0x59, 0x59, 0x59);
        }

        protected override async void CalculateEclipse(bool next, bool saros)
        {
            IsCalculating = true;

            eclipse = eclipsesCalculator.GetNearestLunarEclipse(meeusLunationNumber, next, saros);
            julianDay = eclipse.JulianDayMaximum;
            meeusLunationNumber = eclipse.MeeusLunationNumber;
            elements = eclipsesCalculator.GetLunarEclipseElements(eclipse.JulianDayMaximum);
            string type = eclipse.EclipseType.ToString();
            EclipseDescription = $"{type} lunar eclipse";
            EclipseDate = Formatters.Date.Format(new Date(eclipse.JulianDayMaximum, 0));
            PrevSarosEnabled = eclipsesCalculator.GetNearestLunarEclipse(meeusLunationNumber, next: false, saros: true).Saros == eclipse.Saros;
            NextSarosEnabled = eclipsesCalculator.GetNearestLunarEclipse(meeusLunationNumber, next: true, saros: true).Saros == eclipse.Saros;

            await Task.Run(() =>
            {
                map = LunarEclipses.EclipseMap(eclipse, elements);

                var tracks = new List<Track>();
                var polygons = new List<Polygon>();
                var markers = new List<Marker>();

                if (map.PenumbralBegin != null)
                {
                    var polygon = new Polygon(polygonStyle);
                    polygon.AddRange(map.PenumbralBegin.Select(p => ToGeo(p)));
                    polygons.Add(polygon);
                }

                if (map.PartialBegin != null)
                {
                    var polygon = new Polygon(polygonStyle);
                    polygon.AddRange(map.PartialBegin.Select(p => ToGeo(p)));
                    polygons.Add(polygon);
                }

                if (map.TotalBegin != null)
                {
                    var polygon = new Polygon(polygonStyle);
                    polygon.AddRange(map.TotalBegin.Select(p => ToGeo(p)));
                    polygons.Add(polygon);
                }

                if (map.TotalEnd != null)
                {
                    var polygon = new Polygon(polygonStyle);
                    polygon.AddRange(map.TotalEnd.Select(p => ToGeo(p)));
                    polygons.Add(polygon);
                }

                if (map.PartialEnd != null)
                {
                    var polygon = new Polygon(polygonStyle);
                    polygon.AddRange(map.PartialEnd.Select(p => ToGeo(p)));
                    polygons.Add(polygon);
                }

                if (map.PenumbralEnd != null)
                {
                    var polygon = new Polygon(polygonStyle);
                    polygon.AddRange(map.PenumbralEnd.Select(p => ToGeo(p)));
                    polygons.Add(polygon);
                }

                // Brown lunation number
                var lunation = LunarEphem.Lunation(eclipse.JulianDayMaximum);

                var eclipseGeneralDetails = new ObservableCollection<NameValueTableItem>()
                {
                    new NameValueTableItem("Type", $"{type}"),
                    new NameValueTableItem("Saros", $"{eclipse.Saros}"),
                    new NameValueTableItem("Date", $"{EclipseDate}"),
                    new NameValueTableItem("Magnitude", $"{eclipse.Magnitude.ToString("N5", nf)}"),
                    new NameValueTableItem("Gamma", $"{eclipse.Gamma.ToString("N5", nf)}"),
                    new NameValueTableItem("Penumbral duration", $"{Format.Time.Format(eclipse.PenumbralDuration)}"),
                    new NameValueTableItem("Partial duration", $"{Format.Time.Format(eclipse.PartialDuration)}"),
                    new NameValueTableItem("Total duration", $"{Format.Time.Format(eclipse.TotalityDuration)}"),
                    new NameValueTableItem("Brown Lunation Number", $"{lunation}"),
                    new NameValueTableItem("ΔT", $"{elements.DeltaT.ToString("N1", nf) } s")
                };
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    EclipseGeneralDetails = eclipseGeneralDetails;
                });

                var eclipseContacts = new ObservableCollection<ContactsTableItem>();

                eclipseContacts.Add(new ContactsTableItem("P1: Penumbral begins", eclipse.JulianDayFirstContactPenumbra));
                eclipseContacts.Add(new ContactsTableItem("U1: Partial begins", eclipse.JulianDayFirstContactUmbra));
                eclipseContacts.Add(new ContactsTableItem("U2: Total begins", eclipse.JulianDayTotalBegin));
                eclipseContacts.Add(new ContactsTableItem("Max: Greatest eclipse", eclipse.JulianDayMaximum));
                eclipseContacts.Add(new ContactsTableItem("U3: Total ends", eclipse.JulianDayTotalEnd));
                eclipseContacts.Add(new ContactsTableItem("U4: Partial ends", eclipse.JulianDayLastContactUmbra));
                eclipseContacts.Add(new ContactsTableItem("P4: Penumbral end", eclipse.JulianDayLastContactPenumbra));

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
                beTableHeader.AppendLine($"Elements for t\u2080 = {Formatters.DateTime.Format(new Date(elements.JulianDay0))} TDT (JDE = { elements.JulianDay0.ToString("N6", nf)})");
                beTableHeader.AppendLine($"The Besselian elements are valid over the period t\u2080 - 2h ≤ t\u2080 ≤ t\u2080 + 2h");
                BesselianElementsTableHeader = beTableHeader.ToString();

                Markers = new List<Marker>();
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

        protected override async void CalculateSarosSeries()
        {
            await Task.Run(() =>
            {
                if (SelectedTabIndex != 2 || currentSarosSeries == eclipse.Saros) return;

                IsCalculating = true;

                int ln = meeusLunationNumber;
                List<LunarEclipse> eclipses = new List<LunarEclipse>();

                // add current eclipse
                eclipses.Add(eclipse);
                currentSarosSeries = eclipse.Saros;
                SarosSeriesTableTitle = $"List of eclipses of saros series {currentSarosSeries}";

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
                    string type = e.EclipseType.ToString();
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
                NotifyPropertyChanged(
                    nameof(SarosSeriesTable)
                );
            });
        }

        protected override void AddToCitiesList(CrdsGeographical location)
        {
            var local = eclipsesCalculator.FindLocalCircumstancesForCities(eclipse, elements, new[] { location }).First();
            CitiesListTable.Add(new LunarEclipseCitiesListTableItem(local, eclipsesCalculator.GetLocalVisibilityString(eclipse, local)));
            IsCitiesListTableNotEmpty = true;
        }

        private void ClearLocationsTable()
        {
            if (ViewManager.ShowMessageBox("Warning", "Do you really want to clear the table?", System.Windows.MessageBoxButton.YesNo) == System.Windows.MessageBoxResult.Yes)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    CitiesListTable.Clear();
                    IsCitiesListTableNotEmpty = false;
                    NotifyPropertyChanged(nameof(CitiesListTable));
                });
            }
        }
    }
}
