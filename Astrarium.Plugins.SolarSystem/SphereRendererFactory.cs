using Astrarium.Types;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.SolarSystem
{
    internal class SphereRendererFactory
    {
        public BaseSphereRenderer CreateRenderer()
        {
            try
            {
                //using (var window = new GameWindow())
                //{
                //    // Version should be at least 3.0.
                //    string openGLversion = GL.GetString(StringName.Version);

                //    if (int.TryParse(openGLversion.Split(' ').First().Split('.').First(), out int majorVersion) && majorVersion >= 3)
                //    {
                //        Log.Debug($"OpenGL sphere renderer is used (OpenGL version: {openGLversion})");
                //        return new GLSphereRenderer();
                //    }
                //    else
                //    {
                //        Log.Warn($"WPF sphere renderer is used (OpenGL version: {openGLversion}). Only low-level quality texures are supported. To get high-level textures support, upgrade OpenGL to 3.0 version or higher.");
                //        return new WpfSphereRenderer();
                //    }
                //}
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
