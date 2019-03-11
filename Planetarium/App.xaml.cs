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
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
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

            kernel.Bind<Sky>().ToConstant(new Sky(context, calculators, eventProviders));
            kernel.Bind<ISkyMap>().ToConstant(new SkyMap(context, renderers));

            viewModelViewBindings.Add(typeof(SearchWindowViewModel), typeof(SearchWindow));
            viewModelViewBindings.Add(typeof(ObjectInfoWindowViewModel), typeof(ObjectInfoWindow));

            kernel.Bind<IViewManager>().ToConstant(this).InSingletonScope();
        }

        private void ComposeObjects()
        {
            Current.MainWindow = kernel.Get<MainWindow>();
        }

        void IViewManager.ShowWindow<TViewModel>()
        {
            Show<TViewModel>(model: null, initAction: null, isDialog: true);
        }

        void IViewManager.ShowWindow<TViewModel>(Action<TViewModel> initAction) 
        {
            Show(model: null, initAction: initAction, isDialog: false);
        }

        bool? IViewManager.ShowDialog<TModel>() 
        {
            return Show<TModel>(model: null, initAction: null, isDialog: true);
        }

        bool? IViewManager.ShowDialog<TViewModel>(Action<TViewModel> initAction)
        {
            return Show<TViewModel>(model: null, initAction: initAction, isDialog: true);
        }

        private bool? Show<TViewModel>(TViewModel model, Action<TViewModel> initAction, bool isDialog) where TViewModel : class
        {
            if (model == null)
            {
                model = kernel.Get<TViewModel>();
            }
            
            initAction?.Invoke(model);

            Type viewType = null;
            if (viewModelViewBindings.ContainsKey(typeof(TViewModel)))
            {
                viewType = viewModelViewBindings[typeof(TViewModel)];
            }

            if (viewType != null)
            {
                var window = kernel.Get(viewType) as Window;
                window.DataContext = model;
                window.Owner = Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);

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


        void IViewManager.ShowWindow<TModel>(TModel model)
        {
            Show(model: model, initAction: null, isDialog: false);
        }

        void IViewManager.ShowWindow<TModel>(TModel model, Action<TModel> initAction)
        {
            Show(model: model, initAction: initAction, isDialog: false);
        }

        bool? IViewManager.ShowDialog<TModel>(TModel model)
        {
            return Show(model: model, initAction: null, isDialog: true);
        }

        bool? IViewManager.ShowDialog<TModel>(TModel model, Action<TModel> initAction)
        {
            return Show(model: model, initAction: initAction, isDialog: true);
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
