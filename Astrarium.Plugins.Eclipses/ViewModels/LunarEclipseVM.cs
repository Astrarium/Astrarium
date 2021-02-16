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
        /// General eclipse info table
        /// </summary>
        public ObservableCollection<NameValueTableItem> EclipseGeneralDetails { get; private set; } = new ObservableCollection<NameValueTableItem>();


        /// <summary>
        /// Local circumstance of the eclipse
        /// </summary>
        public LunarEclipseLocalCircumstances LocalCircumstances
        {
            get => GetValue<LunarEclipseLocalCircumstances>(nameof(LocalCircumstances));
            private set => SetValue(nameof(LocalCircumstances), value);
        }

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

            JulianDay = sky.Context.JulianDay - LunarEphem.SINODIC_PERIOD;

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

        private void SetMapColors()
        {
            MapThumbnailBackColor = IsDarkMode ? Color.FromArgb(0xFF, 0x33, 0, 0) : Color.FromArgb(0xFF, 0x33, 0x33, 0x33);
            MapThumbnailForeColor = IsDarkMode ? Color.FromArgb(0xFF, 0x59, 0, 0) : Color.FromArgb(0xFF, 0x59, 0x59, 0x59);
        }

        protected override void CalculateEclipse(bool next, bool saros)
        {
            IsCalculating = true;

            eclipse = eclipsesCalculator.GetNearestLunarEclipse(JulianDay, next, saros);
            JulianDay = eclipse.JulianDayMaximum;
            EclipseDate = Formatters.Date.Format(new Date(JulianDay, 0));

            string type = eclipse.EclipseType.ToString();
            EclipseDescription = $"{type} lunar eclipse";

            elements = eclipsesCalculator.GetLunarEclipseElements(eclipse.JulianDayMaximum);

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
            var lunation = LunarEphem.Lunation(JulianDay);

            var eclipseGeneralDetails = new ObservableCollection<NameValueTableItem>()
            {
                new NameValueTableItem("Type", $"{type}{type}"),
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

            Markers = new List<Marker>();
            Tracks = tracks;
            Polygons = polygons;
            IsCalculating = false;

            AddLocationMarker();
            CalculateSarosSeries();
            CalculateLocalCircumstances(observerLocation);
            //CalculateCitiesTable();


            NotifyPropertyChanged(nameof(EclipseGeneralDetails));
        }

        protected override void CalculateSarosSeries()
        {
            // TODO: implement this
        }

        protected override void ExportSarosSeriesTable()
        {
            // TODO: implement this
        }

        protected override void AddToCitiesList(CrdsGeographical location)
        {
            // TODO: implement this
        }
    }
}
