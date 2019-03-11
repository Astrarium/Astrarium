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
using System.Linq;
using System.Reflection;
using System.Windows;

namespace Planetarium
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, IViewManager
    {
        IKernel kernel = new StandardKernel();

        Dictionary<Type, Type> viewModelViewBindings = new Dictionary<Type, Type>();

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            ConfigureContainer();
            ComposeObjects();

            Current.MainWindow.Show();
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

            var sky = new Sky(context, calculators, eventProviders);

            kernel.Bind<Sky, ISearcher>().ToConstant(sky).InSingletonScope();
            kernel.Bind<ISkyMap>().ToConstant(new SkyMap(context, renderers));

            ResolveViewModelViewBindings();

            kernel.Bind<IViewManager>().ToConstant(this).InSingletonScope();
        }

        private void ComposeObjects()
        {
            Current.MainWindow = kernel.Get<MainWindow>();
        }

        public void ShowWindow<TViewModel>() where TViewModel : ViewModelBase
        {
            Show<TViewModel>(viewModel: null, isDialog: true);
        }

        public bool? ShowDialog<TViewModel>() where TViewModel : ViewModelBase
        {
            return Show<TViewModel>(viewModel: null, isDialog: true);
        }

        public bool? Show<TViewModel>(TViewModel viewModel, bool isDialog) where TViewModel : ViewModelBase
        {
            if (viewModel == null)
            {
                viewModel = CreateViewModel<TViewModel>();
            }
          
            Type viewType = null;
            if (viewModelViewBindings.ContainsKey(typeof(TViewModel)))
            {
                viewType = viewModelViewBindings[typeof(TViewModel)];
            }

            if (viewType != null)
            {
                var window = kernel.Get(viewType) as Window;
                window.DataContext = viewModel;
                window.Owner = Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);

                Action<bool?> viewModelClosingHandler = null;
                viewModelClosingHandler = (dialogResult) =>
                {
                    if (isDialog)
                    {
                        window.DialogResult = dialogResult;
                    }
                    else
                    {
                        window.Close();
                    }

                    if (viewModel is ViewModelBase)
                    {
                        (viewModel as ViewModelBase).Closing -= viewModelClosingHandler;
                    }
                };

                if (viewModel is ViewModelBase)
                {
                    (viewModel as ViewModelBase).Closing += viewModelClosingHandler;
                }

                if (isDialog)
                {
                    return window.ShowDialog();
                }
                else
                {
                    window.Show();
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        private void ResolveViewModelViewBindings()
        {
            Type[] viewModelTypes =
                    Assembly.GetExecutingAssembly().GetTypes().Concat(
                    Assembly.GetExecutingAssembly().GetReferencedAssemblies()
                 .SelectMany(a => Assembly.Load(a).GetTypes()))
                 .Where(t => typeof(ViewModelBase).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)
                 .ToArray();

            Type[] viewTypes =
                Assembly.GetExecutingAssembly().GetTypes().Concat(
                Assembly.GetExecutingAssembly().GetReferencedAssemblies()
                .SelectMany(a => Assembly.Load(a).GetTypes()))
                .Where(t => typeof(Window).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)
                .ToArray();

            foreach (Type viewModelType in viewModelTypes)
            {
                string viewModelName = viewModelType.Name;
                if (viewModelName.EndsWith("VM"))
                {
                    viewModelName = viewModelName.Substring(0, viewModelName.Length - "VM".Length);
                }
                if (viewModelName.EndsWith("ViewModel"))
                {
                    viewModelName = viewModelName.Substring(0, viewModelName.Length - "ViewModel".Length);
                }

                Type viewType = viewTypes.FirstOrDefault(t =>
                    t.Name == $"{viewModelName}" ||
                    t.Name == $"{viewModelName}Window" ||
                    t.Name == $"{viewModelName}View");

                if (viewType != null)
                {
                    viewModelViewBindings.Add(viewModelType, viewType);
                }
            }
        }

        public TViewModel CreateViewModel<TViewModel>() where TViewModel : ViewModelBase
        {
            return kernel.Get<TViewModel>();
        }

        public void ShowWindow<TViewModel>(TViewModel viewModel) where TViewModel : ViewModelBase
        {
            Show(viewModel: viewModel, isDialog: false);
        }

        public bool? ShowDialog<TViewModel>(TViewModel viewModel) where TViewModel : ViewModelBase
        {
            return Show(viewModel: viewModel, isDialog: true);
        }

        MessageBoxResult IViewManager.ShowMessageBox(string caption, string text, MessageBoxButton buttons)
        {
            var dialog = kernel.Get<MessageBoxWindow>();
            dialog.Owner = Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            dialog.Title = caption;
            dialog.MessageContainer.Text = text;
            dialog.Buttons = buttons;
            dialog.ShowDialog();
            return dialog.Result;
        }


    }
}
