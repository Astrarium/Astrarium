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

        /// <summary>
        /// Creates a relative path from one file or folder to another.
        /// </summary>
        /// <param name="fromPath">Contains the directory that defines the start of the relative path.</param>
        /// <param name="toPath">Contains the path that defines the endpoint of the relative path.</param>
        /// <returns>The relative path from the start directory to the end path.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="fromPath"/> or <paramref name="toPath"/> is <c>null</c>.</exception>
        /// <exception cref="UriFormatException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public static string GetRelativePath(string fromPath, string toPath)
        {
            if (string.IsNullOrEmpty(fromPath))
            {
                throw new ArgumentNullException("fromPath");
            }

            if (string.IsNullOrEmpty(toPath))
            {
                throw new ArgumentNullException("toPath");
            }

            Uri fromUri = new Uri(AppendDirectorySeparatorChar(fromPath));
            Uri toUri = new Uri(AppendDirectorySeparatorChar(toPath));

            if (fromUri.Scheme != toUri.Scheme)
            {
                return toPath;
            }

            Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            string relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            if (string.Equals(toUri.Scheme, Uri.UriSchemeFile, StringComparison.OrdinalIgnoreCase))
            {
                relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }

            return relativePath;
        }

        public static string GenerateNewFileName(string oldFilePath)
        {
            return Path.Combine(Path.GetDirectoryName(oldFilePath), $"{Path.GetFileNameWithoutExtension(oldFilePath)}_{Guid.NewGuid()}{Path.GetExtension(oldFilePath)}");
        }

        private static string AppendDirectorySeparatorChar(string path)
        {
            // Append a slash only if the path is a directory and does not have a slash.
            if (!Path.HasExtension(path) &&
                !path.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                return path + Path.DirectorySeparatorChar;
            }

            return path;
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
