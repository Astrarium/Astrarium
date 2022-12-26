using Astrarium.Types;
using System;

namespace Astrarium.Plugins.SolarSystem
{
    internal class SphereRendererFactory
    {
        public BaseSphereRenderer CreateRenderer()
        {
            try
            {
                return new OpenGLSphereRenderer();
            }
            catch (Exception ex)
            {
                Log.Error($"Error on creating OpenGL renderer. WPF sphere renderer is used. Only low-level quality texures are supported. Exception: {ex}");
                return new WpfSphereRenderer();
            }
        }
    }
}
