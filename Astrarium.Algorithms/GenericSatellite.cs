using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Math;
using static Astrarium.Algorithms.Angle;

namespace Astrarium.Algorithms
{
    public static class GenericSatellite
    {
        public static CrdsEcliptical Position(double jd, GenericSatelliteOrbit orbit, CrdsEcliptical planet)
        {
            NutationElements ne = Nutation.NutationElements(jd);
            double epsilon = Date.TrueObliquity(jd, ne.deltaEpsilon);

            // convert current coordinates to epoch, as algorithm requires
            CrdsEquatorial eq = planet.ToEquatorial(epsilon);
            PrecessionalElements peEpoch = Precession.ElementsFK5(jd, Date.EPOCH_J2000);
            CrdsEquatorial eqPlanetEpoch = Precession.GetEquatorialCoordinates(eq, peEpoch);

            // ecliptical pole
            CrdsEquatorial pole = new CrdsEcliptical(0, 90).ToEquatorial(epsilon);
            
            double distance0;
            double distance = planet.Distance;

            CrdsEcliptical eclSatellite;
            do
            {
                distance0 = distance;

                // take light-time effect into account
                double tau = PlanetPositions.LightTimeEffect(distance);

                double t = jd - tau - orbit.jd;

                double M = To360(orbit.M + orbit.n * t);

                double omega = To360(orbit.w + t * 360.0 / (orbit.Pw * 365.25));
                double node = To360(orbit.Om + t * 360.0 / (orbit.POm * 365.25));

                // Find eccentric anomaly by solving Kepler equation
                double E = SolveKepler(M, orbit.e);

                double X = orbit.a * (Cos(E) - orbit.e);
                double Y = orbit.a * Sqrt(1 - orbit.e * orbit.e) * Sin(E);

                // cartesian state vector of satellite
                var d =
                    Matrix.R2(ToRadians(-eqPlanetEpoch.Delta)) *
                    Matrix.R3(ToRadians(eqPlanetEpoch.Alpha)) *
                    Matrix.R3(ToRadians(-pole.Alpha - 90)) *
                    Matrix.R1(ToRadians(pole.Delta - 90)) *
                    Matrix.R3(ToRadians(-node)) *
                    Matrix.R1(ToRadians(-orbit.i)) *
                    Matrix.R3(ToRadians(-omega)) *
                    new Matrix(new double[,] { { X / distance }, { Y / distance }, { 0 } });

                // radial component, positive away from observer
                // converted to degrees
                double x = ToDegrees(d.Values[0, 0]);

                // semimajor axis, expressed in degrees, as visible from Earth
                double theta = ToDegrees(Atan(orbit.a / distance));

                // offsets values in degrees           
                double dAlphaCosDelta = ToDegrees(d.Values[1, 0]);
                double dDelta = ToDegrees(d.Values[2, 0]);

                double delta = eqPlanetEpoch.Delta + dDelta;
                double dAlpha = dAlphaCosDelta / Cos(ToRadians(delta));
                double alpha = eqPlanetEpoch.Alpha + dAlpha;

                CrdsEquatorial eqSatelliteEpoch = new CrdsEquatorial(alpha, delta);

                // convert jd0 equatorial coordinates to current epoch
                // and to ecliptical
                PrecessionalElements pe = Precession.ElementsFK5(Date.EPOCH_J2000, jd);
                CrdsEquatorial eqSatellite = Precession.GetEquatorialCoordinates(eqSatelliteEpoch, pe);
                eclSatellite = eqSatellite.ToEcliptical(epsilon);

                // calculate distance to Earth
                distance = planet.Distance + x / theta * orbit.a;

            }
            while (Abs(distance - distance0) > 1e-6);

            eclSatellite.Distance = distance;

            return eclSatellite;
        }

        public static float Magnitude(double mag0, double Delta, double r)
        {
            return (float)(mag0 + 5 * Math.Log10(r * Delta));
        }

        public static double Semidiameter(double distance, double radius)
        {
            return ToDegrees(Atan(radius / (distance * 149597870.0))) * 3600;
        }

        /// <summary>
        /// Solves Kepler equation
        /// </summary>
        /// <param name="M">Mean anomaly, in degrees</param>
        /// <param name="e">Eccentricity</param>
        /// <returns>Eccentric anomaly, in radians</returns>
        private static double SolveKepler(double M, double e)
        {
            M = ToRadians(M);
            double E0;
            double E1 = M;
            double M_ = M;
            do
            {
                E0 = E1;
                E1 = M_ + e * Sin(E0);
            } while (Abs(E1 - E0) >= 1e-9);
            return E1;
        }
    }
}
