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
        public static CrdsRectangular[] Positions(double jd, double de, double pa)
        {
            CrdsRectangular[] positions = new CrdsRectangular[5];
            for (int i=0; i<positions.Length; i++)
            {
                positions[i] = new CrdsRectangular();
            }

            var RAD = 180 / PI;

            var miranda_sep = 9.4;
            var miranda_elong = 2455076.482;
            var ariel_sep = 13.9;
            var ariel_elong = 2455077.921;
            var umbriel_sep = 18.5;
            var umbriel_elong = 2455078.29;
            var titania_sep = 31.5;
            var titania_elong = 2455083.875;
            var oberon_sep = 42.2;
            var oberon_elong = 2455076.287;

            var miranda_rad = miranda_sep;
            var ariel_rad = ariel_sep;
            var umbriel_rad = umbriel_sep;
            var titania_rad = titania_sep;
            var oberon_rad = oberon_sep;

            var temp = jd;

            var miranda_ang = To360((temp - miranda_elong) / 1.4135 * 360.0 - 90) / RAD;
            var ariel_ang = To360((temp - ariel_elong) / 2.5204 * 360.0 - 90) / RAD;
            var umbriel_ang = To360((temp - umbriel_elong) / 4.1442 * 360.0 - 90) / RAD;
            var titania_ang = To360((temp - titania_elong) / 8.7059 * 360.0 - 90) / RAD;
            var oberon_ang = To360((temp - oberon_elong) / 13.4632 * 360.0 - 90) / RAD;

            var x_miranda = miranda_rad * Sin(miranda_ang);
            var y_miranda = miranda_rad * Cos(miranda_ang) * Sin(de / RAD);
            var x_ariel = ariel_rad * Sin(ariel_ang);
            var y_ariel = ariel_rad * Cos(ariel_ang) * Sin(de / RAD);
            var x_umbriel = umbriel_rad * Sin(umbriel_ang);
            var y_umbriel = umbriel_rad * Cos(umbriel_ang) * Sin(de / RAD);
            var x_titania = titania_rad * Sin(titania_ang);
            var y_titania = titania_rad * Cos(titania_ang) * Sin(de / RAD);
            var x_oberon = oberon_rad * Sin(oberon_ang);
            var y_oberon = oberon_rad * Cos(oberon_ang) * Sin(de / RAD);

            var miranda_dist = Sqrt(Pow(x_miranda, 2) + Pow(y_miranda, 2));
            var ariel_dist = Sqrt(Pow(x_ariel, 2) + Pow(y_ariel, 2));
            var umbriel_dist = Sqrt(Pow(x_umbriel, 2) + Pow(y_umbriel, 2));
            var titania_dist = Sqrt(Pow(x_titania, 2) + Pow(y_titania, 2));
            var oberon_dist = Sqrt(Pow(x_oberon, 2) + Pow(y_oberon, 2));

            var miranda_pos_ang = To360(Atan2(x_miranda, y_miranda) * RAD + pa - 180);
            var ariel_pos_ang = To360(Atan2(x_ariel, y_ariel) * RAD + pa - 180);
            var umbriel_pos_ang = To360(Atan2(x_umbriel, y_umbriel) * RAD + pa - 180);
            var titania_pos_ang = To360(Atan2(x_titania, y_titania) * RAD + pa - 180);
            var oberon_pos_ang = To360(Atan2(x_oberon, y_oberon) * RAD + pa - 180);

            positions[0].X = miranda_dist * Sin((miranda_pos_ang - 180) / RAD);
            positions[0].Y = miranda_dist * Cos((miranda_pos_ang - 180) / RAD);
            positions[1].X = ariel_dist * Sin((ariel_pos_ang - 180) / RAD);
            positions[1].Y = ariel_dist * Cos((ariel_pos_ang - 180) / RAD);
            positions[2].X = umbriel_dist * Sin((umbriel_pos_ang - 180) / RAD);
            positions[2].Y = umbriel_dist * Cos((umbriel_pos_ang - 180) / RAD);
            positions[3].X = titania_dist * Sin((titania_pos_ang - 180) / RAD);
            positions[3].Y = titania_dist * Cos((titania_pos_ang - 180) / RAD);
            positions[4].X = oberon_dist * Sin((oberon_pos_ang - 180) / RAD);
            positions[4].Y = oberon_dist * Cos((oberon_pos_ang - 180) / RAD);

            return positions;
        }
    }
}
