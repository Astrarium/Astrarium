using ADK;
using Planetarium.Objects;
using Planetarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.Plugins.MinorBodies
{
    public abstract class MinorBodyCalc<T> : BaseCalc where T : CelestialObject
    {
        protected double Phase(SkyContext c, T body)
        {
            return BasicEphem.Phase(c.Get(PhaseAngle, body));
        }

        protected double PhaseAngle(SkyContext c, T body)
        {
            double delta = c.Get(DistanceFromEarth, body);
            double r = c.Get(DistanceFromSun, body);
            double R = c.Get(EarthDistanceFromSun);

            return MinorBodyEphem.PhaseAngle(r, delta, R);
        }

        protected abstract OrbitalElements OrbitalElements(SkyContext c, T body);

        /// <summary>
        /// Gets rectangular heliocentric coordinates of minor body
        /// </summary>
        protected CrdsRectangular RectangularH(SkyContext c, T body)
        {
            // final difference to stop iteration process, 1 second of time
            double deltaTau = TimeSpan.FromSeconds(1).TotalDays;

            // time taken by the light to reach the Earth
            double tau = 0;

            // previous value of tau to calculate the difference
            double tau0 = 1;

            // Rectangular coordinates of minor body
            CrdsRectangular rect = null;

            // Rectangular coordinates of the Sun
            var sun = c.Get(SunRectangular);

            // Orbital elements
            var orbit = c.Get(OrbitalElements, body);

            double ksi = 0, eta = 0, zeta = 0, Delta = 0;

            // Iterative process to find rectangular coordinates of minor body
            while (Math.Abs(tau - tau0) > deltaTau)
            {
                // Rectangular coordinates of minor body
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

        /// <summary>
        /// Gets rectangular geocentric coordinates of minor body
        /// </summary>
        public CrdsRectangular RectangularG(SkyContext c, T body)
        {
            var rBody = c.Get(RectangularH, body);
            var rSun = c.Get(SunRectangular);

            double x = rSun.X + rBody.X;
            double y = rSun.Y + rBody.Y;
            double z = rSun.Z + rBody.Z;

            return new CrdsRectangular(x, y, z);
        }

        protected double DistanceFromEarth(SkyContext c, T body)
        {
            var r = c.Get(RectangularG, body);
            return Math.Sqrt(r.X * r.X + r.Y * r.Y + r.Z * r.Z);
        }

        public double DistanceFromSun(SkyContext c, T body)
        {
            var r = c.Get(RectangularH, body);
            return Math.Sqrt(r.X * r.X + r.Y * r.Y + r.Z * r.Z);
        }

        public CrdsEcliptical Ecliptical(SkyContext c, T body)
        {             
            var r = c.Get(RectangularG, body);
            return r.ToEcliptical();
        }

        /// <summary>
        /// Gets precessional elements to convert equatorial coordinates of minor body to current epoch 
        /// </summary>
        protected PrecessionalElements GetPrecessionalElements(SkyContext c)
        {
            return Precession.ElementsFK5(Date.EPOCH_J2000, c.JulianDay);
        }

        /// <summary>
        /// Gets equatorial topocentrical coordinates for J2000.0 epoch
        /// </summary>
        protected CrdsEquatorial EquatorialJ2000T(SkyContext c, T body)
        {
            var eq0 = c.Get(EquatorialJ2000, body);
            var parallax = c.Get(Parallax, body);
            return eq0.ToTopocentric(c.GeoLocation, c.SiderealTime, parallax);
        }

        /// <summary>
        /// Gets equatorial geocentrical coordinates for J2000 epoch
        /// </summary>
        protected CrdsEquatorial EquatorialJ2000(SkyContext c, T body)
        {
            var Delta = c.Get(DistanceFromEarth, body);
            var rBody = c.Get(RectangularH, body);
            var rSun = c.Get(SunRectangular);

            double x = rSun.X + rBody.X;
            double y = rSun.Y + rBody.Y;
            double z = rSun.Z + rBody.Z;

            double alpha = Angle.ToDegrees(Math.Atan2(y, x));
            double delta = Angle.ToDegrees(Math.Asin(z / Delta));

            return new CrdsEquatorial(alpha, delta);
        }

        /// <summary>
        /// Gets equatorial geocentrical coordinates for current epoch
        /// </summary>
        protected CrdsEquatorial EquatorialG(SkyContext c, T body)
        {
            // Precessinal elements to convert between epochs
            var pe = c.Get(GetPrecessionalElements);

            // Equatorial geocentrical coordinates for J2000 epoch
            var eq0 = c.Get(EquatorialJ2000, body);

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
        /// Gets horizontal parallax of minor body
        /// </summary>
        protected double Parallax(SkyContext c, T body)
        {
            return PlanetEphem.Parallax(c.Get(DistanceFromEarth, body));
        }

        /// <summary>
        /// Gets equatorial topocentric coordinates of minor body
        /// </summary>
        protected CrdsEquatorial EquatorialT(SkyContext c, T body)
        {
            var eq0 = c.Get(EquatorialG, body);
            var parallax = c.Get(Parallax, body);
            return eq0.ToTopocentric(c.GeoLocation, c.SiderealTime, parallax);
        }

        /// <summary>
        /// Gets horizontal coordinates of minor body
        /// </summary>
        protected CrdsHorizontal Horizontal(SkyContext c, T body)
        {
            var eq = c.Get(EquatorialT, body);
            return eq.ToHorizontal(c.GeoLocation, c.SiderealTime);
        }

        /// <summary>
        /// Gets rectangular coordinates of Sun for J2000.0 epoch
        /// </summary>
        protected CrdsRectangular SunRectangular(SkyContext c)
        {
            var eSun = c.Get(SunEcliptical);
            return eSun.ToRectangular(c.Epsilon);
        }

        /// <summary>
        /// Gets ecliptical coordinates of Sun for J2000.0 epoch
        /// </summary>
        private CrdsEcliptical SunEcliptical(SkyContext c)
        {
            CrdsHeliocentrical hEarth = PlanetPositions.GetPlanetCoordinates(3, c.JulianDay, !c.PreferFastCalculation, false);

            var eSun = new CrdsEcliptical(Angle.To360(hEarth.L + 180), -hEarth.B, hEarth.R);

            // Corrected solar coordinates to FK5 system
            // NO correction for nutation and aberration should be performed here (ch. 26, p. 171)
            eSun += PlanetPositions.CorrectionForFK5(c.JulianDay, eSun);

            return eSun;
        }

        private CrdsEquatorial SunEquatorial(SkyContext c)
        {
            return c.Get(SunEcliptical).ToEquatorial(c.Epsilon);
        }

        /// <summary>
        /// Gets difference between ecliptical longitudes of the Sun and minor body
        /// </summary>
        public double LongitudeDifference(SkyContext c, T body)
        {
            return BasicEphem.LongitudeDifference(c.Get(SunEcliptical).Lambda, c.Get(Ecliptical, body).Lambda);
        }

        /// <summary>
        /// Gets Earth's distance from the Sun, in a.u.
        /// </summary>
        protected double EarthDistanceFromSun(SkyContext c)
        {
            var r = c.Get(SunRectangular);
            return Math.Sqrt(r.X * r.X + r.Y * r.Y + r.Z * r.Z);
        }

        /// <summary>
        /// Gets rise, transit and set info for the minor body
        /// </summary>
        protected RTS RiseTransitSet(SkyContext c, T body)
        {
            double jd = c.JulianDayMidnight;
            double theta0 = Date.ApparentSiderealTime(jd, c.NutationElements.deltaPsi, c.Epsilon);
            double parallax = c.Get(Parallax, body);

            CrdsEquatorial[] eq = new CrdsEquatorial[3];
            double[] diff = new double[] { 0, 0.5, 1 };

            for (int i = 0; i < 3; i++)
            {
                eq[i] = new SkyContext(jd + diff[i], c.GeoLocation).Get(EquatorialG, body);
            }

            return ADK.Visibility.RiseTransitSet(eq, c.GeoLocation, theta0, parallax);
        }

        protected VisibilityDetails Visibility(SkyContext c, T body)
        {
            double jd = c.JulianDayMidnight;
            double theta0 = Date.ApparentSiderealTime(jd, c.NutationElements.deltaPsi, c.Epsilon);
            double parallax = c.Get(Parallax, body);

            var ctx = new SkyContext(jd, c.GeoLocation);

            var eq = ctx.Get(EquatorialJ2000T, body);
            var eqSun = ctx.Get(SunEquatorial);

            return ADK.Visibility.Details(eq, eqSun, c.GeoLocation, theta0, 5);
        }
    }
}
