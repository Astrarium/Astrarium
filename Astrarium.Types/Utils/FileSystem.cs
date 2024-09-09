using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Types.Utils
{
    public static class FileSystem
    {
        public static bool DeleteFile(string fullPath)
        {
            try
            {
                File.Delete(fullPath);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"Unable to delete file {fullPath}: {ex.Message}");
                return false;
            }
        }

        public static bool DeleteDirectory(string fullPath)
        {
            try
            {
                Directory.Delete(fullPath, recursive: true);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"Unable to delete directory {fullPath}: {ex.Message}");
                return false;
            }
        }
    }
}
