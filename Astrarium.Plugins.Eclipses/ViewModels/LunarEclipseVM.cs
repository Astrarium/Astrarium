using Astrarium.Algorithms;
using Astrarium.Plugins.Eclipses.Types;
using Astrarium.Types;
using System;
using System.Collections.Generic;
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
        
        public LunarEclipseVM(IEclipsesCalculator eclipsesCalculator, IGeoLocationsManager locationsManager, ISky sky, ISettings settings) : base(locationsManager, settings)
        {
            this.eclipsesCalculator = eclipsesCalculator;

            //SettingsLocationName = $"Local visibility ({observerLocation.LocationName})";
            //IsDarkMode = settings.Get<ColorSchema>("Schema") == ColorSchema.Red;
            //ChartZoomLevel = 1;
            
            //SetMapColors();
            
            JulianDay = sky.Context.JulianDay - LunarEphem.SINODIC_PERIOD;

            CalculateEclipse(next: true, saros: false);
        }

        protected override void CalculateLocalCircumstances(CrdsGeographical g)
        {
            // TODO: implement

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

            var contacts = eclipsesCalculator.GetLunarEclipseContacts(eclipse);

            map = LunarEclipses.EclipseMap(contacts);

            var tracks = new List<Track>();
            var polygons = new List<Polygon>();
            var markers = new List<Marker>();
            
            if (map.P1 != null)
            {
                var polygon = new Polygon(polygonStyle);
                polygon.AddRange(map.P1.Select(p => ToGeo(p)));
                polygons.Add(polygon);
            }

            if (map.U1 != null)
            {
                var polygon = new Polygon(polygonStyle);
                polygon.AddRange(map.U1.Select(p => ToGeo(p)));
                polygons.Add(polygon);
            }

            if (map.U2 != null)
            {
                var polygon = new Polygon(polygonStyle);
                polygon.AddRange(map.U2.Select(p => ToGeo(p)));
                polygons.Add(polygon);
            }

            if (map.U3 != null)
            {
                var polygon = new Polygon(polygonStyle);
                polygon.AddRange(map.U3.Select(p => ToGeo(p)));
                polygons.Add(polygon);
            }

            if (map.U4 != null)
            {
                var polygon = new Polygon(polygonStyle);
                polygon.AddRange(map.U4.Select(p => ToGeo(p)));
                polygons.Add(polygon);
            }

            if (map.P4 != null)
            {
                var polygon = new Polygon(polygonStyle);
                polygon.AddRange(map.P4.Select(p => ToGeo(p)));
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
