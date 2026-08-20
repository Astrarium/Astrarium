using Astrarium.Types;

namespace Astrarium.ViewModels
{
    public class RenderingOrderSettingsVM : SettingsViewModel
    {
        /// <summary>
        /// Collection of renderers
        /// </summary>
        public RenderingOrder Renderers { get; private set; }

        public RenderingOrderSettingsVM(ISettings settings) : base(settings)
        {
            Renderers = settings.Get<RenderingOrder>("RenderingOrder");
        }
    }
}
