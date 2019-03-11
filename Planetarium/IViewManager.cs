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
        void ShowWindow<TModel>() where TModel : class;
        void ShowWindow<TModel>(Action<TModel> initAction) where TModel : class;

        bool? ShowDialog<TModel>() where TModel : class;
        bool? ShowDialog<TModel>(Action<TModel> initAction) where TModel : class;

        void ShowWindow<TModel>(TModel model) where TModel : class;
        void ShowWindow<TModel>(TModel model, Action<TModel> initAction) where TModel : class;

        bool? ShowDialog<TModel>(TModel model) where TModel : class;
        bool? ShowDialog<TModel>(TModel model, Action<TModel> initAction) where TModel : class;

        MessageBoxResult ShowMessageBox(string caption, string text, MessageBoxButton buttons);
    }
}
