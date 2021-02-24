using System.Globalization;
using Astrarium.Algorithms;

namespace Astrarium.Plugins.Eclipses.Types
{
    public class LunarElementsTableItem
    {
        public string Index { get; set; }
        public string X { get; set; }
        public string Y { get; set; }
        public string F1 { get; set; }
        public string F2 { get; set; }
        public string F3 { get; set; }

        public LunarElementsTableItem(int index, PolynomialLunarEclipseElements ple)
        {
            Index = index.ToString();
            X = ple.X[index].ToString("N5", CultureInfo.InvariantCulture);
            Y = ple.Y[index].ToString("N5", CultureInfo.InvariantCulture);
            if (index < 3)
            {
                F1 = ple.F1[index].ToString("N5", CultureInfo.InvariantCulture);
                F2 = ple.F2[index].ToString("N5", CultureInfo.InvariantCulture);
                F3 = ple.F3[index].ToString("N5", CultureInfo.InvariantCulture);
            }
        }
    }
}
