using Planetarium.Renderers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium
{
    public class RendererDescription
    {
        public string Name { get; set; }
        public Type RendererType { get; set; }

        public RendererDescription() { }
        public RendererDescription(BaseRenderer renderer)
        {
            Name = renderer.Name;
            RendererType = renderer.GetType();
            // TODO: include version, description, author name, and etc.
        }
    }
}
