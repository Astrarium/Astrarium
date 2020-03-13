using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Astrarium.Types.Themes
{
    public class WindowProperties : DependencyObject
    {
        public static readonly DependencyProperty MinButtonVisibleProperty = DependencyProperty.RegisterAttached(
            "MinButtonVisible", typeof(Visibility), typeof(WindowProperties), new PropertyMetadata(Visibility.Visible));

        public static readonly DependencyProperty MaxButtonVisibleProperty = DependencyProperty.RegisterAttached(
            "MaxButtonVisible", typeof(Visibility), typeof(WindowProperties), new PropertyMetadata(Visibility.Visible));

        public static readonly DependencyProperty CloseButtonVisibleProperty = DependencyProperty.RegisterAttached(
            "CloseButtonVisible", typeof(Visibility), typeof(WindowProperties), new PropertyMetadata(Visibility.Visible));

        public static readonly DependencyProperty IsFullScreenProperty = DependencyProperty.RegisterAttached(
            "IsFullScreen", typeof(bool), typeof(WindowProperties), new PropertyMetadata(false));

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

        public static void SetCloseButtonVisible(DependencyObject target, Visibility value)
        {
            target.SetValue(CloseButtonVisibleProperty, value);
        }

        public static Visibility GetCloseButtonVisible(DependencyObject target)
        {
            return (Visibility)target.GetValue(CloseButtonVisibleProperty);
        }

        public static void SetIsFullScreen(DependencyObject target, bool value)
        {
            target.SetValue(IsFullScreenProperty, value);
        }

        public static bool GetIsFullScreen(DependencyObject target)
        {
            return (bool)target.GetValue(IsFullScreenProperty);
        }
    }
}
