using Astrarium.Objects;
using Astrarium.Types.Localization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Astrarium.Types.Themes
{
    [ValueConversion(typeof(decimal), typeof(int))]
    [ValueConversion(typeof(int), typeof(decimal))]
    public class NumericConverter : ValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int && targetType == typeof(decimal))
            {
                return (decimal)((int)value);
            }
            else if (value is decimal && targetType == typeof(int))
            {
                return (int)Math.Min(int.MaxValue, (decimal)value);
            }
            else
                throw new NotImplementedException();
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert(value, targetType, parameter, culture);
        }
    }

    public class BoolToVisibilityConverter : ValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool)
            {
                return (bool)value ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                throw new ArgumentException("value is not boolean");
            }
        }
    }

    public class NumericUpDownTextConverter : MultiValueConverterBase
    {
        public override object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is decimal && targetType == typeof(string))
            {
                decimal value = (decimal)values[0];
                uint decimalPlaces = (uint)values[1];

                if (value > 0)
                    return string.Format(CultureInfo.InvariantCulture, $"{{0:0.{new string('0', (int)decimalPlaces)}}}", (decimal)value);
                else
                    return ((int)value).ToString();
            }
            else
            {
                return "";
            }
        }

        public override object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            if (string.IsNullOrEmpty((string)value))
                return new object[] { 0 };
            else
                return new object[] { decimal.Parse(value as string, NumberStyles.Float, CultureInfo.InvariantCulture) };
        }
    }

    [ValueConversion(typeof(object), typeof(bool))]
    public class NotNullToBoolConverter : ValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class NotNullToVisibilityConverter : ValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null ? Visibility.Visible : Visibility.Collapsed;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class NullToVisibilityConverter : ValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? Visibility.Visible : Visibility.Collapsed;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    public class LeftMarginMultiplierConverter : ValueConverterBase
    {
        public double Length { get; set; }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var item = value as TreeViewItem;
            if (item == null)
                return new Thickness(0);

            return new Thickness(Length * item.GetDepth(), 0, 0, 0);
        }
    }

    [ValueConversion(typeof(object), typeof(object))]
    public class VisibilityConverter : ValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (object.Equals(value, parameter))
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }
    }

    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class InverseBoolToVisibilityConverter : ValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((bool)value)
                return Visibility.Collapsed;
            else
                return Visibility.Visible;
        }
    }

    [ValueConversion(typeof(bool), typeof(ResizeMode))]
    public class InverseBoolToResizeModeConverter : ValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((bool)value)
                return ResizeMode.NoResize;
            else
                return ResizeMode.CanResize;
        }
    }

    [ValueConversion(typeof(bool), typeof(bool))]
    public class InverseBoolConverter : ValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return !(bool)value;
        }
    }

    public class ColorConverter : ValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert(value);
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert(value);
        }

        private object Convert(object value)
        {
            if (value is System.Windows.Media.Color)
            {
                var mediacolor = (System.Windows.Media.Color)value;
                return System.Drawing.Color.FromArgb(mediacolor.A, mediacolor.R, mediacolor.G, mediacolor.B);
            }
            else if (value is System.Drawing.Color)
            {
                var color = (System.Drawing.Color)value;
                return new System.Windows.Media.Color() { A = color.A, R = color.R, G = color.G, B = color.B };
            }
            else
                throw new ArgumentException("Incorrect data type.");
        }
    }

    public class FontToStringConverter : ValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var font = (System.Drawing.Font)value;
            return font.Name;
        }
    }

    public class ColorToStringConverter : ValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var color = (System.Drawing.Color)value;
            return string.Format("#{3}{0:X2}{3}{1:X2}{3}{2:X2}", color.R, color.G, color.B, "\u200a");
        }
    }

    public class ColorToString : ValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var color = (System.Drawing.Color)value;
            return string.Format("#{3}{0:X2}{3}{1:X2}{3}{2:X2}", color.R, color.G, color.B, "\u200a");
        }
    }

    public class ImageKeyToImageConverter : ValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null ? Application.Current.Resources[value] : null;
        }
    }

    public class EnumValueToEnumCollectionConverter : ValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Enum.GetValues(value.GetType());
        }
    }

    public class EnumValueToEnumDescriptionConverter : ValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var memberInfo = value?.GetType()
               ?.GetMember(value.ToString())
               ?.FirstOrDefault();

            return
                Text.Get(
                    memberInfo?.GetCustomAttribute<DescriptionAttribute>()
                    ?.Description ?? memberInfo.Name);
        }
    }

    public class CelestialObjectToIconConverter : ValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            CelestialObject body = value as CelestialObject;
            if (body != null)
            {
                string key = new[] { body.GetType() }
                     .Concat(body.GetType().GetInterfaces())
                     .Select(inf => $"Icon{inf.Name}")
                     .FirstOrDefault(k => Application.Current.Resources.Contains(k));

                if (key != null)
                {
                    return Application.Current.Resources[key];
                }                
            }
            return null;
        }
    }
}
