using ADK;
using Ninject;
using Planetarium.Config;
using Planetarium.Logging;
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
using System.Threading.Tasks;
using System.Windows;

namespace Planetarium
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IKernel kernel = new StandardKernel();
        private ILogger logger = new Logger();
        private IViewManager viewManager;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            kernel.Bind<IViewManager>().ToConstant(new ViewManager(t => kernel.Get(t))).InSingletonScope();
            viewManager = kernel.Get<IViewManager>();

            var splashVM = new SplashScreenVM();
            viewManager.ShowWindow(splashVM);

            //in order to ensure the UI stays responsive, we need to
            //do the work on a different thread
            Task.Factory.StartNew(() =>
            {
                ConfigureContainer(splashVM);

                Dispatcher.Invoke(() =>
                {
                    viewManager.ShowWindow<MainVM>();
                    splashVM.Close();
                });
            });

            Dispatcher.UnhandledException += (s, ea) =>
            {
                string message = $"An unhandled exception occurred:\n\n{ea.Exception.Message}\nStack trace:\n\n{ea.Exception.StackTrace}";
                logger.Error(message);
                viewManager.ShowMessageBox("Error", message, MessageBoxButton.OK);
                ea.Handled = true;                
            };

        }

        private void ConfigureContainer(IProgress<string> progress)
        {
            logger.Debug("Configuring application container...");

            // use single logger for whole application
            kernel.Bind<ILogger>().ToConstant(logger).InSingletonScope();

            kernel.Bind<ISettings, Settings>().To<Settings>().InSingletonScope();

            kernel.Bind<SettingsConfig>().ToSelf().InSingletonScope();
            kernel.Bind<ToolbarButtonsConfig>().ToSelf().InSingletonScope();

            SettingsConfig settingsConfig = kernel.Get<SettingsConfig>();
            ToolbarButtonsConfig toolbarButtonsConfig = kernel.Get<ToolbarButtonsConfig>();
            ICollection<AbstractPlugin> plugins = new List<AbstractPlugin>();

            // TODO: consider more proper way to load plugins
            string homeFolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            IEnumerable<string> pluginPaths = Directory.EnumerateFiles(homeFolder, "*.dll");

            progress.Report("Loading plugins");

            foreach (string path in pluginPaths)
            {
                try
                {
                    Assembly.LoadFrom(path);
                }
                catch (Exception ex)
                {
                    logger.Error($"Unable to load plugin assembly with path {path}. {ex})");
                }
            }

            // collect all plugins implementations
            // TODO: to support plugin system, we need to load assemblies 
            // from the specific directory and search for plugin there
            Type[] pluginTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => typeof(AbstractPlugin).IsAssignableFrom(t) && !t.IsAbstract)
                .ToArray();

            foreach (Type pluginType in pluginTypes)
            {
                progress.Report($"Creating plugin {pluginType}");

                kernel.Bind(pluginType).ToSelf().InSingletonScope();
                var plugin = kernel.Get(pluginType) as AbstractPlugin;

                // add settings configurations
                settingsConfig.AddRange(plugin.SettingItems);

                // add configured toolbar buttons
                toolbarButtonsConfig.AddRange(plugin.ToolbarItems);

                plugins.Add(plugin);
            }

            // Default rendering order for BaseRenderer descendants.
            settingsConfig.Add(new SettingItem("RenderingOrder", new RenderingOrder(), "Rendering", typeof(RenderersListSettingControl)));

            var settings = kernel.Get<ISettings>();

            // set settings defaults 
            foreach (SettingItem item in settingsConfig)
            {
                settings.Set(item.Name, item.DefaultValue);
            }

            progress.Report($"Loading settings");

            settings.Load();

            var assemblyTypes = Assembly.GetExecutingAssembly().GetTypes();

            // collect all calculators types
            Type[] calcTypes = assemblyTypes
                .Where(t => typeof(BaseCalc).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)
                .Concat(plugins.SelectMany(p => p.Calculators))
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

            // collect all renderers types
            Type[] rendererTypes = assemblyTypes
                .Where(t => typeof(BaseRenderer).IsAssignableFrom(t) && !t.IsAbstract)
                .Concat(plugins.SelectMany(p => p.Renderers))
                .ToArray();

            foreach (Type rendererType in rendererTypes)
            {
                kernel.Bind(rendererType).ToSelf().InSingletonScope();
            }

            // collect all event provider implementations
            Type[] eventProviderTypes = assemblyTypes
                .Where(t => typeof(BaseAstroEventsProvider).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)
                .Concat(plugins.SelectMany(p => p.AstroEventProviders))
                .ToArray();

            foreach (Type eventProviderType in eventProviderTypes)
            {
                kernel.Bind(eventProviderType).ToSelf().InSingletonScope();
            }

            progress.Report($"Creating calculators");

            var calculators = new CalculatorsCollection(calcTypes
                .Select(c => kernel.Get(c))
                .Cast<BaseCalc>());

            progress.Report($"Creating event providers");

            var eventProviders = new AstroEventProvidersCollection(eventProviderTypes
                .Select(c => kernel.Get(c))
                .Cast<BaseAstroEventsProvider>());

            SkyContext context = new SkyContext(
                new Date(DateTime.Now).ToJulianEphemerisDay(),
                new CrdsGeographical(settings.Get<CrdsGeographical>("ObserverLocation")));

            kernel.Bind<ISkyMap>().To<SkyMap>().InSingletonScope();
            kernel.Bind<Sky, ISky, ISearcher, IEphemerisProvider>().To<Sky>().InSingletonScope();

            kernel.Bind<SkyContext>().ToConstant(context).WhenInjectedInto<ISkyMap>().InSingletonScope();
            kernel.Bind<SkyContext>().ToConstant(context).WhenInjectedInto<Sky>().InSingletonScope();
            kernel.Bind<CalculatorsCollection>().ToConstant(calculators).InSingletonScope();
            kernel.Bind<AstroEventProvidersCollection>().ToConstant(eventProviders).InSingletonScope();




            progress.Report($"Creating renderers");

            var renderers = new RenderersCollection(rendererTypes.Select(r => kernel.Get(r)).Cast<BaseRenderer>());
            kernel.Bind<RenderersCollection>().ToConstant(renderers).InSingletonScope();
           
            logger.Debug("Application container has been configured.");

            progress.Report($"Initializing shell");
        }
    }
}
