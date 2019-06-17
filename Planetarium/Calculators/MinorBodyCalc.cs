using ADK;
using Planetarium.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.Calculators
{
    public abstract class MinorBodyCalc : BaseCalc
    {
        protected double Phase(SkyContext c, int i)
        {
            return BasicEphem.Phase(c.Get(PhaseAngle, i));
        }

        protected double PhaseAngle(SkyContext c, int i)
        {
            double delta = c.Get(DistanceFromEarth, i);
            double r = c.Get(DistanceFromSun, i);
            double R = c.Get(EarthDistanceFromSun);

            return MinorBodyEphem.PhaseAngle(r, delta, R);
        }

        protected abstract OrbitalElements OrbitalElements(SkyContext c, int i);

        protected CrdsRectangular Rectangular(SkyContext c, int i)
        {
            // final difference to stop iteration process, 1 second of time
            double deltaTau = TimeSpan.FromSeconds(1).TotalDays;

            // time taken by the light to reach the Earth
            double tau = 0;

            // previous value of tau to calculate the difference
            double tau0 = 1;

            // Rectangular coordinates of asteroid
            CrdsRectangular rect = null;

            // Rectangular coordinates of the Sun
            var sun = c.Get(SunRectangular);

            // Orbital elements
            var orbit = c.Get(OrbitalElements, i);

            double ksi = 0, eta = 0, zeta = 0, Delta = 0;

            // Iterative process to find rectangular coordinates of asteroid
            while (Math.Abs(tau - tau0) > deltaTau)
            {
                // Rectangular coordinates of asteroid
                rect = MinorBodyPositions.GetRectangularCoordinates(orbit, c.JulianDay - tau, c.Epsilon);

                ksi = sun.X + rect.X;
                eta = sun.Y + rect.Y;
                zeta = sun.Z + rect.Z;

                // Distance to the Earth
                Delta = Math.Sqrt(ksi * ksi + eta * eta + zeta * zeta);

                tau0 = tau;
                tau = PlanetPositions.LightTimeEffect(Delta);
            }

            return rect;
        }

        protected double DistanceFromEarth(SkyContext c, int i)
        {
            var rAsteroid = c.Get(Rectangular, i);
            var rSun = c.Get(SunRectangular);

            double x = rSun.X + rAsteroid.X;
            double y = rSun.Y + rAsteroid.Y;
            double z = rSun.Z + rAsteroid.Z;

            return Math.Sqrt(x * x + y * y + z * z);
        }

        protected double DistanceFromSun(SkyContext c, int i)
        {
            var r = c.Get(Rectangular, i);
            return Math.Sqrt(r.X * r.X + r.Y * r.Y + r.Z * r.Z);
        }

        /// <summary>
        /// Gets precessional elements to convert equatorial coordinates of minor body to current epoch 
        /// </summary>
        private PrecessionalElements GetPrecessionalElements(SkyContext c)
        {
            return Precession.ElementsFK5(Date.EPOCH_J2000, c.JulianDay);
        }

        /// <summary>
        /// Gets equatorial geocentrical coordinates for J2000 epoch
        /// </summary>
        protected CrdsEquatorial EquatorialJ2000(SkyContext c, int i)
        {
            var Delta = c.Get(DistanceFromEarth, i);
            var rAsteroid = c.Get(Rectangular, i);
            var rSun = c.Get(SunRectangular);

            double x = rSun.X + rAsteroid.X;
            double y = rSun.Y + rAsteroid.Y;
            double z = rSun.Z + rAsteroid.Z;

            double alpha = Angle.ToDegrees(Math.Atan2(y, x));
            double delta = Angle.ToDegrees(Math.Asin(z / Delta));

            return new CrdsEquatorial(alpha, delta);
        }

        /// <summary>
        /// Gets equatorial geocentrical coordinates for current epoch
        /// </summary>
        protected CrdsEquatorial EquatorialG(SkyContext c, int i)
        {
            // Precessinal elements to convert between epochs
            var pe = c.Get(GetPrecessionalElements);

            // Equatorial geocentrical coordinates for J2000 epoch
            var eq0 = c.Get(EquatorialJ2000, i);

            // Equatorial coordinates for the mean equinox and epoch of the target date
            CrdsEquatorial eq = Precession.GetEquatorialCoordinates(eq0, pe);

            // Nutation effect
            var eq1 = Nutation.NutationEffect(eq, c.NutationElements, c.Epsilon);

            // Aberration effect
            var eq2 = Aberration.AberrationEffect(eq, c.AberrationElements, c.Epsilon);

            // Apparent coordinates of the object
            eq += eq1 + eq2;

            return eq;
        }

        /// <summary>
        /// Gets horizontal parallax of asteroid
        /// </summary>
        protected double Parallax(SkyContext c, int i)
        {
            return PlanetEphem.Parallax(c.Get(DistanceFromEarth, i));
        }

        /// <summary>
        /// Gets equatorial topocentric coordinates of minor body
        /// </summary>
        protected CrdsEquatorial EquatorialT(SkyContext c, int i)
        {
            var eq0 = c.Get(EquatorialG, i);
            var parallax = c.Get(Parallax, i);
            return eq0.ToTopocentric(c.GeoLocation, c.SiderealTime, parallax);
        }

        protected CrdsHorizontal Horizontal(SkyContext c, int i)
        {
            var eq = c.Get(EquatorialT, i);
            return eq.ToHorizontal(c.GeoLocation, c.SiderealTime);
        }

        /// <summary>
        /// Gets rectangular coordinates of Sun for J2000.0 epoch
        /// </summary>
        protected CrdsRectangular SunRectangular(SkyContext c)
        {
            CrdsHeliocentrical hEarth = PlanetPositions.GetPlanetCoordinates(Planet.EARTH, c.JulianDay, !c.PreferFastCalculation, false);

            var eSun = new CrdsEcliptical(Angle.To360(hEarth.L + 180), -hEarth.B, hEarth.R);

            // Corrected solar coordinates to FK5 system
            // NO correction for nutation and aberration should be performed here (ch. 26, p. 171)
            eSun += PlanetPositions.CorrectionForFK5(c.JulianDay, eSun);

            return eSun.ToRectangular(c.Epsilon);
        }

        protected double EarthDistanceFromSun(SkyContext c)
        {
            var r = c.Get(SunRectangular);
            return Math.Sqrt(r.X * r.X + r.Y * r.Y + r.Z * r.Z);
        }

        /// <summary>
        /// Gets rise, transit and set info for the planet
        /// </summary>
        protected RTS RiseTransitSet(SkyContext c, int a)
        {
            double jd = c.JulianDayMidnight;
            double theta0 = Date.ApparentSiderealTime(jd, c.NutationElements.deltaPsi, c.Epsilon);
            double parallax = c.Get(Parallax, a);

            CrdsEquatorial[] eq = new CrdsEquatorial[3];
            double[] diff = new double[] { 0, 0.5, 1 };

            for (int i = 0; i < 3; i++)
            {
                eq[i] = new SkyContext(jd + diff[i], c.GeoLocation).Get(EquatorialG, a);
            }

            return Visibility.RiseTransitSet(eq, c.GeoLocation, theta0, parallax);
        }
    }
}
