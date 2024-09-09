using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Astrarium.Types.Themes
{
    public class WindowProperties : DependencyObject
    {
        public static readonly DependencyProperty WindowBorderBrushProperty = DependencyProperty.RegisterAttached(
            "WindowBorderBrush", typeof(Brush), typeof(WindowProperties), new PropertyMetadata(Brushes.Transparent));

        public static readonly DependencyProperty MinButtonVisibleProperty = DependencyProperty.RegisterAttached(
            "MinButtonVisible", typeof(Visibility), typeof(WindowProperties), new PropertyMetadata(Visibility.Visible));

        public static readonly DependencyProperty MaxButtonVisibleProperty = DependencyProperty.RegisterAttached(
            "MaxButtonVisible", typeof(Visibility), typeof(WindowProperties), new PropertyMetadata(Visibility.Visible));

        public static readonly DependencyProperty CloseButtonVisibleProperty = DependencyProperty.RegisterAttached(
            "CloseButtonVisible", typeof(Visibility), typeof(WindowProperties), new PropertyMetadata(Visibility.Visible));

        public static readonly DependencyProperty IsFullScreenProperty = DependencyProperty.RegisterAttached(
            "IsFullScreen", typeof(bool), typeof(WindowProperties), new PropertyMetadata(false));

        public static readonly DependencyProperty CompactMenuProperty = DependencyProperty.RegisterAttached(
            "CompactMenu", typeof(object), typeof(WindowProperties), new PropertyMetadata(null));

        public static readonly DependencyProperty CompactMenuVisibleProperty = DependencyProperty.RegisterAttached(
            "CompactMenuVisible", typeof(Visibility), typeof(WindowProperties), new PropertyMetadata(Visibility.Collapsed));

        public static void SetWindowBorderBrush(DependencyObject target, Brush value)
        {
            target.SetValue(WindowBorderBrushProperty, value);
        }

        public static Brush GetWindowBorderBrush(DependencyObject target)
        {
            return (Brush)target.GetValue(WindowBorderBrushProperty);
        }

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

        public static void SetCompactMenu(DependencyObject target, object value)
        {
            target.SetValue(CompactMenuProperty, value);
        }

        public static object GetCompactMenu(DependencyObject target)
        {
            return target.GetValue(CompactMenuProperty);
        }

        public static void SetCompactMenuVisible(DependencyObject target, Visibility value)
        {
            target.SetValue(CompactMenuVisibleProperty, value);
        }

        public static Visibility GetCompactMenuVisible(DependencyObject target)
        {
            return (Visibility)target.GetValue(CompactMenuVisibleProperty);
        }
    }
}
