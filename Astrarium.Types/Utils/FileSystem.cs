using System;
using System.IO;

namespace Astrarium.Types.Utils
{
    public static class FileSystem
    {
        public static bool DeleteFile(string fullPath)
        {
            try
            {
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
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
                if (Directory.Exists(fullPath))
                {
                    Directory.Delete(fullPath, recursive: true);
                }
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
