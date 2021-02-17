using Astrarium.Algorithms;
using Astrarium.Plugins.Eclipses.Types;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Eclipses
{
    public class EclipsesCalculator : BaseAstroEventsProvider, IEclipsesCalculator
    {
        private readonly ISky sky;
        private CelestialObject sun;
        private CelestialObject moon;
        private IGeoLocationsManager locationsManager;

        public EclipsesCalculator(ISky sky, IGeoLocationsManager locationsManager)
        {
            this.sky = sky;
            this.sky.Calculated += Sky_Calculated;
            this.locationsManager = locationsManager;
        }

        private void Sky_Calculated()
        {
            if (sun == null)
            {
                sun = sky.Search("@sun", f => true).FirstOrDefault();
            }
            if (moon == null)
            {
                moon = sky.Search("@moon", f => true).FirstOrDefault();
            }
        }

        public override void ConfigureAstroEvents(AstroEventsConfig cfg)
        {
            cfg["Eclipses.Solar"] = FindSolarEclipses;
            cfg["Eclipses.Lunar"] = FindLunarEclipses;
        }

        private ICollection<AstroEvent> FindSolarEclipses(AstroEventsContext context)
        {
            List<AstroEvent> events = new List<AstroEvent>();

            int ln = LunarEphem.Lunation(context.From, LunationSystem.Meeus);
            do
            {
                SolarEclipse eclipse = GetNearestSolarEclipse(ln, next: true, saros: false);
                double jd = eclipse.JulianDayMaximum;
                if (jd <= context.To)
                {
                    var pbe = GetBesselianElements(jd);
                    var localCirc = SolarEclipses.LocalCircumstances(pbe, context.GeoLocation);

                    string type = eclipse.EclipseType.ToString();
                    string subtype = eclipse.IsNonCentral ? " non-central" : "";
                    string phase = eclipse.EclipseType == SolarEclipseType.Partial ? $" (max phase {Formatters.Phase.Format(eclipse.Magnitude)})" : "";
                    double jdMax = localCirc.MaxMagnitude > 0 ? localCirc.Maximum.JulianDay : jd;
                    string localVisibility = GetLocalVisibilityString(eclipse, localCirc);

                    events.Add(new AstroEvent(jdMax, $"{type}{subtype} solar eclipse{phase}, {localVisibility} from current point.", sun, moon));
                    ln++;
                }
                else
                {
                    break;
                }
            }
            while (true);
            return events;
        }

        private ICollection<AstroEvent> FindLunarEclipses(AstroEventsContext context)
        {
            List<AstroEvent> events = new List<AstroEvent>();

            int ln = LunarEphem.Lunation(context.From, LunationSystem.Meeus);
            do
            {
                LunarEclipse eclipse = LunarEclipses.NearestEclipse(ln, next: true);
                double jd = eclipse.JulianDayMaximum;
                if (jd <= context.To)
                {
                    double jdMax = jd;
                    string type = eclipse.EclipseType.ToString();
                    string phase = Formatters.Phase.Format(eclipse.Magnitude);
                    var elements = GetLunarEclipseElements(jdMax);
                    var local = LunarEclipses.LocalCircumstances(eclipse, elements, context.GeoLocation);
                    string localVisibility = GetLocalVisibilityString(eclipse, local);
                    events.Add(new AstroEvent(jdMax, $"{type} lunar eclipse (magnitude {phase}), {localVisibility} from current point.", moon));
                    ln++;
                }
                else
                {
                    break;
                }
            }
            while (true);
            return events;
        }

        public LunarEclipse GetNearestLunarEclipse(int ln, bool next, bool saros)
        {
            return LunarEclipses.NearestEclipse(ln + (next ? 1 : -1) * (saros ? 223 : 1), next);
        }

        public SolarEclipse GetNearestSolarEclipse(int ln, bool next, bool saros)
        {
            do
            {
                ln += (next ? 1 : -1) * (saros ? 223 : 1);
                var eclipse = SolarEclipses.NearestEclipse(ln, next);
                if (eclipse.IsUncertain)
                {
                    var pbe = GetBesselianElements(eclipse.JulianDayMaximum);
                    var be = pbe.GetInstantBesselianElements(eclipse.JulianDayMaximum);
                    bool isExist = Math.Sqrt(be.X * be.X + be.Y * be.Y) - be.L1 <= 0.999;
                    if (isExist)
                    {
                        eclipse.IsUncertain = false;
                        return eclipse;
                    }
                    else
                    {
                        continue;
                    }
                }
                else
                {
                    return eclipse;
                }
            }
            while (true);
        }

        public string GetLocalVisibilityString(SolarEclipse eclipse, SolarEclipseLocalCircumstances localCirc)
        {
            string localVisibility = "invisible";
            if (localCirc.MaxMagnitude > 0)
            {
                string localMag = Formatters.Phase.Format(localCirc.MaxMagnitude);
                string asPartial = eclipse.EclipseType != SolarEclipseType.Partial ? " as partial" : "";

                // max instant not visible
                if (localCirc.Maximum.SolarAltitude <= 0)
                {
                    if ((localCirc.TotalEnd != null && localCirc.TotalEnd.SolarAltitude < 0) || localCirc.PartialEnd.SolarAltitude < 0)
                        localVisibility = $"visible{asPartial} on sunset (max phase {localMag})";
                    else if (localCirc.PartialBegin.SolarAltitude < 0 || (localCirc.TotalBegin != null && localCirc.TotalBegin.SolarAltitude < 0))
                        localVisibility = $"visible{asPartial} on sunrise (max phase {localMag})";
                }
                // max instant visible
                else
                {
                    if (localCirc.TotalDuration > 0)
                        localVisibility = "completely visible";
                    else
                        localVisibility = $"visible{asPartial} (max phase {localMag})";
                }
            }

            return localVisibility;
        }

        public string GetLocalVisibilityString(LunarEclipse eclipse, LunarEclipseLocalCircumstances localCirc)
        {
            if (localCirc.Maximum.LunarAltitude > 0)
                return "visible";

            string localVisibility = "invisible";

            if (localCirc.PenumbralBegin.LunarAltitude > 0)
                localVisibility = "visible begin of penumbral phase";

            if (localCirc.PartialBegin?.LunarAltitude > 0)
                localVisibility = "visible begin of partial phase";

            if (localCirc.TotalBegin?.LunarAltitude > 0)
                localVisibility = "visible begin of total phase";

            if (localCirc.PenumbralEnd.LunarAltitude > 0)
                localVisibility = "visible end of penumbral phase";

            if (localCirc.PartialEnd?.LunarAltitude > 0)
                localVisibility = "visible end of partial phase";

            if (localCirc.TotalEnd?.LunarAltitude > 0)
                localVisibility = "visible end of total phase";

            return localVisibility;
        }

        /// <summary>
        /// Calculates solar and lunar positions at five uniformly spaced times 
        /// over a specified period centered at t0, where t0 is an integer hour of day
        /// nearest to the specified time instant of eclipse maximum.
        /// </summary>
        /// <param name="jdMaximum">Instant of eclipse maximum.</param>
        /// <param name="period">Period, in hours.</param>
        /// <returns></returns>
        private SunMoonPosition[] GetSunMoonPositions(double jdMaximum, double period)
        {
            // found t0 (nearest integer hour closest to the eclipse maximum)
            double t0;
            Date d = new Date(jdMaximum);
            if (d.Minute < 30)
                t0 = jdMaximum - new TimeSpan(0, d.Minute, d.Second).TotalDays;
            else
                t0 = jdMaximum - new TimeSpan(0, d.Minute, d.Second).TotalDays + TimeSpan.FromHours(1).TotalDays;

            // The Besselian elements are derived from a least-squares fit to elements
            // calculated at five uniformly spaced times over a period centered at t0.
            SunMoonPosition[] pos = new SunMoonPosition[5];

            double dt = TimeSpan.FromHours(period / 2).TotalDays;
            double step = TimeSpan.FromHours(period / 4).TotalDays;

            string[] ephemerides = new[] { "Equatorial0.Alpha", "Equatorial0.Delta", "Distance" };

            var sunEphem = sky.GetEphemerides(sun, t0 - dt, t0 + dt + 1e-6, step, ephemerides);
            var moonEphem = sky.GetEphemerides(moon, t0 - dt, t0 + dt + 1e-6, step, ephemerides);

            // astronomical unit, in km
            const double AU = 149597870;

            // earth radius, in km
            const double EARTH_RADIUS = 6371;

            for (int i = 0; i < 5; i++)
            {
                pos[i] = new SunMoonPosition()
                {
                    JulianDay = t0 + step * (i - 2),
                    Sun = new CrdsEquatorial(sunEphem[i].GetValue<double>("Equatorial0.Alpha"), sunEphem[i].GetValue<double>("Equatorial0.Delta")),
                    Moon = new CrdsEquatorial(moonEphem[i].GetValue<double>("Equatorial0.Alpha"), moonEphem[i].GetValue<double>("Equatorial0.Delta")),
                    DistanceSun = sunEphem[i].GetValue<double>("Distance") * AU / EARTH_RADIUS,
                    DistanceMoon = moonEphem[i].GetValue<double>("Distance") / EARTH_RADIUS
                };
            }

            return pos;
        }

        /// <inheritdoc/>
        public PolynomialBesselianElements GetBesselianElements(double jdMaximum)
        {
            return SolarEclipses.BesselianElements(jdMaximum, GetSunMoonPositions(jdMaximum, 12));
        }

        /// <inheritdoc/>
        public PolynomialLunarEclipseElements GetLunarEclipseElements(double jdMaximum)
        {
            return LunarEclipses.BesselianElements(jdMaximum, GetSunMoonPositions(jdMaximum, 4));
        }

        /// <inheritdoc/>
        public ICollection<SolarEclipseLocalCircumstances> FindCitiesOnCentralLine(PolynomialBesselianElements be, ICollection<CrdsGeographical> centralLine, CancellationToken? cancelToken = null, IProgress<double> progress = null)
        {
            var cities = new List<CrdsGeographical>();

            for (int i = 0; i < centralLine.Count - 1; i++)
            {
                // Report progess
                progress?.Report((double)(i + 1) / centralLine.Count * 100);

                // Exit loop if cancel requested
                if (cancelToken?.IsCancellationRequested == true)
                    return new SolarEclipseLocalCircumstances[0];

                // 2 successive points create a central line segment
                var g0 = centralLine.ElementAt(i);
                var g1 = centralLine.ElementAt(i + 1);

                // Segment length, distance between 2 points on central line, in kilometers
                var length = g0.DistanceTo(g1);

                // Local circumstances at point "g0"
                var local = SolarEclipses.LocalCircumstances(be, g0);

                // Lunar umbra radius, in kilometers
                float r = (float)(local.PathWidth / 2);

                if (r > 0)
                {
                    // Count of parts the segment should be splitted 
                    int parts = (int)Math.Round(length / r);

                    // If the segment should be splitted
                    if (parts > 1)
                    {
                        // Find intermediate points and add closest cities 
                        for (int j = 0; j < parts; j++)
                        {
                            var g = Angle.Intermediate(g0, g1, (float)j / parts);
                            cities.AddRange(locationsManager.Search(g, r));
                        }
                    }
                    // The segment should not be splitted, add closest cities to the first point
                    else
                    {
                        cities.AddRange(locationsManager.Search(g0, r));
                    }
                }
            }

            return FindLocalCircumstancesForCities(be, cities, cancelToken, progress);
        }

        /// <inheritdoc/>
        public ICollection<SolarEclipseLocalCircumstances> FindLocalCircumstancesForCities(PolynomialBesselianElements be, ICollection<CrdsGeographical> cities, CancellationToken? cancelToken = null, IProgress<double> progress = null)
        {
            return cities
                .Distinct()
                .Select(c => SolarEclipses.LocalCircumstances(be, c))
                .ToArray();
        }

        /// <inheritdoc/>
        public ICollection<LunarEclipseLocalCircumstances> FindLocalCircumstancesForCities(LunarEclipse e, PolynomialLunarEclipseElements be, ICollection<CrdsGeographical> cities, CancellationToken? cancelToken = null, IProgress<double> progress = null)
        {
            return cities
                .Distinct()
                .Select(c => LunarEclipses.LocalCircumstances(e, be, c))
                .ToArray();
        }
    }
}
