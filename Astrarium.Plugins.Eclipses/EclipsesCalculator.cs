using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Eclipses
{
    public class EclipsesCalculator : BaseAstroEventsProvider, IEclipsesCalculator
    {
        private readonly ISky sky;
        private CelestialObject sun;
        private CelestialObject moon;

        public EclipsesCalculator(ISky sky)
        {
            this.sky = sky;
            this.sky.Calculated += Sky_Calculated;
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
            double jd = context.From;
            do
            {
                SolarEclipse eclipse = SolarEclipses.NearestEclipse(jd, next: true);
                jd = eclipse.JulianDayMaximum;
                if (jd <= context.To)
                {
                    var pbe = GetBesselianElements(jd);
                    var localCirc = SolarEclipses.LocalCircumstances(pbe, context.GeoLocation);

                    string type = eclipse.EclipseType.ToString();
                    string subtype = eclipse.IsNonCentral ? " non-central" : "";
                    string phase = eclipse.EclipseType == SolarEclipseType.Partial ? $" (max phase {Formatters.Phase.Format(eclipse.Phase)})" : "";
                    double jdMax = jd;

                    string localVisibility = "invisible from current place";
                    if (localCirc.MaxMagnitude > 0)
                    {
                        jdMax = localCirc.JulianDayMax;
                        string localMag = Formatters.Phase.Format(localCirc.MaxMagnitude);
                        string asPartial = eclipse.EclipseType != SolarEclipseType.Partial ? " as partial" : "";
                        
                        // max instant not visible
                        if (localCirc.SunAltMax <= 0)
                        {
                            if (localCirc.SunAltTotalEnd < 0 || localCirc.SunAltPartialEnd < 0)
                                localVisibility = $"visible{asPartial} on sunset from current place (max phase {localMag})";
                            else if (localCirc.SunAltPartialBegin < 0 || localCirc.SunAltTotalBegin < 0)
                                localVisibility = $"visible{asPartial} on sunrise from current place (max phase {localMag})";
                        }
                        // max instant visible
                        else
                        {
                            if (localCirc.TotalDuration > 0)
                                localVisibility = "completely visible from current place";
                            else
                                localVisibility = $"visible{asPartial} from current place (max phase {localMag})";
                        }
                    }    

                    events.Add(new AstroEvent(jdMax, $"{type}{subtype} solar eclipse{phase}, {localVisibility}.", sun, moon));
                    jd += LunarEphem.SINODIC_PERIOD;
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
            double jd = context.From;
            do
            {
                LunarEclipse eclipse = LunarEclipses.NearestEclipse(jd, next: true);
                jd = eclipse.JulianDayMaximum;
                if (jd <= context.To)
                {
                    double jdMax = jd;
                    string type = eclipse.EclipseType.ToString();
                    string phase = Formatters.Phase.Format(eclipse.Magnitude);
                    // TODO: local circumstances

                    string[] ephemerides = new[] { "Horizontal.Altitude" };
                    double[] jdInstants = new double[7] 
                    {
                        eclipse.JulianDayFirstContactPenumbra,
                        eclipse.JulianDayFirstContactUmbra,
                        eclipse.JulianDayTotalBegin,
                        eclipse.JulianDayMaximum,
                        eclipse.JulianDayTotalEnd,
                        eclipse.JulianDayLastContactUmbra,
                        eclipse.JulianDayLastContactPenumbra
                    };

                    double[] altitudes = new double[7];

                    for (int i = 0; i < 7; i++)
                    {
                        if (!double.IsNaN(jdInstants[i]))
                        {
                            var ctx = new SkyContext(jdInstants[i], context.GeoLocation);
                            var moonEphem = sky.GetEphemerides(moon, ctx, ephemerides);
                            altitudes[i] = (double)moonEphem[0].Value;
                        }
                    }

                    events.Add(new AstroEvent(jdMax, $"{type} lunar eclipse (magnitude {phase}).", moon));
                    jd += LunarEphem.SINODIC_PERIOD;
                }
                else
                {
                    break;
                }
            }
            while (true);
            return events;
        }

        public PolynomialBesselianElements GetBesselianElements(double jd)
        {
            // 5 measurements with 3h step, so interval is -6...+6 hours
            SunMoonPosition[] pos = new SunMoonPosition[5];

            double dt = TimeSpan.FromHours(6).TotalDays;
            double step = TimeSpan.FromHours(3).TotalDays;
            string[] ephemerides = new[] { "Equatorial0.Alpha", "Equatorial0.Delta", "Distance" };

            var sunEphem = sky.GetEphemerides(sun, jd - dt, jd + dt + 1e-6, step, ephemerides);
            var moonEphem = sky.GetEphemerides(moon, jd - dt, jd + dt + 1e-6, step, ephemerides);

            for (int i = 0; i < 5; i++)
            {
                pos[i] = new SunMoonPosition()
                {
                    JulianDay = jd + step * (i - 2),
                    Sun = new CrdsEquatorial(sunEphem[i].GetValue<double>("Equatorial0.Alpha"), sunEphem[i].GetValue<double>("Equatorial0.Delta")),
                    Moon = new CrdsEquatorial(moonEphem[i].GetValue<double>("Equatorial0.Alpha"), moonEphem[i].GetValue<double>("Equatorial0.Delta")),
                    DistanceSun = sunEphem[i].GetValue<double>("Distance") * 149597870 / 6371.0,
                    DistanceMoon = moonEphem[i].GetValue<double>("Distance") / 6371.0
                };
            }

            return SolarEclipses.BesselianElements(pos);
        }
    }
}
