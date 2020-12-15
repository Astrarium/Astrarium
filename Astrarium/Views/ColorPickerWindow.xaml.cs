using Astrarium.ViewModels;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Astrarium.Views
{
    /// <summary>
    /// Interaction logic for ColorPickerWindow.xaml
    /// </summary>
    public partial class ColorPickerWindow : Window
    {
        public static readonly DependencyProperty SliderOnlyProperty = DependencyProperty.Register(
            "SliderOnly", typeof(bool), typeof(ColorPickerWindow), new PropertyMetadata(SliderOnlyPropertyChanged));

        public static readonly DependencyProperty SelectedColorProperty = DependencyProperty.Register(
            "SelectedColor", typeof(Color), typeof(ColorPickerWindow), new PropertyMetadata(SelectedColorPropertyChanged));

        public static readonly DependencyProperty SliderBorderColorProperty = DependencyProperty.Register(
            "SliderBorderColor", typeof(System.Windows.Media.Color), typeof(ColorPickerWindow), new PropertyMetadata(SliderBorderColorPropertyChanged));

        public static readonly DependencyProperty SliderThumbsColorProperty = DependencyProperty.Register(
            "SliderThumbsColor", typeof(System.Windows.Media.Color), typeof(ColorPickerWindow), new PropertyMetadata(SliderThumbsColorPropertyChanged));

        public static readonly DependencyProperty SliderThumbsHighlightColorProperty = DependencyProperty.Register(
            "SliderThumbsHighlightColor", typeof(System.Windows.Media.Color), typeof(ColorPickerWindow), new PropertyMetadata(SliderThumbsHighlightColorPropertyChanged));

        public static void SetSliderOnly(DependencyObject target, bool value)
        {
            target.SetValue(SliderOnlyProperty, value);
        }

        public static bool GetSliderOnly(DependencyObject target)
        {
            return (bool)target.GetValue(SliderOnlyProperty);
        }

        public static void SetSelectedColor(DependencyObject target, Color value)
        {
            target.SetValue(SelectedColorProperty, value);
        }

        public static Color GetSelectedColor(DependencyObject target)
        {
            return (Color)target.GetValue(SelectedColorProperty);
        }

        public static void SetSliderBorderColor(DependencyObject target, System.Windows.Media.SolidColorBrush value)
        {
            target.SetValue(SliderBorderColorProperty, value);
        }

        public static System.Windows.Media.Color GetSliderBorderColor(DependencyObject target)
        {
            return (System.Windows.Media.Color)target.GetValue(SliderBorderColorProperty);
        }

        public static void SetSliderThumbsColor(DependencyObject target, System.Windows.Media.Color value)
        {
            target.SetValue(SliderThumbsColorProperty, value);
        }

        public static System.Windows.Media.Color GetSliderThumbsColor(DependencyObject target)
        {
            return (System.Windows.Media.Color)target.GetValue(SliderThumbsColorProperty);
        }

        public static void SetSliderThumbsHighlightColor(DependencyObject target, System.Windows.Media.Color value)
        {
            target.SetValue(SliderThumbsHighlightColorProperty, value);
        }

        public static System.Windows.Media.Color GetSliderThumbsHighlightColor(DependencyObject target)
        {
            return (System.Windows.Media.Color)target.GetValue(SliderThumbsHighlightColorProperty);
        }

        private static void SliderOnlyPropertyChanged(DependencyObject target, DependencyPropertyChangedEventArgs args)
        {
            (target as ColorPickerWindow).picker.SliderOnly = (bool)args.NewValue;
        }

        private static void SelectedColorPropertyChanged(DependencyObject target, DependencyPropertyChangedEventArgs args)
        {
            var picker = (target as ColorPickerWindow).picker;
            var color = (Color)args.NewValue;
            if (!picker.SelectedColor.Equals(color))
            {
                picker.SelectedColor = color;
            }
        }

        private static void SliderBorderColorPropertyChanged(DependencyObject target, DependencyPropertyChangedEventArgs args)
        {
            (target as ColorPickerWindow).sliderBorderColor = ToColor(args.NewValue);
        }

        private static void SliderThumbsColorPropertyChanged(DependencyObject target, DependencyPropertyChangedEventArgs args)
        {
            (target as ColorPickerWindow).sliderThumbsColor = ToColor(args.NewValue);
        }

        private static void SliderThumbsHighlightColorPropertyChanged(DependencyObject target, DependencyPropertyChangedEventArgs args)
        {
            (target as ColorPickerWindow).sliderThumbsHighlightColor = ToColor(args.NewValue);
        }

        private static Color ToColor(object value)
        {
            System.Windows.Media.Color color = (System.Windows.Media.Color)value;
            return Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        private Color sliderBorderColor;
        private Color sliderThumbsColor;
        private Color sliderThumbsHighlightColor;

        public ColorPickerWindow()
        {
            InitializeComponent();

            picker.SliderBorderColor = sliderBorderColor;
            picker.SliderThumbsColor = sliderThumbsColor;
            picker.SliderThumbsHighlightColor = sliderThumbsHighlightColor;
            picker.SelectedColorChanged += () => SetValue(SelectedColorProperty, picker.SelectedColor);
        }
    }
}
