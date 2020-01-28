using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Math;
using static ADK.Angle;

namespace ADK
{
    public class UranianMoons
    {
        //private static readonly double[] sep = new double[] { 9.4, 13.9, 18.5, 31.5, 42.2 };
        private static readonly double[] a = new double[] { 129390, 191020, 266300, 435910, 583520 };
        private static readonly double[] elong = new double[] { 2455076.482, 2455077.921, 2455078.29, 2455083.875, 2455076.287 };
        private static readonly double[] rotp = new double[] { 1.413479, 2.520379, 4.144177, 8.705872, 13.463239 };

        public static CrdsRectangular[] Positions(double jd, double de, double pa)
        {
            CrdsRectangular[] positions = new CrdsRectangular[5];
            
            const double RAD = 180 / PI;
           
            for (int i = 0; i < 5; i++)
            {
                double ang = To360((jd - elong[i]) / rotp[i] * 360.0 - 90) / RAD;

                double sep = a[i] / 25362;

                double x = sep * Sin(ang);
                double y = sep * Cos(ang) * Sin(-de / RAD);

                double dist = Sqrt(x * x + y * y);

                double posAngle = To360(Atan2(x, y) * RAD + pa + 90);

                positions[i] = new CrdsRectangular(-dist * Sin((posAngle - 180) / RAD), dist * Cos((posAngle - 180) / RAD), 0);
            }

            return positions;
        }
    }
}
