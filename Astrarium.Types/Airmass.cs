using System.ComponentModel;
using static System.Math;
using static Astrarium.Algorithms.Angle;

namespace Astrarium.Types
{
    /// <summary>
    /// Implements logic for airmass computations.
    /// </summary>
    public static class Airmass
    {
        /// <summary>
        /// Gets airmass value
        /// </summary>
        /// <param name="h">Body altitude, in degrees.</param>
        /// <param name="model">Airmass model.</param>
        /// <returns>X, an airmass value.</returns>
        public static double GetValue(double h, AirmassModel model)
        {
            switch (model)
            {
                case AirmassModel.PlaneParallel:
                    return PlaneParallel(h);
                case AirmassModel.Hardie:
                    return Hardie(h);
                case AirmassModel.KastenYoung:
                    return KastenYoung(h);
                case AirmassModel.Young:
                    return Young(h);
                case AirmassModel.YoungIrvine:
                    return YoungIrvine(h);
                case AirmassModel.Rozenberg:
                    return Rozenberg(h);
                case AirmassModel.Pickering:
                    return Pickering(h);
                default:
                    return 0;
            }
        }

        /// <remarks>
        /// See Duffett-Smith, book "Practical Astronomy With Calculator", topic "Atmospheric Extinction"
        /// </remarks>
        private static double PlaneParallel(double h)
        {
            double z = 90 - h;
            if (z > 85) z = 85; // the formula works well only for zenith distances less than 85 degrees
            return 1.0 / Cos(ToRadians(z));
        }

        /// <remarks>
        /// Hardie, R. H. 1962. In Astronomical Techniques. Hiltner, W. A., ed. Chicago: University of Chicago Press, 184–. LCCN 62009113. 
        /// Bibcode: 1962aste.book.....H
        /// </remarks>
        private static double Hardie(double h)
        {
            double z = 90 - h;
            if (z > 85) z = 85; // the formula works well only for zenith distances less than 85 degrees
            double secZ = 1 / Cos(ToRadians(z));
            double secZ_1 = secZ - 1;
            double secZ_1_2 = secZ_1 * secZ_1;
            double secZ_1_3 = secZ_1_2 * secZ_1;
            return secZ - 0.0018167 * secZ_1 - 0.002875 * secZ_1_2 - 0.0008083 * secZ_1_3;
        }

        /// <remarks>
        /// Kasten, F.; Young, A. T. (1989). "Revised optical air mass tables and approximation formula". Applied Optics. 28 (22): 4735–4738. 
        /// Bibcode: 1989ApOpt..28.4735K
        /// </remarks>
        private static double KastenYoung(double h)
        {
            double z = 90 - h;
            double cosZ = Cos(ToRadians(z));
            return 1.0 / (cosZ + 0.50572 * Pow(6.07995 + h, -1.6364));
        }

        /// <remarks>
        /// Young, Andrew T. (1994-02-20). "Air Mass and Refraction". Applied Optics. 33 (6): 1108–1110. 
        /// Bibcode: 1994ApOpt..33.1108Y
        /// </remarks>
        private static double Young(double h)
        {
            double z = 90 - h;
            double cosZ = Cos(ToRadians(z));
            return ((1.002432 * cosZ + 0.148386) * cosZ + 0.0096467) / (((cosZ + 0.149864) * cosZ + 0.0102963) * cosZ + 0.000303978);
        }

        /// <remarks>
        /// Young, Andrew T.; Irvine, William M. (1967). "Multicolor photoelectric photometry of the brighter planets. I. Program and Procedure". The Astronomical Journal. 72: 945–950. 
        /// Bibcode: 1967AJ.....72..945Y
        /// </remarks>
        private static double YoungIrvine(double h)
        {
            double z = 90 - h;
            if (z > 80) z = 80; // the formula works well only for zenith distances less than 80 degrees
            double secZ = 1 / Cos(ToRadians(z));
            return secZ * (1 - 0.0012 * (secZ * secZ - 1));
        }

        /// <remarks>
        /// Rozenberg, Grzegorz V. (1966). Twilight: A Study in Atmospheric Optics. New York: Plenum Press. ISBN 978-1-4899-6353-6. LCCN 65011345
        /// </remarks>
        private static double Rozenberg(double h)
        {
            double z = 90 - h;
            double cosZ = Cos(ToRadians(z));
            return 1.0 / (cosZ + 0.025 * Exp(-11 * cosZ));
        }

        /// <remarks>
        /// Pickering, K. A. (2002). "The Southern Limits of the Ancient Star Catalog" (PDF). DIO. 12 (1): 20–39.
        /// </remarks>
        private static double Pickering(double h)
        {
            return 1.0 / Sin(ToRadians(h + 244.0 / (165.0 + 47 * Pow(h, 1.1))));
        }
    }

    /// <summary>
    /// Airmass model
    /// </summary>
    public enum AirmassModel
    {
        /// <summary>
        /// Plane-parallel atmosphere
        /// </summary>
        [Description("AirmassModel.PlaneParallel")]
        PlaneParallel,

        /// <summary>
        /// Hardie (1962)
        /// </summary>
        [Description("AirmassModel.Hardie")]
        Hardie,

        /// <summary>
        /// Rozenberg (1966)
        /// </summary>
        [Description("AirmassModel.Rozenberg")]
        Rozenberg,

        /// <summary>
        /// Young & Irvine (1967)
        /// </summary>
        [Description("AirmassModel.YoungIrvine")]
        YoungIrvine,

        /// <summary>
        /// Kasten & Young (1989)
        /// </summary>
        [Description("AirmassModel.KastenYoung")]
        KastenYoung,

        /// <summary>
        /// Young (1994)
        /// </summary>
        [Description("AirmassModel.Young")]
        Young,

        /// <summary>
        /// Pickering (2002)
        /// </summary>
        [Description("AirmassModel.Pickering")]
        Pickering
    }
}
