using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADK.Demo.Calculators
{
    public class PlanetsEventsProvider : BaseAstroEventsProvider
    {
        private readonly IPlanetsCalc planetsCalc;

        public PlanetsEventsProvider(IPlanetsCalc planetsCalc)
        {
            this.planetsCalc = planetsCalc;
        }

        public override void ConfigureAstroEvents(AstroEventsConfig config)
        {
            config
               .Add("PlanetsMutualConjunctions", MutualConjunctions);
        }

        private ICollection<AstroEvent> MutualConjunctions(AstroEventsContext context)
        {
            List<AstroEvent> events = new List<AstroEvent>();
            SkyContext ctx = new SkyContext(context.From, context.GeoLocation, true);

            // queue of equatorial coordinates for all planets
            Queue<CrdsEquatorial[]> queue = new Queue<CrdsEquatorial[]>();

            // equatorial coordinates of planets
            CrdsEquatorial[] eq = new CrdsEquatorial[9];

            // current calculated value of Julian Day
            double jd = context.From;

            for (jd = context.From - 2; jd < context.To + 2; jd++)
            {
                ctx.JulianDay = jd;
                
                // calculate coordinates of planets
                for (int p = 1; p <= 8; p++)
                {
                    if (p != 3)
                    {                      
                        eq[p] = ctx.Get(planetsCalc.Equatorial, p);
                    }
                }

                queue.Enqueue(eq);

                // limit queue for 5 values maximum (floating window)
                if (queue.Count > 5)
                {
                    queue.Dequeue();
                }

                // if 5 points set is reached
                if (queue.Count == 5)
                {
                    // "a" is a difference in Right Ascension between two planets (5 points)
                    double[] a = new double[5];

                    // "d" is a difference in Declination between two planets (5 points)
                    double[] d = new double[5];

                    // p1 is a number of first planet
                    for (int p1 = 1; p1 <= 8; p1++)
                    {
                        // p2 is a number for second planet
                        for (int p2 = p1 + 1; p2 <= 8; p2++)
                        {
                            // skip Earth
                            if (p1 != 3 && p2 != 3)
                            {
                                // "a1" is Right Ascensions for the first planet (5 points)
                                double[] a1 = new double[5];

                                // "a2" is Right Ascensions for the second planet (5 points)
                                double[] a2 = new double[5];

                                // collect Right Ascensions for both planets
                                for (int i = 0; i < 5; i++)
                                {
                                    a1[i] = queue.ElementAt(i)[p1].Alpha;
                                    a2[i] = queue.ElementAt(i)[p2].Alpha;
                                }

                                // Align values to avoid 360 degrees point crossing
                                Angle.Align(a1);
                                Angle.Align(a2);

                                for (int i = 0; i < 5; i++)
                                {
                                    // "a" is a difference in Right Ascension between two planets (5 points)
                                    a[i] = a1[i] - a2[i];

                                    // "d" is a difference in Declination between two planets (5 points)
                                    d[i] = queue.ElementAt(i)[p1].Delta - queue.ElementAt(i)[p2].Delta;
                                }

                                // If difference in Right Ascension changes its sign, it means a conjunction takes place
                                // Will use interpolation to find a point where the conjunction occurs ("zero point")
                                if (a[1] * a[2] < 0)
                                {                                  
                                    // Time shifts, in days, from starting point to each point in a range to be interpolated
                                    // This is a "X" axis in interpolation process
                                    double[] t = { 0, 1, 2, 3, 4 };

                                    // "t1" is a current left edge of the time segment
                                    double t1 = t[0];

                                    // "t0" is a midpoint of the time segment
                                    double t0 = t[2];

                                    // "t2" is a current right edge of the time segment
                                    double t2 = t[4];

                                    // "y0" is a value of interpolated function at the midpoint of the segment
                                    // "y2" is a value of interpolated function at the right edge of the segment
                                    double y0, y2;

                                    do
                                    {
                                        y0 = Interpolation.Lagrange(t, a, t0);
                                        y2 = Interpolation.Lagrange(t, a, t2);

                                        // the function changes sign at the right half of the segment
                                        if (y0 * y2 < 0)
                                        {
                                            // move left point of the segment
                                            t1 = t0;
                                        }
                                        // the function changes sign at the left half of the segment
                                        else
                                        {
                                            // move right point of the segment
                                            t2 = t0;
                                        }

                                        // caculate new value of the midpoint
                                        t0 = (t2 + t1) / 2;
                                    }
                                    while (Math.Abs(y0) > 1e-6);

                                    // planet names
                                    string name1 = planetsCalc.GetPlanetName(p1);
                                    string name2 = planetsCalc.GetPlanetName(p2);

                                    // passing direction
                                    string direction = queue.ElementAt(2)[p1].Delta > queue.ElementAt(2)[p2].Delta ? "north" : "south";

                                    // find the angular distance at the "zero point"
                                    string ad = Formatters.ConjunctionSeparation.Format(Math.Abs(Interpolation.Lagrange(t, d, t0)));

                                    // magnitude of the first planet
                                    string mag1 = Formatters.Magnitude.Format(ctx.Get(planetsCalc.Magnitude, p1));

                                    // magnitude of the second planet
                                    string mag2 = Formatters.Magnitude.Format(ctx.Get(planetsCalc.Magnitude, p2));

                                    events.Add(new AstroEvent(jd - 4 + t0, $"{name1} ({mag1}) passes {ad} {direction} to {name2} ({mag2})"));
                                }
                            }
                        }
                    }
                }
            }

            return events;
        }
    }
}
