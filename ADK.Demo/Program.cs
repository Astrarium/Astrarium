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

            kernel.Get<Settings>().Load();

            // TODO: get location info from settings
            SkyContext context = new SkyContext(
                new Date(DateTime.Now).ToJulianEphemerisDay(),
                new CrdsGeographical(56.3333, -44, +3));

            // collect all calculators implementations
            // TODO: to support plugin system, we need to load assemblies 
            // from the specific directory and search for calculators there
            Type[] calcTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => typeof(BaseCalc).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)
                .ToArray();
                        
            foreach (Type calcType in calcTypes)
            {
                var types = calcType.GetInterfaces().ToList();
                if (types.Any())
                {
                    // each interface that calculator implements
                    // should be bound to the calc instance
                    types.Add(calcType);
                    kernel.Bind(types.ToArray()).To(calcType).InSingletonScope();
                }
            }

            // collect all calculators implementations
            // TODO: to support plugin system, we need to load assemblies 
            // from the specific directory and search for renderers there
            Type[] rendererTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => typeof(BaseRenderer).IsAssignableFrom(t) && !t.IsAbstract)
                .ToArray();

            foreach (Type rendererType in rendererTypes)
            {                
                kernel.Bind(rendererType).ToSelf().InSingletonScope();                
            }

            // collect all event provider implementations
            // TODO: to support plugin system, we need to load assemblies 
            // from the specific directory and search for providers there
            Type[] eventProviderTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => typeof(BaseAstroEventsProvider).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)
                .ToArray();

            foreach (Type eventProviderType in eventProviderTypes)
            {
                kernel.Bind(eventProviderType).ToSelf().InSingletonScope();
            }

            var calculators = calcTypes
                .Select(c => kernel.Get(c))
                .Cast<BaseCalc>()
                .ToArray();

            var renderers = rendererTypes
                .Select(r => kernel.Get(r))
                .Cast<BaseRenderer>()
                .OrderBy(r => r.ZOrder)
                .ToArray();

            var eventProviders = eventProviderTypes
                .Select(c => kernel.Get(c))
                .Cast<BaseAstroEventsProvider>()
                .ToArray();

            kernel.Bind<Sky>().ToConstant(new Sky(context, calculators, eventProviders));
            kernel.Bind<ISkyMap>().ToConstant(new SkyMap(context, renderers));

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(kernel.Get<FormMain>());
        }
    }
}
