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
        private readonly LunarCalc lunarCalc;
        private readonly SolarCalc solarCalc;

        public LunarCalendar(LunarCalc lunarCalc, SolarCalc solarCalc) 
        {
            this.lunarCalc = lunarCalc;
            this.solarCalc = solarCalc;
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
                            dayInfo.PhaseText = Text.Get($"Moon.Phases.{phase}");
                            dayInfo.Phase = phase;
                            dayInfo.PhaseInstant = instant.ToJulianEphemerisDay();
                            dayInfo.PhaseInstantString = instant.ToDateTime().ToString("HH:mm");
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
