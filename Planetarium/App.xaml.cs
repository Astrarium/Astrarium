using ADK;
using Ninject;
using Planetarium.Config;
using Planetarium.Renderers;
using Planetarium.Types;
using Planetarium.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
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

            Dispatcher.UnhandledException += (s, ea) =>
            {
                kernel.Get<IViewManager>().ShowMessageBox("Error", $"An unhandled exception occurred:\n\n{ea.Exception.Message}\nStack trace:\n\n{ea.Exception.StackTrace}", MessageBoxButton.OK);
                ea.Handled = true;
            };

            kernel.Get<IViewManager>().ShowWindow<MainVM>();
        }

        private void Dispatcher_UnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void ConfigureContainer()
        {
            var settings = new Settings();
            kernel.Bind<ISettings, Settings>().ToConstant(settings).InSingletonScope();

            kernel.Bind<SettingsConfig>().ToSelf().InSingletonScope();
            kernel.Bind<ToolbarButtonsConfig>().ToSelf().InSingletonScope();

            SettingsConfig settingsConfig = kernel.Get<SettingsConfig>();
            ToolbarButtonsConfig toolbarButtonsConfig = kernel.Get<ToolbarButtonsConfig>();



            // TODO: consider more proper way to load plugins
            string homeFolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            IEnumerable<string> pluginPaths = Directory.EnumerateFiles(homeFolder, "*.dll");

            foreach (string path in pluginPaths)
            {
                try
                {
                    Assembly.LoadFrom(path);
                }
                catch (Exception ex)
                {
                    // TODO: log
                }
            }

            var alltypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes());

            // collect all plugins implementations
            // TODO: to support plugin system, we need to load assemblies 
            // from the specific directory and search for plugin there
            Type[] pluginTypes = alltypes
                .Where(t => typeof(AbstractPlugin).IsAssignableFrom(t) && !t.IsAbstract)
                .ToArray();

            foreach (Type pluginType in pluginTypes)
            {
                kernel.Bind(pluginType).ToSelf().InSingletonScope();
                var plugin = kernel.Get(pluginType) as AbstractPlugin;

                // add settings configurations
                settingsConfig.AddRange(plugin.SettingItems);

                // add configured toolbar buttons
                toolbarButtonsConfig.AddRange(plugin.ToolbarItems);
            }

            // set settings defaults 
            foreach (SettingItem item in settingsConfig)
            {
                settings.Set(item.Name, item.DefaultValue);
            }

            
          
            settings.Load();

            // collect all calculators implementations
            // TODO: to support plugin system, we need to load assemblies 
            // from the specific directory and search for calculators there
            Type[] calcTypes = alltypes
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
            Type[] rendererTypes = alltypes
                .Where(t => typeof(BaseRenderer).IsAssignableFrom(t) && !t.IsAbstract)
                .ToArray();



            foreach (Type rendererType in rendererTypes)
            {
                kernel.Bind(rendererType).ToSelf().InSingletonScope();
            }

            // collect all event provider implementations
            // TODO: to support plugin system, we need to load assemblies 
            // from the specific directory and search for providers there
            Type[] eventProviderTypes = alltypes
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

            var eventProviders = eventProviderTypes
                .Select(c => kernel.Get(c))
                .Cast<BaseAstroEventsProvider>()
                .ToArray();

            SkyContext context = new SkyContext(
                new Date(DateTime.Now).ToJulianEphemerisDay(),
                new CrdsGeographical(settings.Get<CrdsGeographical>("ObserverLocation")));

            kernel.Bind<Sky, ISearcher, IEphemerisProvider>().ToConstant(new Sky(context, calculators, eventProviders)).InSingletonScope();

            RenderersCollection renderers = new RenderersCollection(rendererTypes
                .Select(r => kernel.Get(r))
                .Cast<BaseRenderer>()
                .OrderBy(r => r.ZOrder));

            settings.Set("RenderingOrder", renderers.Select(r => new RendererDescription(r)).ToList());

            kernel.Bind<ISkyMap>().ToConstant(new SkyMap(context, renderers)).InSingletonScope();
            kernel.Bind<IViewManager>().ToConstant(new ViewManager(t => kernel.Get(t))).InSingletonScope();
        }
    }
}
