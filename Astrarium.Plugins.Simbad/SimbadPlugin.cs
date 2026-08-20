using Astrarium.Plugins.Simbad.ViewModels;
using Astrarium.Plugins.Simbad.Views;
using Astrarium.Types;
using System.Linq;

namespace Astrarium.Plugins.Simbad
{
    public class SimbadPlugin : AbstractPlugin
    {
        private readonly string[] SUPPORTED_BODY_TYPES = new[] { "Star", "DeepSky" };

        public SimbadPlugin()
        {
            ExtendObjectInfo<SimbadControl, SimbadVM>("Simbad", CreateSimbadViewModel);
        }

        private SimbadVM CreateSimbadViewModel(SkyContext ctx, CelestialObject body)
        {
            if (body != null && SUPPORTED_BODY_TYPES.Contains(body.Type.Split('.').First()))
            {
                return ViewManager.CreateViewModel<SimbadVM>().ForBody(body);
            }
            else
            {
                return null;
            }
        }
    }
}
