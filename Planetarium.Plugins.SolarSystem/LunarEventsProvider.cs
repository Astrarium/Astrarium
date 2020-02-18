using ADK;
using Planetarium.Types;
using Planetarium.Types.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.Plugins.SolarSystem
{
    public class LunarEventsProvider : BaseAstroEventsProvider
    {
        private readonly LunarCalc lunarCalc = null;
        private readonly PlanetsCalc planetsCalc = null;
        
        private readonly LibrationLongitudeFormatter librationLongitudeFormatter = new LibrationLongitudeFormatter();
        private readonly LibrationLatitudeFormatter librationLatitudeFormatter = new LibrationLatitudeFormatter();
        private readonly Formatters.SignedDoubleFormatter declinationFormatter = new Formatters.SignedDoubleFormatter(3, "\u00B0");

        // Bright stars which can be in conjunction with Moon
        private readonly ConjunctedStar[] stars = new ConjunctedStar[]
        {
            new ConjunctedStar("Pleiades", "03h 47m 29.1s", "+24° 06' 18''", 0.019f, -0.046f),
            new ConjunctedStar("Aldebaran", "04h 35m 55.2s", "+16° 30' 33''", 0.063f, -0.19f),
            new ConjunctedStar("Pollux", "07h 45m 19.4s", "+28° 01' 35''", -0.628f, -0.046f),
            new ConjunctedStar("Regul", "10h 08m 22.3s", "+11° 58' 02''", -0.248f, 0.006f),
            new ConjunctedStar("Spica", "13h 25m 11.6s", "-11° 09' 41''", -0.041f, -0.028f),
            new ConjunctedStar("Antares", "16h 29m 24.4s", "-26° 25' 55''", 0.01f, -0.02f)
        };

        public LunarEventsProvider(LunarCalc lunarCalc, PlanetsCalc planetsCalc)
        {
            this.lunarCalc = lunarCalc;
            this.planetsCalc = planetsCalc;
        }

        public override void ConfigureAstroEvents(AstroEventsConfig c)
        {
            c["MoonEvents.Phases"] = Phases;
            c["MoonEvents.Apsis"] = Apsis;
            c["MoonEvents.Librations"] = MaxLibrations;
            c["MoonEvents.MaxDeclinations"] = MaxDeclinations;
            c["MoonEvents.ConjWithStars"] = ConjunctionsWithStars;
            c["MoonEvents.ConjWithPlanets"] = ConjuntionsWithPlanets;
        }

        /// <summary>
        /// Calculates dates of lunar phases within specified range
        /// </summary>
        private ICollection<AstroEvent> Phases(AstroEventsContext context)
        {
            List<AstroEvent> events = new List<AstroEvent>();
            double jd = 0;

            jd = context.From;
            while (jd < context.To)
            {
                jd = LunarEphem.NearestPhase(jd, MoonPhase.NewMoon);
                events.Add(new AstroEvent(jd, Text.Get("MoonEvents.Phases.NewMoon"), lunarCalc.Moon));
                jd += LunarEphem.SINODIC_PERIOD;
            }

            jd = context.From;
            while (jd < context.To)
            {
                jd = LunarEphem.NearestPhase(jd, MoonPhase.FirstQuarter);
                events.Add(new AstroEvent(jd, Text.Get("MoonEvents.Phases.FirstQuarter"), lunarCalc.Moon));
                jd += LunarEphem.SINODIC_PERIOD;
            }

            jd = context.From;
            while (jd < context.To)
            {
                jd = LunarEphem.NearestPhase(jd, MoonPhase.FullMoon);
                events.Add(new AstroEvent(jd, Text.Get("MoonEvents.Phases.FullMoon"), lunarCalc.Moon));
                jd += LunarEphem.SINODIC_PERIOD;
            }

            jd = context.From;
            while (jd < context.To)
            {
                jd = LunarEphem.NearestPhase(jd, MoonPhase.LastQuarter);
                events.Add(new AstroEvent(jd, Text.Get("MoonEvents.Phases.LastQuarter"), lunarCalc.Moon));
                jd += LunarEphem.SINODIC_PERIOD;
            }

            return events;
        }

        /// <summary>
        /// Calculates dates of perigees and apogees within specified range
        /// </summary>
        private ICollection<AstroEvent> Apsis(AstroEventsContext context)
        {
            List<AstroEvent> events = new List<AstroEvent>();
            double jd, diameter;

            jd = context.From;
            while (jd < context.To)
            {
                jd = LunarEphem.NearestApsis(jd, MoonApsis.Apogee, out diameter);
                events.Add(new AstroEvent(jd, Text.Get("MoonEvents.Apsis.Apogee", ("diameter", Formatters.AngularDiameter.Format(diameter)))));
                jd += LunarEphem.ANOMALISTIC_PERIOD;
            }

            jd = context.From;
            while (jd < context.To)
            {
                jd = LunarEphem.NearestApsis(jd, MoonApsis.Perigee, out diameter);
                events.Add(new AstroEvent(jd, Text.Get("MoonEvents.Apsis.Perigee", ("diameter", Formatters.AngularDiameter.Format(diameter)))));
                jd += LunarEphem.ANOMALISTIC_PERIOD * 1.1;
            }

            return events;
        }

        /// <summary>
        /// Calculates dates of maximal librations within specified range
        /// </summary>
        private ICollection<AstroEvent> MaxLibrations(AstroEventsContext context)
        {
            List<AstroEvent> events = new List<AstroEvent>();
            double jd, librationAngle;

            jd = context.From;
            while (jd < context.To)
            {
                jd = LunarEphem.NearestMaxLibration(jd, LibrationEdge.East, out librationAngle);
                events.Add(new AstroEvent(jd, Text.Get("MoonEvents.Librations.East", ("angle", librationLongitudeFormatter.Format(librationAngle)))));
                jd += LunarEphem.ANOMALISTIC_PERIOD;
            }

            jd = context.From;
            while (jd < context.To)
            {
                jd = LunarEphem.NearestMaxLibration(jd, LibrationEdge.West, out librationAngle);
                events.Add(new AstroEvent(jd, Text.Get("MoonEvents.Librations.West", ("angle", librationLongitudeFormatter.Format(librationAngle)))));
                jd += LunarEphem.ANOMALISTIC_PERIOD;
            }

            jd = context.From;
            while (jd < context.To)
            {
                jd = LunarEphem.NearestMaxLibration(jd, LibrationEdge.North, out librationAngle);
                events.Add(new AstroEvent(jd, Text.Get("MoonEvents.Librations.North", ("angle", librationLatitudeFormatter.Format(librationAngle)))));
                jd += LunarEphem.DRACONIC_PERIOD;
            }

            jd = context.From;
            while (jd < context.To)
            {
                jd = LunarEphem.NearestMaxLibration(jd, LibrationEdge.South, out librationAngle);
                events.Add(new AstroEvent(jd, Text.Get("MoonEvents.Librations.South", ("angle", librationLatitudeFormatter.Format(librationAngle)))));
                jd += LunarEphem.DRACONIC_PERIOD;
            }

            return events;
        }

        /// <summary>
        /// Calculates dates of maximal declinations of the Moon within specified range
        /// </summary>
        private ICollection<AstroEvent> MaxDeclinations(AstroEventsContext context)
        {
            List<AstroEvent> events = new List<AstroEvent>();

            double jd = context.From;
            double delta;

            while (jd < context.To)
            {
                jd = LunarEphem.NearestMaxDeclination(jd, MoonDeclination.North, out delta);
                events.Add(new AstroEvent(jd, Text.Get("MoonEvents.MaxDeclinations.North", ("declination", declinationFormatter.Format(delta)))));
                jd += LunarEphem.DRACONIC_PERIOD;
            }

            jd = context.From;
            while (jd < context.To)
            {
                jd = LunarEphem.NearestMaxDeclination(jd, MoonDeclination.South, out delta);
                events.Add(new AstroEvent(jd, Text.Get("MoonEvents.MaxDeclinations.South", ("declination", declinationFormatter.Format(-delta)))));
                jd += LunarEphem.DRACONIC_PERIOD;
            }

            return events;
        }

        /// <summary>
        /// Calculates dates of conjunctions and occultations of stars by Moon
        /// </summary>
        private ICollection<AstroEvent> ConjunctionsWithStars(AstroEventsContext context)
        {
            List<AstroEvent> events = new List<AstroEvent>();

            foreach (ConjunctedStar star in stars)
            {
                SkyContext ctx = new SkyContext(context.From, context.GeoLocation);
                                
                double jd = context.From;
                while (jd < context.To)
                {
                    ctx.JulianDay = jd;

                    jd = NearestPassWithStar(ctx, star);

                    CrdsEquatorial eqMoon = ctx.Get(lunarCalc.Equatorial);
                    CrdsEquatorial eqStar = ctx.Get(StarEquatorial, star);

                    double semidiameter = ctx.Get(lunarCalc.Semidiameter) / 3600;
                    double separation = Angle.Separation(eqMoon, eqStar);

                    // occultation
                    if (semidiameter >= separation)
                    {
                        events.Add(new AstroEvent(jd, Text.Get("MoonEvents.ConjWithStars.Occults", ("starName", star.Name))));
                    }
                    // conjunction
                    else
                    {
                        string direction = eqMoon.Delta > eqStar.Delta ? Text.Get("MoonEvents.ConjWithStars.Conj.North") : Text.Get("MoonEvents.ConjWithStars.Conj.South");
                        events.Add(new AstroEvent(jd, Text.Get("MoonEvents.ConjWithStars.Conj", ("angularDistance", Formatters.ConjunctionSeparation.Format(separation)), ("direction", direction), ("starName", star.Name))));
                    }

                    jd += LunarEphem.SIDEREAL_PERIOD;
                }
            }

            return events;
        }

        private ICollection<AstroEvent> ConjuntionsWithPlanets(AstroEventsContext context)
        {
            List<AstroEvent> events = new List<AstroEvent>();
           
            for (int p = 1; p <= 6; p++)
            {
                if (p != 3)
                {
                    SkyContext ctx = new SkyContext(context.From, context.GeoLocation, true);
                    
                    double jd = context.From;
                    while (jd < context.To)
                    {
                        ctx.JulianDay = jd;

                        jd = NearestPassWithPlanet(ctx, p);

                        CrdsEquatorial eqMoon = ctx.Get(lunarCalc.Equatorial);
                        CrdsEquatorial eqPlanet = ctx.Get(planetsCalc.Planet_Equatorial, p);

                        double semidiameter = ctx.Get(lunarCalc.Semidiameter) / 3600;
                        double separation = Angle.Separation(eqMoon, eqPlanet);
                        string planetName = Text.Get($"Planet.{p}.GenitiveName");

                        // occultation
                        if (semidiameter >= separation)
                        {
                            events.Add(new AstroEvent(jd, Text.Get("MoonEvents.ConjWithPlanets.Occults", ("planetName", planetName))));
                        }
                        // conjunction
                        else
                        {
                            string moonPhase = Formatters.Phase.Format(ctx.Get(lunarCalc.Phase));
                            string planetMagnitude = Formatters.Magnitude.Format(ctx.Get(planetsCalc.Planet_Magnitude, p));
                            string angularDistance = Formatters.ConjunctionSeparation.Format(separation);
                            string direction = eqMoon.Delta > eqPlanet.Delta ? Text.Get("MoonEvents.ConjWithPlanets.Conj.North") : Text.Get("MoonEvents.ConjWithPlanets.Conj.South");
                            events.Add(new AstroEvent(jd, Text.Get("MoonEvents.ConjWithPlanets.Conj", ("moonPhase", moonPhase), ("angularDistance", angularDistance), ("direction", direction), ("planetName", planetName), ("planetMagnitude", planetMagnitude))));
                        }

                        jd += LunarEphem.SIDEREAL_PERIOD;
                    }
                }
            }

            return events;
        }

        private double NearestPassWithStar(SkyContext ctx, ConjunctedStar star)
        {
            double minute = TimeSpan.FromMinutes(1).TotalDays;
            double days = double.MaxValue;
            while (Math.Abs(days) > minute)
            {
                CrdsEquatorial eqMoon = ctx.Get(lunarCalc.Equatorial);
                CrdsEquatorial eqStar = ctx.Get(StarEquatorial, star);

                double[] alpha = new[] { eqMoon.Alpha, eqStar.Alpha };
                Angle.Align(alpha);

                days = (alpha[1] - alpha[0]) / LunarEphem.AVERAGE_DAILY_MOTION;

                ctx.JulianDay += days;
            }

            return ctx.JulianDay;
        }

        private double NearestPassWithPlanet(SkyContext ctx, int planet)
        {
            double minute = TimeSpan.FromMinutes(1).TotalDays;
            double days = double.MaxValue;
            while (Math.Abs(days) > minute)
            {
                CrdsEquatorial eqMoon = ctx.Get(lunarCalc.Equatorial);
                CrdsEquatorial eqPlanet = ctx.Get(planetsCalc.Planet_Equatorial, planet);

                double[] alpha = new[] { eqMoon.Alpha, eqPlanet.Alpha };
                Angle.Align(alpha);

                days = (alpha[1] - alpha[0]) / LunarEphem.AVERAGE_DAILY_MOTION;

                ctx.JulianDay += days;
            }

            return ctx.JulianDay;
        }

        private PrecessionalElements PrecessionalElements(SkyContext c)
        {
            return Precession.ElementsFK5(Date.EPOCH_J2000, c.JulianDay);
        }

        private CrdsEquatorial StarEquatorial(SkyContext c, ConjunctedStar star)
        {
            PrecessionalElements p = c.Get(PrecessionalElements);

            // Number of years, with fractions, since J2000 epoch
            double years = (c.JulianDay - Date.EPOCH_J2000) / 365.25;

            // Initial coodinates for J2000 epoch
            CrdsEquatorial eq0 = new CrdsEquatorial(star.Equatorial0);

            // Take into account effect of proper motion:
            // now coordinates are for the mean equinox of J2000.0,
            // but for epoch of the target date
            eq0.Alpha += star.PmAlpha * years / 3600.0;
            eq0.Delta += star.PmDelta * years / 3600.0;

            // Equatorial coordinates for the mean equinox and epoch of the target date
            // without aberration and nutation corrections
            return Precession.GetEquatorialCoordinates(eq0, p);
        }

        private class ConjunctedStar
        {
            private string name;
            public string Name => Text.Get($"MoonEvents.ConjWithStars.Star.{name}");
            public CrdsEquatorial Equatorial0 { get; private set; }
            public float PmAlpha { get; private set; }
            public float PmDelta { get; private set; }
            public ConjunctedStar(string name, string ra, string dec, float pmAlpha, float pmDelta)
            {
                this.name = name;
                Equatorial0 = new CrdsEquatorial(new HMS(ra), new DMS(dec));
                PmAlpha = pmAlpha;
                PmDelta = pmDelta;
            }
        }
    }
}
