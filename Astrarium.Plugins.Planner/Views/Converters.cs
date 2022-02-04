﻿using Astrarium.Algorithms;
using Astrarium.Types;
using Astrarium.Types.Themes;
using System;
using System.Globalization;

namespace Astrarium.Plugins.Planner.Views
{
    public class Converter : ValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Formatters.GetDefault((string)parameter).Format(value);
        }
    }

    public class TimeLinkVisibilityConverter : ValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return double.IsNaN((value as Date).Day) ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
        }
    }

    public class TimeLinkInverseVisibilityConverter : ValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return double.IsNaN((value as Date).Day) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
        }
    }

    public class CelestialObjectNameConverter : ValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Join(", ", (value as CelestialObject).Names);
        }
    }

    public class CelestialObjectTypeDescriptionConverter : ValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Text.Get($"{(value as CelestialObject).Type}.Type");
        }
    }
}
