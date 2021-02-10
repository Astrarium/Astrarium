using Astrarium.Algorithms;
using Astrarium.Plugins.Eclipses.Types;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
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
        private LunarEclipseContacts contacts;

        /// <summary>
        /// Table of local contacts instants, displayed to the right of eclipse map
        /// </summary>
        public ObservableCollection<LunarEclipseLocalContactsTableItem> LocalContactsTable { get; private set; } = new ObservableCollection<LunarEclipseLocalContactsTableItem>();

        public LunarEclipseVM(IEclipsesCalculator eclipsesCalculator, IGeoLocationsManager locationsManager, ISky sky, ISettings settings) : base(locationsManager, settings)
        {
            this.eclipsesCalculator = eclipsesCalculator;

            //SettingsLocationName = $"Local visibility ({observerLocation.LocationName})";
            //IsDarkMode = settings.Get<ColorSchema>("Schema") == ColorSchema.Red;
            //ChartZoomLevel = 1;

            //SetMapColors();

            for (int i = 0; i < 7; i++)
            {
                LocalContactsTable.Add(new LunarEclipseLocalContactsTableItem(null, null));
            }

            JulianDay = sky.Context.JulianDay - LunarEphem.SINODIC_PERIOD;

            CalculateEclipse(next: true, saros: false);
        }

        protected override void CalculateLocalCircumstances(CrdsGeographical g)
        {
            var local = LunarEclipses.LocalCircumstances(contacts, g);

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


                //LocalCircumstancesTable[0] = new NameValueTableItem("Maximal magnitude", local.MaxMagnitude > 0 ? Format.Mag.Format(local.MaxMagnitude) : "");
                //LocalCircumstancesTable[1] = new NameValueTableItem("Moon/Sun diameter ratio", local.MoonToSunDiameterRatio > 0 ? Format.Ratio.Format(local.MoonToSunDiameterRatio) : "");
                //LocalCircumstancesTable[2] = new NameValueTableItem("Partial phase duration", !double.IsNaN(local.PartialDuration) && local.PartialDuration > 0 ? Format.Time.Format(local.PartialDuration) : "");
                //LocalCircumstancesTable[3] = new NameValueTableItem("Total phase duration", !double.IsNaN(local.TotalDuration) && local.TotalDuration > 0 ? Format.Time.Format(local.TotalDuration) : "");
                //LocalCircumstancesTable[4] = new NameValueTableItem("Shadow path width", local.PathWidth > 0 ? Format.PathWidth.Format(local.PathWidth) : "");
            });

            LocalVisibilityDescription =
                (local.PenumbralBegin.LunarAltitude > 0/* -contacts.FirstContactPenumbra.Parallax*/).ToString();

            ObserverLocationName = (IsMouseOverMap && !IsMapLocked) ? $"Mouse coordinates ({Format.Geo.Format(FromGeoPoint(MapMouse))})" : $"{observerLocation.LocationName} ({Format.Geo.Format(observerLocation)})";
        }

        protected override void CalculateEclipse(bool next, bool saros)
        {
            IsCalculating = true;

            eclipse = eclipsesCalculator.GetNearestLunarEclipse(JulianDay, next, saros);
            JulianDay = eclipse.JulianDayMaximum;
            EclipseDate = Formatters.Date.Format(new Date(JulianDay, 0));

            string type = eclipse.EclipseType.ToString();
            EclipseDescription = $"{type} lunar eclipse";
            LocalVisibilityDescription = "visible as... TODO";

            contacts = eclipsesCalculator.GetLunarEclipseContacts(eclipse);


            map = LunarEclipses.EclipseMap(contacts);

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

            Tracks = tracks;
            Polygons = polygons;
            IsCalculating = false;
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
