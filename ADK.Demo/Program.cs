using ADK.Demo.Calculators;
using ADK.Demo.Config;
using ADK.Demo.Objects;
using ADK.Demo.Renderers;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ADK.Demo
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            IKernel kernel = new StandardKernel();

            kernel.Bind<ISettings, Settings>().To<Settings>().InSingletonScope();
            kernel.Bind<ITracksProvider, Sky>().To<Sky>().InSingletonScope();

            // collect all caculators implementations
            // TODO: to support plugin system, we need to load assemblies 
            // from the specific directory and search for calculators there
            Type[] calcTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => typeof(ISkyCalc).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)
                .ToArray();
                        
            foreach (Type calcType in calcTypes)
            {
                var interfaces = calcType.GetInterfaces().ToList();
                if (interfaces.Any())
                {
                    // each interface that calculator implements
                    // should be bound to the calc instance
                    interfaces.Add(calcType);
                    kernel.Bind(interfaces.ToArray()).To(calcType).InSingletonScope();
                }
            }


            Type[] rendererTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => typeof(IRenderer).IsAssignableFrom(t) && !t.IsAbstract)
                .ToArray();

            foreach (Type rendererType in rendererTypes)
            {                
                kernel.Bind(rendererType).ToSelf().InSingletonScope();                
            }

            Sky sky = kernel.Get<Sky>();

            // TODO: this should be rewritten
            // Renderers should be loaded from difference assemblies
            kernel.Bind<ISkyMap>().ToConstant(new SkyMap(sky.Context));


            ISkyMap map = kernel.Get<ISkyMap>();

            map.Renderers.Add(kernel.Get<MilkyWayRenderer>());
            map.Renderers.Add(kernel.Get<DeepSkyRenderer>());
            map.Renderers.Add(kernel.Get<ConstellationsRenderer>());
            map.Renderers.Add(kernel.Get<CelestialGridRenderer>());
            map.Renderers.Add(kernel.Get<TrackRenderer>());
            map.Renderers.Add(kernel.Get<StarsRenderer>());
            map.Renderers.Add(kernel.Get<SolarSystemRenderer>());
            map.Renderers.Add(kernel.Get<GroundRenderer>());



            foreach (Type calcType in calcTypes)
            {
                sky.Calculators.Add(kernel.Get(calcType) as ISkyCalc);
            }

            kernel.Get<Settings>().Load();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(kernel.Get<FormMain>());
        }
    }
}
