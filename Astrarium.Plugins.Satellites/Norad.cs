using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Astrarium.Plugins.Satellites
{
    public static class Norad
    {
        #region Constants

        /// <summary>
        /// ke constant (=SQRT(G*M))
        /// </summary>
        private static double ke = 0.743669161e-1;

        /// <summary>
        /// Equatorial radius of the Earth, in 'er' units
        /// </summary>
        private static double aE = 1.0;

        /// <summary>
        ///  J2 harmonic
        /// </summary>
        private static double J2 = 1.0826158E-3;

        /// <summary>
        /// J3 harmonic
        /// </summary>
        private static double J3 = -2.53881E-6;

        /// <summary>
        /// J4 harmonic
        /// </summary>
        private static double J4 = -1.65597E-6;

        /// <summary>
        /// k2 constant
        /// </summary>
        private static double k2 = 0.5 * J2 * aE * aE;

        /// <summary>
        /// k4 constant
        /// </summary>
        private static double k4 = -3.0 / 8.0 * J4 * aE * aE * aE * aE;

        /// <summary>
        /// A3,0 constant
        /// </summary>
        private static double A30 = -J3 * aE * aE * aE;

        /// <summary>
        /// Earth radius, in km (=XKMPER)
        /// </summary>
        private static double EarthRadius = 6378.135;

        /// <summary>
        /// Earth flattening
        /// </summary>
        private static double EarthFlattening = 1.0 / 298.26;

        /// <summary>
        /// Solar radius, in kilomenters
        /// </summary>
        private static double SolarRadius = 696000.0;


        #endregion Constants

        /// <summary>
        /// Calculates satellite position with SGP4 algorithm
        /// </summary>
        /// <param name="tle">Tle orbit data</param>
        public static Vec3 SGP4(TLE tle, double jd)
        {
            double n0 = tle.MeanMotion * (2 * Math.PI) / 1440.0;
            double e0 = tle.Eccentricity;
            double i0 = Angle.ToRadians(tle.Inclination);
            double Bstar = tle.BStar;
            double w0 = Angle.ToRadians(tle.ArgumentOfPerigee);
            double M0 = Angle.ToRadians(tle.MeanAnomaly);
            double OMEGA0 = Angle.ToRadians(tle.LongitudeAscNode);

            double t_t0 = (jd - tle.Epoch) * 1440.0; // time since epoch, in minutes
            double t_t0_2 = t_t0 * t_t0;
            double t_t0_3 = t_t0_2 * t_t0;
            double t_t0_4 = t_t0_3 * t_t0;
            double t_t0_5 = t_t0_4 * t_t0;

            double theta = Math.Cos(i0);

            double _t1 = (3 * theta * theta - 1) / Math.Pow(1 - e0 * e0, 1.5);

            double a1 = Math.Pow(ke / n0, 2.0 / 3.0);
            double delta1 = 1.5 * k2 / (a1 * a1) * _t1;
            double delta1_2 = delta1 * delta1;
            double delta1_3 = delta1_2 * delta1;

            double a0 = a1 * (1 - 1.0 / 3.0 * delta1 - delta1_2 - 134.0 / 81.0 * delta1_3);
            double delta0 = 1.5 * k2 / (a0 * a0) * _t1;

            double n0__ = n0 / (1 + delta0);

            // semimajor axis
            double a0__ = a0 / (1 - delta0);
            double a0__2 = a0__ * a0__;
            double a0__4 = a0__2 * a0__2;

            double perigee = (a0__ * (1 - e0) - aE) * EarthRadius;

            bool isSimp = false;

            if (perigee < 220)
            {
                isSimp = true;
            }

            double s = aE + 78.0 / EarthRadius;   // s
            double q0 = aE + 120.0 / EarthRadius; // q0
            double qo_s4 = Math.Pow(q0 - s, 4);   // (qo - s)^4

            if (perigee <= 98)
            {
                s = aE + 20.0 / EarthRadius;
                qo_s4 = Math.Pow(q0 - s, 4);
            }
            else if (perigee > 98 && perigee < 156)
            {
                s = a0__ * (1 - e0) - s + aE;
                qo_s4 = Math.Pow(q0 - s, 4);
            }


            double theta_2 = theta * theta;
            double theta_3 = theta_2 * theta;
            double theta_4 = theta_3 * theta;

            double ksi = 1.0 / (a0__ - s);
            double ksi_2 = ksi * ksi;
            double ksi_3 = ksi_2 * ksi;
            double ksi_4 = ksi_3 * ksi;
            double ksi_5 = ksi_4 * ksi;

            double beta0 = Math.Sqrt(1 - e0 * e0);
            double beta0_2 = beta0 * beta0;
            double beta0_3 = beta0_2 * beta0;
            double beta0_4 = beta0_3 * beta0;
            double beta0_7 = beta0_4 * beta0_3;
            double beta0_8 = beta0_7 * beta0;

            double eta = a0__ * e0 * ksi;
            double eta_2 = eta * eta;
            double eta_3 = eta_2 * eta;
            double eta_4 = eta_3 * eta;

            double C2 = qo_s4 * ksi_4 * n0__ * Math.Pow(1 - eta_2, -3.5) *
                (
                    a0__ * (1 + 1.5 * eta_2 + 4 * e0 * eta + e0 * eta_3) +
                    1.5 * (k2 * ksi) / (1 - eta_2) * (-0.5 + 1.5 * theta_2) * (8 + 24 * eta_2 + 3 * eta_4)
                );

            double C1 = Bstar * C2;

            double sini0 = Math.Sin(i0);

            double C3 = (qo_s4 * ksi_5 * A30 * n0__ * aE * sini0) / (k2 * e0);

            double C4 = 2 * n0__ * qo_s4 * ksi_4 * a0__ * beta0_2 * Math.Pow(1 - eta_2, -3.5) *
                (
                    (2 * eta * (1 + e0 * eta) + 0.5 * e0 + 0.5 * eta_3)
                    - (2 * k2 * ksi) / (a0__ * (1 - eta_2)) *
                        (
                            3 * (1 - 3 * theta_2) * (1 + 1.5 * eta_2 - 2 * e0 * eta - 0.5 * e0 * eta_3) +
                            0.75 * (1 - theta_2) * (2 * eta_2 - e0 * eta - e0 * eta_3) * Math.Cos(2 * w0)
                        )
                );

            double C5 = 2 * qo_s4 * ksi_4 * a0__ * beta0_2 * Math.Pow(1 - eta_2, -3.5) *
                (1 + 11.0 / 4.0 * eta * (eta + e0) + e0 * eta_3);

            double C1_2 = C1 * C1;
            double C1_3 = C1_2 * C1;
            double C1_4 = C1_3 * C1;

            double D2 = 4 * a0__ * ksi * C1_2;

            double D3 = 4.0 / 3.0 * a0__ * ksi_2 * (17 * a0__ + s) * C1_3;

            double D4 = 2.0 / 3.0 * a0__ * ksi_3 * (221 * a0__ + 31 * s) * C1_4;

            // secular effects:

            double MDF = M0 +
                (
                    1 + 3 * k2 * (-1 + 3 * theta_2) / (2 * a0__2 * beta0_3) +
                    3 * k2 * k2 * (13 - 78 * theta_2 + 137 * theta_4) / (16 * a0__4 * beta0_7)
                ) * n0__ * t_t0;

            double wDF = w0 +
                (
                    -3 * k2 * (1 - 5 * theta_2) / (2 * a0__2 * beta0_4) +
                     3 * k2 * k2 * (7 - 114 * theta_2 + 395 * theta_4) / (16 * a0__4 * beta0_8) +
                     5 * k4 * (3 - 36 * theta_2 + 49 * theta_4) / (4 * a0__4 * beta0_8)
                ) * n0__ * t_t0;

            double OMEGADF = OMEGA0 +
                (
                    -3 * k2 * theta / (a0__2 * beta0_4) +
                    3 * k2 * k2 * (4 * theta - 19 * theta_3) / (2 * a0__4 * beta0_8) +
                    5 * k4 * theta * (3 - 7 * theta_2) / (2 * a0__4 * beta0_8)
                ) * n0__ * t_t0;


            double Mp;
            double w;

            if (isSimp) /* perigee < 220 km */
            {
                Mp = MDF;
                w = wDF;
            }
            else
            {
                double deltaw = Bstar * C3 * Math.Cos(w0) * t_t0;

                double deltaM = -2.0 / 3.0 * qo_s4 * Bstar * ksi_4 * aE / (e0 * eta) *
                    (
                        Math.Pow(1 + eta * Math.Cos(MDF), 3) - Math.Pow(1 + eta * Math.Cos(M0), 3)
                    );

                Mp = MDF + deltaw + deltaM;
                w = wDF - deltaw - deltaM;
            }

            double OMEGA = OMEGADF - 10.5 * (n0__ * k2 * theta) / (a0__2 * beta0_2) * C1 * t_t0_2;

            double e;
            double a;
            double IL;

            if (isSimp) /* perigee < 220 km */
            {
                // terms after C1 truncated:

                e = e0 - Bstar * C4 * t_t0;

                a = a0__ * Math.Pow(1 - C1 * t_t0, 2);

                IL = Mp + w + OMEGA + n0__ *
                    (
                        1.5 * C1 * t_t0_2
                    );
            }
            else
            {
                e = e0 - Bstar * C4 * t_t0 - Bstar * C5 * (Math.Sin(Mp) - Math.Sin(M0));

                a = a0__ * Math.Pow(1 - C1 * t_t0 - D2 * t_t0_2 - D3 * t_t0_3 - D4 * t_t0_4, 2);

                IL = Mp + w + OMEGA + n0__ *
                    (
                        1.5 * C1 * t_t0_2 + (D2 + 2 * C1_2) * t_t0_3 +
                        0.25 * (3 * D3 + 12 * C1 * D2 + 10 * C1_3) * t_t0_4
                        + 0.2 * (3 * D4 + 12 * C1 * D3 + 6 * D2 * D2 + 30 * C1_2 * D2 + 15 * C1_4) * t_t0_5
                    );
            }


            double beta = Math.Sqrt(1 - e * e);

            double n = ke / Math.Pow(a, 1.5);

            // Long-period terms:

            double axN = e * Math.Cos(w);

            double ILL = (A30 * sini0) / (8 * k2 * a * beta * beta) * (e * Math.Cos(w)) * (3 + 5 * theta) / (1 + theta);

            double ayNL = (A30 * sini0) / (4 * k2 * a * beta * beta);

            double ILT = IL + ILL;

            double ayN = e * Math.Sin(w) + ayNL;

            double Ew1, Ew0, DELTA_Ew0;

            // Solve Kepler equation:

            DELTA_Ew0 = 100;
            Ew0 = ILT - OMEGA;
            Ew1 = 0;
            int step = 0;
            while (Math.Abs(DELTA_Ew0) > 1e-6 && step < 10)
            {
                DELTA_Ew0 = (ILT - OMEGA - ayN * Math.Cos(Ew0) + axN * Math.Sin(Ew0) - Ew0) / (-ayN * Math.Sin(Ew0) - axN * Math.Cos(Ew0) + 1);
                Ew1 = Ew0 + DELTA_Ew0;
                Ew0 = Ew1;
                step++;
            }

            double ecosE = axN * Math.Cos(Ew1) + ayN * Math.Sin(Ew1);
            double esinE = axN * Math.Sin(Ew1) - ayN * Math.Cos(Ew1);

            double eL = Math.Sqrt(axN * axN + ayN * ayN);
            double pL = a * (1 - eL * eL);
            double r = a * (1 - ecosE);

            double rdot = ke * Math.Sqrt(a) / r * esinE;
            double rfdot = ke * Math.Sqrt(pL) / r;

            double cosu = (a / r) *
                (
                    Math.Cos(Ew1) - axN +
                    (ayN * esinE) / (1 + Math.Sqrt(1 - eL * eL))
                );

            double sinu = (a / r) *
                (
                    Math.Sin(Ew1) - ayN -
                    (axN * esinE) / (1 + Math.Sqrt(1 - eL * eL))
                );

            double u = Math.Atan2(sinu, cosu);
            double cos2u = Math.Cos(2 * u);
            double sin2u = Math.Sin(2 * u);

            double DELTAr = k2 / (2 * pL) * (1 - theta_2) * cos2u;
            double DELTAu = -k2 / (4 * pL * pL) * (7 * theta_2 - 1) * sin2u;
            double DELTAOMEGA = (3 * k2 * theta) / (2 * pL * pL) * sin2u;
            double DELTAi = (3 * k2 * theta) / (2 * pL * pL) * sini0 * cos2u;
            double DELTArdot = -(k2 * n) / pL * (1 - theta_2) * sin2u;
            double DELATArfdot = (k2 * n) / pL *
                (
                    (1 - theta_2) * cos2u - 1.5 * (1 - 3 * theta_2)
                );

            // Short-period periodics:

            double rk = r *
                (
                    1 - 1.5 * k2 * Math.Sqrt(1 - eL * eL) / (pL * pL) * (3 * theta_2 - 1)
                ) + DELTAr;

            double uk = u + DELTAu;

            double OMEGAk = OMEGA + DELTAOMEGA;

            double ik = i0 + DELTAi;

            double rdotk = rdot + DELTArdot;

            double rfdotk = rfdot + DELATArfdot;

            // unit orientation vectors:

            double sinuk = Math.Sin(uk);
            double cosuk = Math.Cos(uk);

            double[] M = new double[] {
                             -Math.Sin(OMEGAk) * Math.Cos(ik),
                             Math.Cos(OMEGAk) * Math.Cos(ik),
                             Math.Sin(ik)
                         };

            double[] N = new double[] {
                            Math.Cos(OMEGAk),
                            Math.Sin(OMEGAk),
                            0
                         };

            double[] U = new double[3];
            double[] V = new double[3];
            double[] position = new double[3];
            double[] velocity = new double[3];

            for (int i = 0; i < 3; i++)
            {
                U[i] = M[i] * sinuk + N[i] * cosuk;
                V[i] = M[i] * cosuk - N[i] * sinuk;
                position[i] = rk * U[i];
                velocity[i] = rdotk * U[i] + rfdotk * V[i];

                position[i] = position[i] * EarthRadius / aE;           // in km
                velocity[i] = velocity[i] * EarthRadius / aE * 60.0;    // in km/s
            }

            return new Vec3(position[0], position[1], position[2]);
        }

        public static Vec3 TopocentricLocationVector(CrdsGeographical location, double sideralTime)
        {
            double longitude = -Angle.ToRadians(location.Longitude);
            double latitude = Angle.ToRadians(location.Latitude);
            double altitude = location.Elevation / 1000.0;

            double localMeanSiderealTime = (Angle.ToRadians(sideralTime) + longitude) % (2 * Math.PI);

            double sinLatitude = Math.Sin(latitude);
            double cosLatitude = Math.Cos(latitude);

            double c = 1.0 / Math.Sqrt(1 + EarthFlattening * (EarthFlattening - 2) * sinLatitude * sinLatitude);
            double s = (1 - EarthFlattening) * (1 - EarthFlattening) * c;
            double achcp = (EarthRadius * c + altitude) * cosLatitude;

            double x = achcp * Math.Cos(localMeanSiderealTime);
            double y = achcp * Math.Sin(localMeanSiderealTime);
            double z = (EarthRadius * s + altitude) * sinLatitude;

            return new Vec3(x, y, z);
        }

        public static Vec3 TopocentricSatelliteVector(Vec3 observer, Vec3 satellite)
        {
            return satellite - observer;
        }

        public static CrdsHorizontal HorizontalCoordinates(CrdsGeographical observerSite, Vec3 satellite, double sideralTime)
        {
            // distance to satellite:
            double range = satellite.Length;

            double theta = (Angle.ToRadians(sideralTime) + Angle.ToRadians(-observerSite.Longitude)) % (2 * Math.PI);

            double lat = Angle.ToRadians(observerSite.Latitude);
            double sin_lat = Math.Sin(lat);
            double cos_lat = Math.Cos(lat);
            double sin_theta = Math.Sin(theta);
            double cos_theta = Math.Cos(theta);

            double top_s = sin_lat * cos_theta * satellite.X +
                           sin_lat * sin_theta * satellite.Y -
                           cos_lat * satellite.Z;
            double top_e = -sin_theta * satellite.X +
                            cos_theta * satellite.Y;
            double top_z = cos_lat * cos_theta * satellite.X +
                           cos_lat * sin_theta * satellite.Y +
                           sin_lat * satellite.Z;

            double az = Math.Atan(-top_e / top_s);

            if (top_s > 0.0)
                az += Math.PI;

            if (az < 0.0)
                az += 2.0 * Math.PI;

            double el = Math.Asin(top_z / range);

            CrdsHorizontal hor = new CrdsHorizontal();

            hor.Altitude = Angle.ToDegrees(el);
            hor.Azimuth = Angle.To360(Angle.ToDegrees(az) + 180);

            return hor;
        }

        /// <summary>
        /// Returns visual satellite magnitude at specified conditions 
        /// </summary>
        /// <param name="stdmag">Standard satellite magnitude</param>
        /// <param name="range">The distance from observer to satellite, km</param>
        /// <returns></returns>
        public static float GetSatelliteMagnitude(float stdmag, double range)
        {
            return (float)(stdmag - 15.75 + 2.5 * Math.Log10(range * range));
        }

        public static double GetSatellitePerigee(TLE tle)
        {
            double n0 = tle.MeanMotion * (2 * Math.PI) / 1440.0;
            double e0 = tle.Eccentricity;
            double i0 = Angle.ToRadians(tle.Inclination);

            double theta = Math.Cos(i0);

            double _t1 = (3 * theta * theta - 1) / Math.Pow(1 - e0 * e0, 1.5);

            double a1 = Math.Pow(ke / n0, 2.0 / 3.0);
            double delta1 = 1.5 * k2 / (a1 * a1) * (_t1);
            double delta1_2 = delta1 * delta1;
            double delta1_3 = delta1_2 * delta1;

            double a0 = a1 * (1 - 1.0 / 3.0 * delta1 - delta1_2 - 134.0 / 81.0 * delta1_3);
            double delta0 = 1.5 * k2 / (a0 * a0) * (_t1);

            // semimajor axis
            double a0__ = a0 / (1 - delta0);

            double perigee = (a0__ * (1 - e0) - aE) * EarthRadius;

            return perigee;
        }

        public static double GetSatelliteApogee(TLE tle)
        {
            return GetSatellitePerigee(tle) / (1 - tle.Eccentricity) * (1 + tle.Eccentricity);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="satellite">Geocentrical ECI vector of satellite</param>
        /// <param name="sun">ECI vector of Sun</param>
        /// <returns>Returns 'true' if satellite is eclipsed by earth shadow</returns>
        public static bool IsSatelliteEclipsed(Vec3 satellite, Vec3 sun)
        {
            // http://celestrak.com/columns/v03n01/

            // Vector "satellite -> Sun"
            Vec3 rho_S = sun - satellite;

            // Vector "satellite -> Earth center"
            Vec3 rho_E = -1 * satellite;

            // Earth semidiameter: 
            double theta_E = Math.Asin(EarthRadius / satellite.Length);

            // Solar semidiameter:
            double theta_S = Math.Asin(SolarRadius / rho_S.Length);

            // Angular separation between Sun and Earth centers
            double delta = rho_E.Angle(rho_S);
            
            // Earth and Sun centers are sufficient separated:
            if (delta > theta_E + theta_S) return false;

            // Earth hides Sun
            if (delta < theta_E - theta_S) return true;

            return false;
        }
    }
}
