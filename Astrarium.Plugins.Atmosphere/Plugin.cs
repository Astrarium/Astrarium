using Astrarium.Types;
using System;

namespace Astrarium.Plugins.Atmosphere
{
    public class Plugin : AbstractPlugin
    {
        public Plugin(ISettings settings)
        {
            DefineSetting("Atmosphere", true);

            ExportResourceDictionaries("Images.xaml");

            ToolbarItems.Add("Ground", new ToolbarToggleButton("IconAtmosphere", "$Settings.Atmosphere", new SimpleBinding(settings, "Atmosphere", "IsChecked")));
        }
    }
}
