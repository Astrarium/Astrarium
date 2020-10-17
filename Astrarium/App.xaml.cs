using Astrarium.Algorithms;
using Ninject;
using Astrarium.Config;
using Astrarium.Logging;
using Astrarium.Types;
using Astrarium.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Threading.Tasks;
using System.Windows;
using Astrarium.Config.Controls;

namespace Astrarium
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IKernel kernel = new StandardKernel();
        private ICommandLineArgs commandLineArgs = null;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            commandLineArgs = new CommandLineArgs(e.Args);

            ViewManager.SetImplementation(new DefaultViewManager(t => kernel.Get(t)));

            var splashVM = new SplashScreenVM();
            ViewManager.ShowWindow(splashVM);

            // in order to ensure the UI stays responsive, we need to
            // do the work on a different thread
            Task.Factory.StartNew(() =>
            {
                ConfigureContainer(splashVM);
                Dispatcher.Invoke(() =>
                {
                    ViewManager.ShowWindow<MainVM>();
                    splashVM.Close();
                });
            });

            Dispatcher.UnhandledException += (s, ea) =>
            {
                string message = $"An unhandled exception occurred:\n\n{ea.Exception.Message}\nStack trace:\n\n{ea.Exception.StackTrace}";
                Trace.TraceError(message);
                ViewManager.ShowMessageBox("Error", message, MessageBoxButton.OK);
                ea.Handled = true;                
            };
        }

        protected override void OnExit(ExitEventArgs e)
        {
            CursorsHelper.SetSystemCursors();
            base.OnExit(e);
        }

        private void SetColorSchema(ColorSchema schema)
        {
            Current.Resources.MergedDictionaries[0].Source = new Uri($@"pack://application:,,,/Astrarium.Types;component/Themes/Colors{(schema == ColorSchema.Red ? "Red" : "Default")}.xaml");
            if (schema == ColorSchema.Red)
            {
                CursorsHelper.SetCustomCursors();
            }
            else
            {
                CursorsHelper.SetSystemCursors();
            }
        }

        private void SetLanguage(string language)
        {            
            var locale = Text.GetLocales().FirstOrDefault(loc => loc.Name == language);
            if (locale != null)
            {
                Text.SetLocale(locale);
            }
        }

        private IEnumerable<Type> GetRenderers(Type pluginType)
        {
            return pluginType.Assembly.GetTypes().Where(t => typeof(BaseRenderer).IsAssignableFrom(t) && !t.IsAbstract);
        }

        private IEnumerable<Type> GetCalculators(Type pluginType)
        {
            return pluginType.Assembly.GetTypes().Where(t => typeof(BaseCalc).IsAssignableFrom(t) && !t.IsAbstract);
        }

        private IEnumerable<Type> GetAstroEventProviders(Type pluginType)
        {
            return pluginType.Assembly.GetTypes().Where(t => typeof(BaseAstroEventsProvider).IsAssignableFrom(t) && !t.IsAbstract);
        }

        private void ConfigureContainer(IProgress<string> progress)
        {
            kernel.Bind<ICommandLineArgs>().ToConstant(commandLineArgs).InSingletonScope();

            // use single logger for whole application
            kernel.Bind<Logger>().ToSelf().InSingletonScope();
            kernel.Get<Logger>();

            Debug.WriteLine($"Starting Astrarium {FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion}");

            kernel.Bind<ISettings, Settings>().To<Settings>().InSingletonScope();

            kernel.Bind<ISky, Sky>().To<Sky>().InSingletonScope();
            kernel.Bind<ISkyMap, SkyMap>().To<SkyMap>().InSingletonScope();

            kernel.Bind<UIElementsIntegration>().ToSelf().InSingletonScope();
            UIElementsIntegration uiIntegration = kernel.Get<UIElementsIntegration>();

            ICollection<AbstractPlugin> plugins = new List<AbstractPlugin>();

            string homeFolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            IEnumerable<string> pluginPaths = Directory.EnumerateFiles(homeFolder, "Astrarium.Plugins.*.dll", SearchOption.AllDirectories);

            progress.Report("Loading plugins");

            foreach (string path in pluginPaths)
            {
                try
                {
                    var plugin = Assembly.UnsafeLoadFrom(path);
                    Debug.WriteLine($"Loaded plugin {plugin.FullName}");
                }
                catch (Exception ex)
                {
                    Trace.TraceError($"Unable to load plugin assembly with path {path}. {ex})");
                }
            }

            // collect all plugins implementations            
            Type[] pluginTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => typeof(AbstractPlugin).IsAssignableFrom(t) && !t.IsAbstract)
                .ToArray();

            // collect all calculators types
            Type[] calcTypes = pluginTypes
                .SelectMany(p => GetCalculators(p))
                .ToArray();

            foreach (Type calcType in calcTypes)
            {
                var types = new[] { calcType }.Concat(calcType.GetInterfaces()).ToArray();
                kernel.Bind(types).To(calcType).InSingletonScope();
            }

            // collect all renderers types
            Type[] rendererTypes = pluginTypes
                .SelectMany(p => GetRenderers(p))
                .ToArray();

            foreach (Type rendererType in rendererTypes)
            {
                var types = new[] { rendererType }.Concat(rendererType.GetInterfaces()).ToArray();
                kernel.Bind(rendererType).ToSelf().InSingletonScope();
            }

            // collect all event provider implementations
            Type[] eventProviderTypes = pluginTypes
                .SelectMany(p => GetAstroEventProviders(p))
                .ToArray();

            foreach (Type eventProviderType in eventProviderTypes)
            {
                kernel.Bind(eventProviderType).ToSelf().InSingletonScope();
            }

            foreach (Type pluginType in pluginTypes)
            {
                progress.Report($"Creating plugin {pluginType}");

                kernel.Bind(pluginType).ToSelf().InSingletonScope();
                var plugin = kernel.Get(pluginType) as AbstractPlugin;

                // add settings configurations
                uiIntegration.SettingItems.AddRange(plugin.SettingItems);

                // add configured toolbar buttons
                uiIntegration.ToolbarButtons.AddRange(plugin.ToolbarItems);

                // add menu items
                uiIntegration.MenuItems.AddRange(plugin.MenuItems);

                plugins.Add(plugin);                
            }

            // Default rendering order for BaseRenderer descendants.
            uiIntegration.SettingItems.Add("Rendering", new SettingItem("RenderingOrder", new RenderingOrder(), typeof(RenderersListSettingControl)));

            var settings = kernel.Get<ISettings>();

            // set settings defaults 
            foreach (string group in uiIntegration.SettingItems.Groups)
            {
                foreach (var item in uiIntegration.SettingItems[group])
                {
                    settings.Set(item.Name, item.DefaultValue);
                }
            }

            settings.Save("Defaults");

            progress.Report($"Loading settings");

            settings.Load();

            SetLanguage(settings.Get<string>("Language"));
            SetColorSchema(settings.Get<ColorSchema>("Schema"));

            SkyContext context = new SkyContext(
                new Date(DateTime.Now).ToJulianEphemerisDay(),
                new CrdsGeographical(settings.Get<CrdsGeographical>("ObserverLocation")));

            progress.Report($"Creating calculators");

            var calculators = calcTypes
                .Select(c => kernel.Get(c))
                .Cast<BaseCalc>()
                .ToArray();

            progress.Report($"Creating event providers");

            var eventProviders = eventProviderTypes
                .Select(c => kernel.Get(c))
                .Cast<BaseAstroEventsProvider>()
                .ToArray();

            progress.Report($"Creating renderers");

            var renderers = rendererTypes.Select(r => kernel.Get(r)).Cast<BaseRenderer>().ToArray();

            kernel.Get<Sky>().Initialize(context, calculators, eventProviders);

            kernel.Get<SkyMap>().Initialize(context, renderers);

            Debug.Write("Application container has been configured.");

            progress.Report($"Initializing shell");

            settings.SettingValueChanged += (settingName, value) =>
            {
                if (settingName == "Schema")
                {
                    SetColorSchema((ColorSchema)value);
                }
                else if (settingName == "Language")
                {
                    SetLanguage((string)value);
                }
            };

            foreach (var plugin in plugins)
            {
                plugin.Initialize();
            }
        }
    }
}
