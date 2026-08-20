using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Astrarium.Plugins.SolarSystem
{
    [Singleton]
    public class LunarCalendar
    {
        private readonly ISky sky;
        private readonly LunarCalc lunarCalc;
        private readonly SolarCalc solarCalc;

        private readonly string[] ephemerides = new string[] { "Constellation", "RTS.Rise", "RTS.Transit", "RTS.Set", "RTS.Duration", "RTS.RiseAzimuth", "RTS.TransitAltitude", "RTS.SetAzimuth", "Equatorial.Alpha", "Equatorial.Delta", "Horizontal.Altitude", "Horizontal.Azimuth", "Ecliptical.Lambda", "Ecliptical.Beta", "Phase", "PhaseAngle", "Age", "Lunation", "Magnitude", "Distance", "HorizontalParallax", "AngularDiameter", "Libration.Latitude", "Libration.Longitude" };

        public LunarCalendar(ISky sky, LunarCalc lunarCalc, SolarCalc solarCalc) 
        {
            this.sky = sky;
            this.lunarCalc = lunarCalc;
            this.solarCalc = solarCalc;
        }

        public Task<LunarDay> Calculate(double jdMidnight, double timeOfDay, CrdsGeographical geoLocation)
        {
            return Task.Run(() =>
            {
                var ctx = new SkyContext(jdMidnight + timeOfDay, geoLocation, preferFast: false);

                var dayInfo = new LunarDay();
                dayInfo.JdMidnight = jdMidnight;
                dayInfo.Sun = ctx.Get(solarCalc.RiseTransitSet);
                dayInfo.Moon = ctx.Get(lunarCalc.RiseTransitSet);
                dayInfo.Illumination = Math.Sign(ctx.Get(lunarCalc.Elongation)) * ctx.Get(lunarCalc.Phase);
                dayInfo.PositionAngle = ctx.Get(lunarCalc.PAaxis);
                dayInfo.Ephemerides = sky.GetEphemerides(lunarCalc.Moon, ctx, ephemerides);

                ctx = new SkyContext(jdMidnight + 0.5, geoLocation, preferFast: false);
                dayInfo.SiderealTime = ctx.SiderealTime;
                dayInfo.SunCoordinates = ctx.Get(sky.SunEquatorial);

                return dayInfo;
            });
        }

        public Task<IReadOnlyCollection<LunarDay>> Calculate(Date date, CrdsGeographical geoLocation)
        {
            return Task.Run(() =>
            {
                // starting date
                double jd0 = date.ToJulianEphemerisDay();

                // day of week for first date
                var dayOfWeek = Date.DayOfWeek(jd0);

                // number of days in month
                int daysInMonth = Date.DaysInMonth(date.Year, date.Month);

                var calendar = new List<LunarDay>();

                for (int i = 0; i < daysInMonth; i++)
                {
                    // mid day
                    double jd = jd0 + i + 0.5;

                    var ctx = new SkyContext(jd, geoLocation, preferFast: true);

                    var dayInfo = new LunarDay();

                    dayInfo.JdMidnight = jd0 + i;
                    dayInfo.DayOfMonth = i + 1;
                    dayInfo.Sun = ctx.Get(solarCalc.RiseTransitSet);
                    dayInfo.Moon = ctx.Get(lunarCalc.RiseTransitSet);

                    if (!dayInfo.Moon.Transit.Equals(RTS.None))
                    {
                        // adjust time instant for Moon transit
                        ctx.JulianDay = jd0 + i + dayInfo.Moon.Transit;
                    }

                    dayInfo.Illumination = Math.Sign(ctx.Get(lunarCalc.Elongation)) * ctx.Get(lunarCalc.Phase);
                    dayInfo.PositionAngle = ctx.Get(lunarCalc.PAaxis);

                    foreach (var phase in new[] { MoonPhase.NewMoon, MoonPhase.FirstQuarter, MoonPhase.FullMoon, MoonPhase.LastQuarter })
                    {
                        Date instant = ctx.Get(lunarCalc.NearestPhase, phase);
                        if ((int)instant.Day == dayInfo.DayOfMonth &&
                            instant.Month == date.Month)
                        {
                            dayInfo.Phase = phase;
                            dayInfo.PhaseInstant = instant;
                            dayInfo.Illumination = Math.Round(dayInfo.Illumination * 2) / 2;
                        }
                    }

                    calendar.Add(dayInfo);
                }

                if (calendar.Count(x => Math.Cos(Angle.ToRadians(x.Moon.TransitAzimuth)) < 0) > calendar.Count() / 2)
                {
                    calendar.ForEach(x =>
                    {
                        x.PositionAngle += 180;
                    });
                }

                return (IReadOnlyCollection<LunarDay>)calendar.AsReadOnly();
            });
        }
    }
}
