using Planetarium.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Planetarium
{
    public class ViewManager : IViewManager
    {
        private Dictionary<Type, Type> viewModelViewBindings = new Dictionary<Type, Type>();
        private Func<Type, object> typeFactory;

        public ViewManager(Func<Type, object> typeFactory)
        {
            this.typeFactory = typeFactory;
            ResolveViewModelViewBindings();
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
                var window = typeFactory(viewType) as Window;
                window.DataContext = viewModel;
                window.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);

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
            return typeFactory(typeof(TViewModel)) as TViewModel;
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
            var dialog = typeFactory(typeof(MessageBoxWindow)) as MessageBoxWindow;
            dialog.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            dialog.Title = caption;
            dialog.MessageContainer.Text = text;
            dialog.Buttons = buttons;
            dialog.ShowDialog();
            return dialog.Result;
        }
    }
}
