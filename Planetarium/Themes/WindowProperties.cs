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

        public static readonly DependencyProperty MaxButtonVisibleProperty = DependencyProperty.RegisterAttached(
            "MaxButtonVisible", typeof(Visibility), typeof(WindowProperties), new PropertyMetadata(Visibility.Visible));


        public static void SetMinButtonVisible(DependencyObject target, Visibility value)
        {
            target.SetValue(MinButtonVisibleProperty, value);
        }

        public static Visibility GetMinButtonVisible(DependencyObject target)
        {
            return (Visibility)target.GetValue(MinButtonVisibleProperty);
        }

        public static void SetMaxButtonVisible(DependencyObject target, Visibility value)
        {
            target.SetValue(MaxButtonVisibleProperty, value);
        }

        public static Visibility GetMaxButtonVisible(DependencyObject target)
        {
            return (Visibility)target.GetValue(MaxButtonVisibleProperty);
        }
    }
}
