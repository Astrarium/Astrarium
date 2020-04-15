using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Astrarium.Types.Controls
{
    public class BindableListView : ListView
    {
        public static readonly DependencyProperty MouseDoubleClickCommandProperty =
                DependencyProperty.Register(
                nameof(MouseDoubleClickCommand),
                typeof(ICommand),
                typeof(BindableListView));

        public ICommand MouseDoubleClickCommand
        {
            get
            {
                return (ICommand)GetValue(MouseDoubleClickCommandProperty);
            }
            set
            {
                SetValue(MouseDoubleClickCommandProperty, value);
            }
        }

        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            DependencyObject obj = (DependencyObject)e.OriginalSource;
            while (obj != null && obj != this)
            {
                if (obj.GetType() == typeof(ListViewItem))
                {
                    if (SelectedItem != null)
                    {
                        MouseDoubleClickCommand?.Execute(SelectedItem);
                    }
                }
                obj = VisualTreeHelper.GetParent(obj);
            }
        }
    }
}
