using Astrarium.Algorithms;
using System.Globalization;

namespace Astrarium.Plugins.Eclipses.Types
{
    public class BesselianElementsTableItem
    {
        public string Index { get; set; }
        public string X { get; set; }
        public string Y { get; set; }
        public string D { get; set; }
        public string L1 { get; set; }
        public string L2 { get; set; }
        public string Mu { get; set; }

        public BesselianElementsTableItem(int index, PolynomialBesselianElements pbe)
        {
            Index = index.ToString();
            X = pbe.X[index].ToString("N6", CultureInfo.InvariantCulture);
            Y = pbe.Y[index].ToString("N6", CultureInfo.InvariantCulture);

            if (index <= 2)
            {
                D = pbe.D[index].ToString("N6", CultureInfo.InvariantCulture);
                L1 = pbe.L1[index].ToString("N6", CultureInfo.InvariantCulture);
                L2 = pbe.L2[index].ToString("N6", CultureInfo.InvariantCulture);
            }

            if (index <= 1)
                Mu = Angle.To360(pbe.Mu[index]).ToString("N6", CultureInfo.InvariantCulture);
        }
    }
}
