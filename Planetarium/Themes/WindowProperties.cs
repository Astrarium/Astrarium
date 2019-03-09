using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Planetarium.Themes
{
    public class WindowProperties : DependencyObject
    {
        public static readonly DependencyProperty MinButtonVisibleProperty = DependencyProperty.RegisterAttached(
            "MinButtonVisible", typeof(Visibility), typeof(WindowProperties), new PropertyMetadata(Visibility.Visible));

        public static void SetMinButtonVisible(DependencyObject target, Visibility value)
        {
            target.SetValue(MinButtonVisibleProperty, value);
        }

        public static Visibility GetMinButtonVisible(DependencyObject target)
        {
            return (Visibility)target.GetValue(MinButtonVisibleProperty);
        }
    }

    public class ListViewItemProperties : DependencyObject
    {
        public static readonly DependencyProperty ListViewItemClickProperty = DependencyProperty.RegisterAttached(
            "ListViewItemClick", typeof(ICommand), typeof(ListViewItemProperties), new PropertyMetadata(null));

        public static void SetListViewItemClick(DependencyObject target, ICommand value)
        {
            target.SetValue(ListViewItemClickProperty, value);
        }

        public static ICommand GetListViewItemClick(DependencyObject target)
        {
            return (ICommand)target.GetValue(ListViewItemClickProperty);
        }
    }
}
