using ADK;
using ADK.Demo;
using ADK.Demo.Calculators;
using ADK.Demo.Config;
using ADK.Demo.Renderers;
using Ninject;
using Planetarium.ViewModels;
using Planetarium.Views;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace Planetarium
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IKernel kernel = new StandardKernel();

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            ConfigureContainer();

            kernel.Get<IViewManager>().ShowWindow<MainVM>();
            //kernel.Get<IViewManager>().ShowWindow<SettingsVM>();
        }

        private void ConfigureContainer()
        {
            kernel.Bind<ISettings, Settings>().To<Settings>().InSingletonScope();

            kernel.Get<Settings>().Load();

            // TODO: get location info from settings
            SkyContext context = new SkyContext(
                new Date(DateTime.Now).ToJulianEphemerisDay(),
                new CrdsGeographical(56.3333, -44, +3));

            // collect all calculators implementations
            // TODO: to support plugin system, we need to load assemblies 
            // from the specific directory and search for calculators there
            Type[] calcTypes = Assembly.GetExecutingAssembly().GetReferencedAssemblies()
                .SelectMany(a => Assembly.Load(a).GetTypes())
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
            Type[] rendererTypes = Assembly.GetExecutingAssembly().GetReferencedAssemblies()
                .SelectMany(a => Assembly.Load(a).GetTypes())
                .Where(t => typeof(BaseRenderer).IsAssignableFrom(t) && !t.IsAbstract)
                .ToArray();

            foreach (Type rendererType in rendererTypes)
            {
                kernel.Bind(rendererType).ToSelf().InSingletonScope();
            }

            // collect all event provider implementations
            // TODO: to support plugin system, we need to load assemblies 
            // from the specific directory and search for providers there
            Type[] eventProviderTypes = Assembly.GetExecutingAssembly().GetReferencedAssemblies()
                .SelectMany(a => Assembly.Load(a).GetTypes())
                .Where(t => typeof(BaseAstroEventsProvider).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)
                .ToArray();


            var settings = kernel.Get<ISettings>();
            SettingsConfig settingsConfig = new SettingsConfig();

            settingsConfig.Add("Grids", "EquatorialGrid", true);
            settingsConfig.Add<bool>("Grids", "LabelEquatorialPoles").EnabledWhenTrue("EquatorialGrid");

            settingsConfig.Add("Grids", "HorizontalGrid", true);
            settingsConfig.Add<bool>("Grids", "LabelHorizontalPoles").EnabledWhenTrue("HorizontalGrid");

            settingsConfig.Add("Grids", "EclipticLine", true);
            settingsConfig.Add<bool>("Grids", "LabelEquinoxPoints").EnabledWhenTrue("EclipticLine");
            settingsConfig.Add<bool>("Grids", "LabelLunarNodes").EnabledWhenTrue("EclipticLine");

            settingsConfig.Add("Grids", "EclipticColorNight", Color.FromArgb(0xC88080));

            settingsConfig.Add("Grids", "GalacticEquator", true);
            settingsConfig.Add("Grids", "MilkyWay", true);
            settingsConfig.Add("Grids", "Ground", true);
            settingsConfig.Add("Grids", "HorizonLine", true);
            settingsConfig.Add("Grids", "LabelCardinalDirections", true).EnabledWhenTrue("HorizonLine");



            settingsConfig.Add("Sun", "Sun", true);
            settingsConfig.Add("Sun", "LabelSun", true).EnabledWhenTrue("Sun");
            settingsConfig.Add("Sun", "TextureSun", true).EnabledWhenTrue("Sun");
            settingsConfig.Add("Sun", "TextureSunPath", "https://sohowww.nascom.nasa.gov/data/realtime/hmi_igr/1024/latest.jpg")
                .EnabledWhenTrue("Sun")
                .WithBuilder(typeof(FilePathSettingControlBuilder));

            settingsConfig.Add("Sun", "TestSetting", "some value")
                .EnabledWhenTrue("Sun");

            settingsConfig.Add("Sun", "ConstLabelsType", ConstellationsRenderer.LabelType.InternationalName)
                .EnabledWhenTrue("Sun")
                .WithBuilder(typeof(DropdownSettingControlBuilder));

            settingsConfig.Add("Sun", "testfont", System.Drawing.SystemFonts.DefaultFont)
                .EnabledWhenTrue("Sun");

            settingsConfig.Add("Constellations", "ConstLabels", true);
            settingsConfig.Add("Constellations", "ConstLabelsType", ConstellationsRenderer.LabelType.InternationalName).EnabledWhenTrue("ConstLabels");
            settingsConfig.Add("Constellations", "ConstLines", true);
            settingsConfig.Add("Constellations", "ConstBorders", true);

            settingsConfig.Add("Stars", "Stars", true);
            settingsConfig.Add("Stars", "StarsLabels", true).EnabledWhenTrue("Stars");
            settingsConfig.Add("Stars", "StarsProperNames", true).EnabledWhenTrue("Stars");



            kernel.Bind<ISettingsConfig, SettingsConfig>().ToConstant(settingsConfig).InSingletonScope();

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

            kernel.Bind<Sky, ISearcher>().ToConstant(new Sky(context, calculators, eventProviders)).InSingletonScope();
            kernel.Bind<ISkyMap>().ToConstant(new SkyMap(context, renderers)).InSingletonScope();
            kernel.Bind<IViewManager>().ToConstant(new ViewManager(t => kernel.Get(t))).InSingletonScope();
        }

    }
}
