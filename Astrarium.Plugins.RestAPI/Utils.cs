using Astrarium.Types;

namespace Astrarium.Plugins.RestAPI
{
    internal class Utils
    {
        public static string GetRestAPIServerBaseAddress(ISettings settings)
        {
            return $"http://localhost:{(int)settings.Get<decimal>(SettingsNames.RestAPIServerPort)}";
        }
    }
}
