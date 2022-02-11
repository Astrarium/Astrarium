using Astrarium.Types;
using Astrarium.Algorithms;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;
using System;

namespace Astrarium.Plugins.Meteors
{
    public class MeteorsCalculator : BaseCalc, ICelestialObjectCalc<Meteor>
    {
        private readonly ISky Sky;
        private CelestialObject Moon;
        private ICollection<Meteor> Meteors;

        public MeteorsCalculator(ISky sky)
        {
            Sky = sky;
        }

        public IEnumerable<Meteor> GetCelestialObjects() => Meteors;

        public override void Initialize()
        {
            string file = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data/Meteors.dat");
            Meteors = new MeteorsReader().Read(file);
        }

        public override void Calculate(SkyContext ctx)
        {
            foreach (var meteor in Meteors)
            {
                meteor.IsActive = ctx.Get(IsActive, meteor);
                meteor.Horizontal = ctx.Get(Horizontal, meteor);
            }
        }

        private int DayOfYear(SkyContext ctx)
        {
            var date = new Date(ctx.JulianDay);
            return Date.DayOfYear(date);
        }

        private bool IsActive(SkyContext ctx, Meteor m)
        {
            int dayOfYear = ctx.Get(DayOfYear);
            return m.Begin <= dayOfYear && dayOfYear <= m.End;
        }

        private CrdsEquatorial Equatorial(SkyContext ctx, Meteor m)
        {
            int days = 0;
            int dayOfYear = ctx.Get(DayOfYear);
            if (ctx.Get(IsActive, m))
            {
                days = dayOfYear - (m.Max > 0 ? m.Max : 365 + m.Max);
            }
            else if (days < 0)
            {
                days = m.Begin - (m.Max > 0 ? m.Max : 365 + m.Max);
            }
            else
            {
                days = m.End - (m.Max > 0 ? m.Max : 365 + m.Max);
            }

            return new CrdsEquatorial(m.Equatorial0.Alpha + m.Drift.Alpha * days, m.Equatorial0.Delta + m.Drift.Delta * days);
        }

        private CrdsHorizontal Horizontal(SkyContext ctx, Meteor m)
        {
            return ctx.Get(Equatorial, m).ToHorizontal(ctx.GeoLocation, ctx.SiderealTime);
        }

        private RTS RiseTransitSet(SkyContext ctx, Meteor m)
        {
            double theta0 = Date.ApparentSiderealTime(ctx.JulianDayMidnight, ctx.NutationElements.deltaPsi, ctx.Epsilon);
            var eq = ctx.Get(Equatorial, m);
            return Visibility.RiseTransitSet(eq, ctx.GeoLocation, theta0);
        }

        public double LunarPhaseAtMax(SkyContext ctx, Meteor m)
        {
            if (Moon == null)
            {
                Moon = Sky.Search("Moon");
            }
            return Moon != null ? (double)Sky.GetEphemerides(Moon, ctx, new[] { "Phase" }).First().Value : 0;
        }

        public void ConfigureEphemeris(EphemerisConfig<Meteor> e)
        {
            e["Equatorial.Alpha"] = (c, m) => c.Get(Equatorial, m).Alpha;
            e["Equatorial.Delta"] = (c, m) => c.Get(Equatorial, m).Delta;
            e["Horizontal.Altitude"] = (c, m) => c.Get(Horizontal, m).Altitude;
            e["Horizontal.Azimuth"] = (c, m) => c.Get(Horizontal, m).Azimuth;
            e["RTS.Rise"] = (c, m) => c.GetDateFromTime(c.Get(RiseTransitSet, m).Rise);
            e["RTS.Transit"] = (c, m) => c.GetDateFromTime(c.Get(RiseTransitSet, m).Transit);
            e["RTS.Set"] = (c, m) => c.GetDateFromTime(c.Get(RiseTransitSet, m).Set);
            e["RTS.Duration"] = (c, m) => c.Get(RiseTransitSet, m).Duration;
        }

        public void GetInfo(CelestialObjectInfo<Meteor> info)
        {
            Meteor m = info.Body;
            SkyContext c = info.Context;

            string constellation = Constellations.FindConstellation(c.Get(Equatorial, m), c.JulianDay);
            int year = c.GetDate(c.JulianDay).Year;
            var offset = c.GeoLocation.UtcOffset;
            var jd0 = Date.DeltaT(c.JulianDay) / 86400.0 + Date.JulianDay0(year) - offset / 24;
            var begin = new Date(jd0 + m.Begin, offset);
            var max = new Date(jd0 + m.Max, offset);
            var end = new Date(jd0 + m.End, offset);
            SkyContext cMax = new SkyContext(jd0 + m.Max, c.GeoLocation, c.PreferFastCalculation);
            var phase = LunarPhaseAtMax(cMax, m);

            info
            .SetTitle(string.Join(", ", info.Body.Names))
            .SetSubtitle(Text.Get("Meteor.Type"))
            .AddRow("Constellation", constellation)

            .AddHeader(Text.Get("Meteor.Equatorial"))
            .AddRow("Equatorial.Alpha")
            .AddRow("Equatorial.Delta")

            .AddHeader(Text.Get("Meteor.Horizontal"))
            .AddRow("Horizontal.Azimuth")
            .AddRow("Horizontal.Altitude")

            .AddHeader(Text.Get("Meteor.RTS"))
            .AddRow("RTS.Rise")
            .AddRow("RTS.Transit")
            .AddRow("RTS.Set")
            .AddRow("RTS.Duration")

            .AddHeader(Text.Get("Meteor.Activity"))
            .AddRow("Activity.Begin", begin, Formatters.Date)
            .AddRow("Activity.Max", max, Formatters.Date)
            .AddRow("Activity.End", end, Formatters.Date)
            .AddRow("Activity.LunarPhaseAtMax", phase, Formatters.Phase)

            .AddHeader(Text.Get("Meteor.Data"))
            .AddRow("Data.ZHR", m.ZHR)
            .AddRow("Data.ActivityClass", m.ActivityClass)
            .AddRow("Data.DriftRA", m.Drift.Alpha, Formatters.Angle)
            .AddRow("Data.DriftDec", m.Drift.Delta, Formatters.Angle);
        }

        public ICollection<CelestialObject> Search(SkyContext context, string searchString, Func<CelestialObject, bool> filterFunc, int maxCount = 50)
        {
            return Meteors
                .Where(m => m.Names.Any(n => n.StartsWith(searchString, StringComparison.OrdinalIgnoreCase)))
                .Where(filterFunc)
                .Take(maxCount)
                .ToArray();
        }
    }
}
