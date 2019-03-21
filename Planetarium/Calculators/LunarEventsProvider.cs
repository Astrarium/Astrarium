using ADK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.Calculators
{
    public class LunarEventsProvider : BaseAstroEventsProvider
    {
        private readonly ILunarCalc lunarCalc = null;
        private readonly IStarsCalc starsCalc = null;
        private readonly IPlanetsCalc planetsCalc = null;

        // BSC catalogue numbers of brightest stars that can be in conjunction with Moon
        private readonly ushort[] starsNumbers = new ushort[]
        {
            1165, // Alcyone (Pleiades)
            1457, // Aldebaran
            2990, // Pollux
            3982, // Regul
            5056, // Spica
            6134  // Antares
        };

        public LunarEventsProvider(ILunarCalc lunarCalc, IStarsCalc starsCalc, IPlanetsCalc planetsCalc)
        {
            this.lunarCalc = lunarCalc;
            this.starsCalc = starsCalc;
            this.planetsCalc = planetsCalc;
        }

        public override void ConfigureAstroEvents(AstroEventsConfig config)
        {
            config
                .Add("Moon.Phases", Phases)
                .Add("Moon.Apsis", Apsis)
                .Add("Moon.Librations", MaxLibrations)
                .Add("Moon.MaxDeclinations", MaxDeclinations)
                .Add("Moon.ConjWithStars", ConjunctionsWithStars)
                .Add("Moon.ConjWithPlanets", ConjuntionsWithPlanets);
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
                events.Add(new AstroEvent(jd, "New Moon"));
                jd += LunarEphem.SINODIC_PERIOD;
            }

            jd = context.From;
            while (jd < context.To)
            {
                jd = LunarEphem.NearestPhase(jd, MoonPhase.FirstQuarter);
                events.Add(new AstroEvent(jd, "First Quarter"));
                jd += LunarEphem.SINODIC_PERIOD;
            }

            jd = context.From;
            while (jd < context.To)
            {
                jd = LunarEphem.NearestPhase(jd, MoonPhase.FullMoon);
                events.Add(new AstroEvent(jd, "Full Moon"));
                jd += LunarEphem.SINODIC_PERIOD;
            }

            jd = context.From;
            while (jd < context.To)
            {
                jd = LunarEphem.NearestPhase(jd, MoonPhase.LastQuarter);
                events.Add(new AstroEvent(jd, "Last Quarter"));
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
            double jd = 0;
            double diameter = 0;

            jd = context.From;
            while (jd < context.To)
            {
                jd = LunarEphem.NearestApsis(jd, MoonApsis.Apogee, out diameter);
                events.Add(new AstroEvent(jd, $"Moon at apogee ({Formatters.AngularDiameter.Format(diameter)})"));
                jd += LunarEphem.ANOMALISTIC_PERIOD;
            }

            jd = context.From;
            while (jd < context.To)
            {
                jd = LunarEphem.NearestApsis(jd, MoonApsis.Perigee, out diameter);
                events.Add(new AstroEvent(jd, $"Moon at perigee ({Formatters.AngularDiameter.Format(diameter)})"));
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
            double jd = 0;
            double librationAngle = 0;

            jd = context.From;
            while (jd < context.To)
            {
                jd = LunarEphem.NearestMaxLibration(jd, LibrationEdge.East, out librationAngle);
                events.Add(new AstroEvent(jd, $"Maximal eastern libration of the Moon ({Formatters.LibrationLongitude.Format(librationAngle)})"));
                jd += LunarEphem.ANOMALISTIC_PERIOD;
            }

            jd = context.From;
            while (jd < context.To)
            {
                jd = LunarEphem.NearestMaxLibration(jd, LibrationEdge.West, out librationAngle);
                events.Add(new AstroEvent(jd, $"Maximal western libration of the Moon ({Formatters.LibrationLongitude.Format(librationAngle)})"));
                jd += LunarEphem.ANOMALISTIC_PERIOD;
            }

            jd = context.From;
            while (jd < context.To)
            {
                jd = LunarEphem.NearestMaxLibration(jd, LibrationEdge.North, out librationAngle);
                events.Add(new AstroEvent(jd, $"Maximal northern libration of the Moon ({Formatters.LibrationLatitude.Format(librationAngle)})"));
                jd += LunarEphem.DRACONIC_PERIOD;
            }

            jd = context.From;
            while (jd < context.To)
            {
                jd = LunarEphem.NearestMaxLibration(jd, LibrationEdge.South, out librationAngle);
                events.Add(new AstroEvent(jd, $"Maximal southern libration of the Moon ({Formatters.LibrationLatitude.Format(librationAngle)})"));
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
            double jd = 0;
            double delta = 0;

            jd = context.From;
            while (jd < context.To)
            {
                jd = LunarEphem.NearestMaxDeclination(jd, MoonDeclination.North, out delta);
                events.Add(new AstroEvent(jd, $"Maximal northern declination of the Moon ({Formatters.MoonDeclination.Format(delta)})"));
                jd += LunarEphem.DRACONIC_PERIOD;
            }

            jd = context.From;
            while (jd < context.To)
            {
                jd = LunarEphem.NearestMaxDeclination(jd, MoonDeclination.South, out delta);
                events.Add(new AstroEvent(jd, $"Maximal southern declination of the Moon ({Formatters.MoonDeclination.Format(-delta)})"));
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

            foreach (ushort star in starsNumbers)
            {
                SkyContext ctx = new SkyContext(context.From, context.GeoLocation);
                
                string starName = starsCalc.GetPrimaryStarName(star);

                double jd = context.From;
                while (jd < context.To)
                {
                    ctx.JulianDay = jd;

                    jd = NearestPassWithStar(ctx, star);

                    CrdsEquatorial eqMoon = ctx.Get(lunarCalc.Equatorial);
                    CrdsEquatorial eqStar = ctx.Get(starsCalc.Equatorial, star);

                    double semidiameter = ctx.Get(lunarCalc.Semidiameter) / 3600;
                    double separation = Angle.Separation(eqMoon, eqStar);

                    // occultation
                    if (semidiameter >= separation)
                    {
                        events.Add(new AstroEvent(jd, $"Moon occults star {starName}"));
                    }
                    // conjunction
                    else
                    {
                        string direction = eqMoon.Delta > eqStar.Delta ? "north" : "south";
                        events.Add(new AstroEvent(jd, $"Moon passes {Formatters.ConjunctionSeparation.Format(separation)} {direction} to star {starName}"));
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
                    string planetName = planetsCalc.GetPlanetName(p);

                    double jd = context.From;
                    while (jd < context.To)
                    {
                        ctx.JulianDay = jd;

                        jd = NearestPassWithPlanet(ctx, p);

                        CrdsEquatorial eqMoon = ctx.Get(lunarCalc.Equatorial);
                        CrdsEquatorial eqPlanet = ctx.Get(planetsCalc.Equatorial, p);

                        double semidiameter = ctx.Get(lunarCalc.Semidiameter) / 3600;
                        double separation = Angle.Separation(eqMoon, eqPlanet);

                        // occultation
                        if (semidiameter >= separation)
                        {
                            events.Add(new AstroEvent(jd, $"Moon occults {planetName}"));
                        }
                        // conjunction
                        else
                        {
                            string phase = Formatters.Phase.Format(ctx.Get(lunarCalc.Phase));
                            string magnitude = Formatters.Magnitude.Format(ctx.Get(planetsCalc.Magnitude, p));
                            string ad = Formatters.ConjunctionSeparation.Format(separation);
                            string direction = eqMoon.Delta > eqPlanet.Delta ? "north" : "south";
                            events.Add(new AstroEvent(jd, $"Moon (Φ={phase}) passes {ad} {direction} to {planetName} ({magnitude})"));
                        }

                        jd += LunarEphem.SIDEREAL_PERIOD;
                    }
                }
            }

            return events;
        }

        private double NearestPassWithStar(SkyContext ctx, ushort star)
        {
            double minute = TimeSpan.FromMinutes(1).TotalDays;
            double days = double.MaxValue;
            while (Math.Abs(days) > minute)
            {
                CrdsEquatorial eqMoon = ctx.Get(lunarCalc.Equatorial);
                CrdsEquatorial eqStar = ctx.Get(starsCalc.Equatorial, star);

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
                CrdsEquatorial eqPlanet = ctx.Get(planetsCalc.Equatorial, planet);

                double[] alpha = new[] { eqMoon.Alpha, eqPlanet.Alpha };
                Angle.Align(alpha);

                days = (alpha[1] - alpha[0]) / LunarEphem.AVERAGE_DAILY_MOTION;

                ctx.JulianDay += days;
            }

            return ctx.JulianDay;
        }
    }
}
