using Microsoft.Win32;
using Astrarium.Objects;
using Astrarium.Types;
using Astrarium.ViewModels;
using Astrarium.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using WF = System.Windows.Forms;
using System.Drawing.Printing;

namespace Astrarium
{
    /// <summary>
    /// Default implementation of the <see cref="IViewManager"/> interface.
    /// </summary>
    internal class DefaultViewManager : IViewManager
    {
        /// <summary>
        /// Dictionary of ViewModel <=> View types bindings.
        /// </summary>
        private Dictionary<Type, Type> viewModelViewBindings = new Dictionary<Type, Type>();

        /// <summary>
        /// Factory method to create instances of requested types. 
        /// IoC container method should be passed here.
        /// </summary>
        private Func<Type, object> typeFactory;

        internal DefaultViewManager(Func<Type, object> typeFactory)
        {
            this.typeFactory = typeFactory;
        }

        public void ShowWindow<TViewModel>() where TViewModel : ViewModelBase
        {
            Show<TViewModel>(viewModel: null, isDialog: false);
        }

        public bool? ShowDialog<TViewModel>() where TViewModel : ViewModelBase
        {
            return Show<TViewModel>(viewModel: null, isDialog: true);
        }

        private bool? Show<TViewModel>(TViewModel viewModel, bool isDialog = false) where TViewModel : ViewModelBase
        {
            bool needDisposeModel = false;

            if (viewModel == null)
            {
                needDisposeModel = true;
                viewModel = CreateViewModel<TViewModel>();
            }

            Type viewType = null;
            if (viewModelViewBindings.ContainsKey(typeof(TViewModel)))
            {
                viewType = viewModelViewBindings[typeof(TViewModel)];
            }
            else
            {
                viewType = ResolveVVMBindings(typeof(TViewModel));
            }            

            if (viewType != null)
            {
                var window = typeFactory(viewType) as Window;
                window.DataContext = viewModel;

                if (window.GetType() != typeof(MainWindow))
                {
                    window.Owner = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                }

                Action<bool?> viewModelClosingHandler = null;
                viewModelClosingHandler = (dialogResult) =>
                {
                    if (isDialog)
                    {
                        window.DialogResult = dialogResult;
                    }
                    
                    window.Close();

                    if (viewModel is ViewModelBase)
                    {
                        (viewModel as ViewModelBase).Closing -= viewModelClosingHandler;
                    }

                    if (needDisposeModel)
                    {
                        viewModel.Dispose();
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

        private IEnumerable<T> FindChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                var children = LogicalTreeHelper.GetChildren(depObj).OfType<DependencyObject>();
                foreach (var child in children)
                {
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        private Type ResolveVVMBindings(Type type)
        {
            var assembly = type.Assembly;

            Type[] viewModelTypes = assembly.GetTypes()
                 .Where(t => typeof(ViewModelBase).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)
                 .ToArray();

            Type[] viewTypes = assembly.GetTypes()
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

            if (viewModelViewBindings.ContainsKey(type))
            {
                return viewModelViewBindings[type];
            }
            else
            {
                return null;
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

        public MessageBoxResult ShowMessageBox(string caption, string text, MessageBoxButton buttons)
        {
            var dialog = typeFactory(typeof(MessageBoxWindow)) as MessageBoxWindow;
            dialog.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            dialog.Title = caption;
            dialog.MessageContainer.Text = text;
            dialog.Buttons = buttons;
            dialog.ShowDialog();
            return dialog.Result;
        }

        public void ShowProgress(string caption, string text, CancellationTokenSource tokenSource, Progress<double> progress = null)
        {
            var dialog = typeFactory(typeof(ProgressWindow)) as ProgressWindow;
            dialog.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            dialog.Title = caption;
            dialog.Text = text;
            dialog.CancellationTokenSource = tokenSource;
            dialog.Progress = progress;
            dialog.Show();
        }

        public string ShowSaveFileDialog(string caption, string fileName, string extension, string filter)
        {
            var dialog = new SaveFileDialog();
            dialog.Title = caption;
            dialog.FileName = fileName;
            dialog.DefaultExt = extension;
            dialog.Filter = filter;
            return (dialog.ShowDialog() ?? false) ? dialog.FileName : null;
        }

        public string ShowOpenFileDialog(string caption, string filter)
        {
            var dialog = new OpenFileDialog();
            dialog.Title = caption;
            dialog.Filter = filter;
            return (dialog.ShowDialog() ?? false) ? dialog.FileName : null;
        }

        public bool ShowPrintDialog(PrintDocument document)
        {
            var dialog = new WF.PrintDialog(); 
            dialog.Document = document;
            dialog.UseEXDialog = true;
            return dialog.ShowDialog() == WF.DialogResult.OK;
        }

        public bool ShowPrintPreviewDialog(PrintDocument document)
        {
            var dialog = new WF.PrintPreviewDialog();
            dialog.Document = document;
            dialog.ShowIcon = false;
            return dialog.ShowDialog() == WF.DialogResult.OK;
        }

        public string ShowSelectFolderDialog(string caption, string path)
        {
            var dialog = new WF.FolderBrowserDialog();
            dialog.Description = caption;
            return (WF.DialogResult.OK == dialog.ShowDialog()) ? dialog.SelectedPath : null;
            //var vm = new SelectFolderVM();
            //vm.SelectedPath = path;
            //return (ShowDialog(vm) ?? false) ? vm.SelectedPath : null;
        }

        public double? ShowDateDialog(double jd, double utcOffset, DateOptions displayMode = DateOptions.DateTime)
        {
            var vm = new DateVM(jd, utcOffset, displayMode);
            return (ShowDialog(vm) ?? false) ? (double?)vm.JulianDay : null;
        }

        public CelestialObject ShowSearchDialog(Func<CelestialObject, bool> filter = null)
        {
            var vm = CreateViewModel<SearchVM>();
            if (filter != null)
            {
                vm.Filter = filter;
            }

            return (ShowDialog(vm) ?? false) ? vm.SelectedItem.Body : null;
        }

        public TimeSpan? ShowTimeSpanDialog(TimeSpan timeSpan)
        {
            var vm = CreateViewModel<TimeSpanVM>();
            vm.TimeSpan = timeSpan;
            return (ShowDialog(vm) ?? false) ? (TimeSpan?)vm.TimeSpan : null;
        }
    }
}
