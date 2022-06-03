using Astrarium.Algorithms;
using Ninject;
using Astrarium.Config;
using Astrarium.Types;
using Astrarium.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using NLog;

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
            Log.SetImplementation((DefaultLogger)LogManager.GetLogger("", typeof(DefaultLogger)));
            if (commandLineArgs.Contains("-debug", StringComparer.OrdinalIgnoreCase))
            {
                Log.Level = "Debug";
            }

            Log.Info($"Starting Astrarium {FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion}");

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
                Log.Error(message);
                ViewManager.ShowMessageBox("Error", message, MessageBoxButton.OK);
                ea.Handled = true;
            };
        }

        protected override void OnExit(ExitEventArgs e)
        {
            CursorsHelper.SetSystemCursors();
            base.OnExit(e);
        }

        private void SetColorSchema(ColorSchema schema, string appTheme)
        {
            Current.Resources.MergedDictionaries[0].Source = new Uri($@"pack://application:,,,/Astrarium.Types;component/Themes/Colors{(schema == ColorSchema.Red ? "Red" : appTheme)}.xaml");
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
            kernel.Bind<ISettings, Settings>().To<Settings>().InSingletonScope();
            kernel.Bind<IAppUpdater>().To<AppUpdater>().InSingletonScope();
            kernel.Bind<ISky, Sky>().To<Sky>().InSingletonScope();
            kernel.Bind<ISkyMap, SkyMap>().To<SkyMap>().InSingletonScope();
            kernel.Bind<IGeoLocationsManager, GeoLocationsManager>().To<GeoLocationsManager>().InSingletonScope();
            kernel.Bind<IMainWindow, MainVM>().To<MainVM>().InSingletonScope();
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

                    // get singletons defined in plugin
                    var singletons = plugin.GetExportedTypes().Where(t => t.IsDefined(typeof(SingletonAttribute), false)).ToArray();
                    foreach (var singletonImpl in singletons)
                    {
                        var singletonAttr = singletonImpl.GetCustomAttribute<SingletonAttribute>();
                        if (singletonAttr.InterfaceType != null)
                        {
                            if (!singletonAttr.InterfaceType.IsAssignableFrom(singletonImpl))
                            {
                                throw new Exception($"Interface type {singletonAttr.InterfaceType} is not assignable from {singletonImpl}");
                            }
                            kernel.Bind(singletonAttr.InterfaceType).To(singletonImpl).InSingletonScope();
                        }
                        else
                        {
                            kernel.Bind(singletonImpl).ToSelf().InSingletonScope();
                        }
                    }

                    Log.Info($"Loaded plugin {plugin.FullName}");
                }
                catch (Exception ex)
                {
                    Log.Error($"Unable to load plugin assembly with path {path}. {ex})");
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
                kernel.Bind(types).To(rendererType).InSingletonScope();
            }

            // collect all event provider implementations
            Type[] eventProviderTypes = pluginTypes
                .SelectMany(p => GetAstroEventProviders(p))
                .ToArray();

            foreach (Type eventProviderType in eventProviderTypes)
            {
                var types = new[] { eventProviderType }.Concat(eventProviderType.GetInterfaces()).ToArray();
                kernel.Bind(types).To(eventProviderType).InSingletonScope();
            }

            foreach (Type pluginType in pluginTypes)
            {
                progress.Report($"Creating plugin {pluginType}");

                try
                {
                    // plugin is a singleton
                    kernel.Bind(pluginType).ToSelf().InSingletonScope();
                    var plugin = kernel.Get(pluginType) as AbstractPlugin;

                    // add settings definitions
                    uiIntegration.SettingDefinitions.AddRange(plugin.SettingDefinitions);

                    // add settings sections
                    uiIntegration.SettingSections.AddRange(plugin.SettingSections);

                    // add configured toolbar buttons
                    uiIntegration.ToolbarButtons.AddRange(plugin.ToolbarItems);

                    // add menu items
                    uiIntegration.MenuItems.AddRange(plugin.MenuItems);

                    plugins.Add(plugin);
                }
                catch (Exception ex)
                {
                    Log.Error($"Unable to create plugin of type {pluginType.Name}: {ex}");
                    kernel.Unbind(pluginType);
                }
            }

            progress.Report($"Creating renderers");

            var renderers = rendererTypes.Select(r => kernel.Get(r)).Cast<BaseRenderer>().ToArray();
            var defaultRenderingOrder = new RenderingOrder(renderers.OrderBy(r => r.Order).Select(r => new RenderingOrderItem(r)));
            uiIntegration.SettingDefinitions.Add(new SettingDefinition("RenderingOrder", defaultRenderingOrder, false));

            var settings = kernel.Get<ISettings>();
            settings.Define(uiIntegration.SettingDefinitions);
            settings.Save("Defaults");

            progress.Report($"Loading settings");

            settings.Load();

            SetLanguage(settings.Get<string>("Language"));
            SetColorSchema(settings.Get<ColorSchema>("Schema"), settings.Get("AppTheme", "DeepBlue"));

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

            progress.Report($"Initializing sky");
            kernel.Get<Sky>().Initialize(context, calculators, eventProviders);

            progress.Report($"Initializing sky map");
            kernel.Get<SkyMap>().Initialize(context, renderers);

            Log.Debug("Application container has been configured.");

            progress.Report($"Initializing shell");

            settings.SettingValueChanged += (settingName, value) =>
            {
                if (settingName == "Schema" || settingName == "AppTheme")
                {
                    SetColorSchema(settings.Get("Schema", ColorSchema.Night), settings.Get("AppTheme", "DeepBlue"));
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
