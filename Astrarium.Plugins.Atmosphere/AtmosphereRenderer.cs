using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Atmosphere
{
    public class AtmosphereRenderer : BaseRenderer
    {
        public override RendererOrder Order => RendererOrder.Terrestrial;

        private readonly AtmosphereCalculator calc;
        private readonly ISettings settings;

        public AtmosphereRenderer(AtmosphereCalculator calc, ISettings settings)
        {
            this.calc = calc;
            this.settings = settings;
        }

        public override void Render(IMapContext map)
        {
            throw new NotImplementedException();
        }

        public override void Render(ISkyMap map)
        {
            


        }
    }
}
