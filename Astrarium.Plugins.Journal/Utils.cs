using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
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

        public static string GetOpenImageDialogFilterString()
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();

            string allExt = string.Join(";", codecs.Select(c => c.FilenameExtension));

            StringBuilder sb = new StringBuilder($"All image formats|{allExt}");
            foreach (var c in codecs)
            {
                sb.Append($"|{c.FormatDescription} files ({c.FilenameExtension})|{c.FilenameExtension}");
            }
            return sb.ToString();
        }
    }
}
