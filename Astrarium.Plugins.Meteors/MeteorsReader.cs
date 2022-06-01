using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Meteors
{
    [Singleton(typeof(IMeteorsReader))]
    public class MeteorsReader : IMeteorsReader
    {
        public ICollection<Meteor> Read(string filePath)
        {
            var ci = CultureInfo.InvariantCulture;
            List<Meteor> meteors = new List<Meteor>();
            using (var sr = new StreamReader(new FileStream(filePath, FileMode.Open, FileAccess.Read)))
            {
                string line = "";
                while ((line = sr.ReadLine()) != null)
                {
                    string code = line.Substring(0, 3);
                    string name = line.Substring(4, 21);
                    string[] begin = line.Substring(25, 5).Split('/');
                    string[] end = line.Substring(33, 5).Split('/');
                    string[] max = line.Substring(39, 5).Split('/');
                    string zhr = line.Substring(88, 3).Trim();
                    string activityClass = line.Substring(92, 1);

                    short beginDay = (short)Date.DayOfYear(new Date(2001, int.Parse(begin[1]), int.Parse(begin[0])));
                    short endDay = (short)Date.DayOfYear(new Date(2001, int.Parse(end[1]), int.Parse(end[0])));
                    short maxDay = (short)Date.DayOfYear(new Date(2001, int.Parse(max[1]), int.Parse(max[0])));

                    if (beginDay > endDay)
                        beginDay -= 365;

                    if (maxDay > endDay)
                        maxDay -= 365;

                    double ra = double.Parse(line.Substring(52, 5), ci);
                    double dec = double.Parse(line.Substring(58, 5), ci);
                    double dRa = double.Parse(line.Substring(63, 7), ci);
                    double dDec = double.Parse(line.Substring(70, 7), ci);

                    var meteor = new Meteor()
                    {
                        Code = code,
                        Name = name.Trim(),
                        Begin = beginDay,
                        End = endDay,
                        Max = maxDay,
                        Equatorial0 = new CrdsEquatorial(ra, dec),
                        Drift = new CrdsEquatorial(dRa, dDec),
                        ZHR = zhr,
                        ActivityClass = int.Parse(activityClass)
                    };
                    meteors.Add(meteor);
                }
            }

            return meteors;
        }
    }
}
