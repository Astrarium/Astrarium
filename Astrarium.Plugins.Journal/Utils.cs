using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Journal
{
    public static class Utils
    {
        public static bool ArePathsEqual(string path1, string path2)
        {
            path1 = path1.Replace('\\', Path.PathSeparator).Replace('/', Path.PathSeparator);
            path2 = path2.Replace('\\', Path.PathSeparator).Replace('/', Path.PathSeparator);
            return string.Compare(
                Path.GetFullPath(path1).TrimEnd(Path.PathSeparator),
                Path.GetFullPath(path2).TrimEnd(Path.PathSeparator),
                StringComparison.InvariantCultureIgnoreCase) == 0;
        }
    }
}
