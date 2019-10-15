using ADK;
using Planetarium.Calculators;
using Planetarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.Plugins.SolarSystem
{
    public class DaylightCalc : BaseCalc
    {
        private readonly SolarCalc solarCalc;
        private readonly LunarCalc lunarCalc;

        public DaylightCalc(SolarCalc solarCalc, LunarCalc lunarCalc)
        {
            this.solarCalc = solarCalc;
            this.lunarCalc = lunarCalc;
        }

        public override void Calculate(SkyContext context)
        {
            context.DayLightFactor = context.Get(DaylightFactor);
        }

        private float DaylightFactor(SkyContext c)
        {
            var hSun = c.Get(solarCalc.Horizontal);
            
            double alt = hSun.Altitude;

            if (alt >= 0)
            {
                var hMoon = c.Get(lunarCalc.Horizontal);
                var sdSun = c.Get(solarCalc.Semidiameter);
                var sdMoon = c.Get(lunarCalc.Semidiameter);

                // Angular separation between Sun and Moon disks, in arcseconds
                double delta = Angle.Separation(hSun, hMoon) * 3600.0;

                if (delta < Math.Abs(sdSun - sdMoon))
                {
                    return 0;
                }

                // Solar eclipse (disks are overlapping)
                if (delta <= sdSun + sdMoon)
                {
                    // find overlapping area of two circles
                    // (https://abakbot.ru/online-2/73-ploshhad-peresecheniya-okruzhnostej)

                    double r1 = sdSun;
                    double r2 = sdMoon;
                    double f1 = 2 * Math.Acos((r1 * r1 - r2 * r2 + delta * delta) / (2 * r1 * delta));
                    double f2 = 2 * Math.Acos((r2 * r2 - r1 * r1 + delta * delta) / (2 * r2 * delta));

                    double s1 = r1 * r1 * Math.Sin(f1 - Math.Sin(f1)) / 2;
                    double s2 = r2 * r2 * Math.Sin(f2 - Math.Sin(f2)) / 2;

                    // area of overlapping area
                    double s = s1 + s2;

                    // area of Sun disk
                    double ss = Math.PI * r1 * r1;

                    double percentage = Math.Abs(s / ss);

                    if (percentage <= 0.1)
                    {
                        return (float)(percentage / 0.1);
                    }
                }
            }

            // Absolute value of solar altitude 
            // at the end of Nautical twilight / beginning of Astronomical twilight
            const double nightAlt = 12;

            if (alt >= 0)
                return 1;
            else if (alt < 0 && alt > -nightAlt)
                return 1 - (float)(-alt / nightAlt);
            else
                return 0;
        }
    }
}
