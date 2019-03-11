using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Planetarium
{
    public interface IViewManager
    {
        TViewModel CreateViewModel<TViewModel>() where TViewModel : ViewModelBase;

        void ShowWindow<TViewModel>() where TViewModel : ViewModelBase;
        bool? ShowDialog<TViewModel>() where TViewModel : ViewModelBase;

        void ShowWindow<TViewModel>(TViewModel viewModel) where TViewModel : ViewModelBase;
        bool? ShowDialog<TViewModel>(TViewModel viewModel) where TViewModel : ViewModelBase;
 
        MessageBoxResult ShowMessageBox(string caption, string text, MessageBoxButton buttons);
    }
}
