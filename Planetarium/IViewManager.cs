using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Planetarium
{
    /// <summary>
    /// Defines methods to work with views (Windows and MessageBoxes) and ViewModels.
    /// </summary>
    public interface IViewManager
    {
        /// <summary>
        /// Creates new ViewModel by its type.
        /// </summary>
        /// <typeparam name="TViewModel">Type of the ViewModel.</typeparam>
        /// <returns>Instance of ViewModel type <typeparamref name="TViewModel"/>.</returns>
        TViewModel CreateViewModel<TViewModel>() where TViewModel : ViewModelBase;

        /// <summary>
        /// Shows window by its ViewModel type. 
        /// Calling this method automatically creates instance of the ViewModel and attaches it to DataContext property. />
        /// </summary>
        /// <typeparam name="TViewModel"></typeparam>
        void ShowWindow<TViewModel>() where TViewModel : ViewModelBase;
        bool? ShowDialog<TViewModel>() where TViewModel : ViewModelBase;

        void ShowWindow<TViewModel>(TViewModel viewModel) where TViewModel : ViewModelBase;
        bool? ShowDialog<TViewModel>(TViewModel viewModel) where TViewModel : ViewModelBase;
 
        MessageBoxResult ShowMessageBox(string caption, string text, MessageBoxButton buttons);
    }
}
