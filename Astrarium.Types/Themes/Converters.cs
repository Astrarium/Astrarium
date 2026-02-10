using Astrarium.Types.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Astrarium.Types.Themes
{
    public class LocalizedTextConverter : ValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return value;
            }
            else if (value is string)
            {
                string text = (string)value;
                return text.StartsWith("$") ? Text.Get(text.Substring(1)) : text;
            }
            else
            {
                throw new ArgumentException("value is not string");
            }
        }
    }

    public static class FirstCapitalLetterExtension
    {
        public static string ToUpperFirstLetter(this string text)
        {
            if (text != null)
                return text.Length > 0 ? char.ToUpper(text[0]) + text.Substring(1) : text;
            else
                return null;
        } 
    }

    public class LocaleNameConverter : ValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string language)
                return language.ToUpperFirstLetter();
            else
                return null;
        }
    }

    public class MarkdownToPlainTextConverter : ValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Markdown.ToPlainText(value as string);
        }
    }

    public class SingleLineTextConverter : ValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value as string)?.Replace("\r\n", " ");
        }
    }

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

    public class MultiBoolToVisibilityConverter : MultiValueConverterBase
    {
        public override object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Visibility falseState = parameter is Visibility ? (Visibility)parameter : Visibility.Collapsed;
            return values.OfType<IConvertible>().All(System.Convert.ToBoolean) ? Visibility.Visible : falseState;
        }

        public override object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class BoolToVisibilityConverter : ValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool)
            {
                Visibility falseState = parameter is Visibility ? (Visibility)parameter : Visibility.Collapsed;
                return (bool)value ? Visibility.Visible : falseState;
            }
            else
            {
                throw new ArgumentException("value is not boolean");
            }
        }
    }

    public class BooleanAndConverter : MultiValueConverterBase
    {
        public override object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return values.OfType<IConvertible>().All(System.Convert.ToBoolean);
        }

        public override object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class BooleanOrConverter : MultiValueConverterBase
    {
        public override object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return values.OfType<IConvertible>().Any(System.Convert.ToBoolean);
        }

        public override object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
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
                return string.Format(CultureInfo.InvariantCulture, $"{{0:0.{new string('0', (int)decimalPlaces)}}}", (decimal)value);
            }
            else
            {
                return "";
            }
        }

        public override object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            string strValue = (string)value;
            if (string.IsNullOrEmpty(strValue) || strValue == "-")
                return new object[] { 0 };
            else
                return new object[] { decimal.Parse(strValue, NumberStyles.Float, CultureInfo.InvariantCulture) };
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

    [ValueConversion(typeof(string), typeof(Visibility))]
    public class NullOrEmptyToVisibilityConverter : ValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.IsNullOrEmpty(value as string) ? Visibility.Visible : Visibility.Collapsed;
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

    [ValueConversion(typeof(ICollection), typeof(Visibility))]
    public class EmptyCollectionToVisibilityConverter : ValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null || (value is ICollection collection && collection.Count == 0) ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    [ValueConversion(typeof(ICollection), typeof(Visibility))]
    public class NotEmptyCollectionToVisibilityConverter : ValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is ICollection collection && collection.Count != 0 ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    [ValueConversion(typeof(CelestialObject), typeof(string))]
    public class CelestialObjectNameConverter : ValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? Text.Get("NoSelectedObject") : (value as CelestialObject).Names.First();
        }
    }

    [ValueConversion(typeof(Astrarium.Algorithms.CrdsGeographical), typeof(string))]
    public class GeoLocationNameConverter : ValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value as Astrarium.Algorithms.CrdsGeographical).Name;
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

    [ValueConversion(typeof(object), typeof(bool))]
    public class EqualityConverter : ValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return object.Equals(value, parameter);
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return parameter;
        }
    }

    [ValueConversion(typeof(object), typeof(object))]
    public class VisibilityConverter : ValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                var castedParameter = System.Convert.ChangeType(parameter, value.GetType());

                if (object.Equals(value, castedParameter))
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }
            else
            {
                return Visibility.Collapsed;
            }
        }
    }

    [ValueConversion(typeof(object), typeof(object))]
    public class InverseVisibilityConverter : ValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var castedParameter = System.Convert.ChangeType(parameter, value.GetType());

            if (object.Equals(value, castedParameter))
                return Visibility.Collapsed;
            else
                return Visibility.Visible;
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

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }
    }

    public class FontStyleConverter : ValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var fontStyle = (System.Drawing.FontStyle)value;
            if (fontStyle.HasFlag(System.Drawing.FontStyle.Italic))
                return FontStyles.Italic;
            else
                return FontStyles.Normal;
        }
    }

    public class FontWeightConverter : ValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var fontStyle = (System.Drawing.FontStyle)value;
            if (fontStyle.HasFlag(System.Drawing.FontStyle.Bold))
                return FontWeights.Bold;
            else
                return FontWeights.Normal;
        }
    }

    public class FontDecorationsConverter : ValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var font = (Font)value;
            var decorations = new TextDecorationCollection();
            if (font.Underline)
                decorations.Add(TextDecorations.Underline);
            if (font.Strikeout)
                decorations.Add(TextDecorations.Strikethrough);
            return decorations;
        }
    }

    public class ColorToStringConverter : ValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var color = (Color)value;
            return string.Format("#{3}{0:X2}{3}{1:X2}{3}{2:X2}", color.R, color.G, color.B, "\u200a");
        }
    }

    public class ColorToMediaColorConverter : ValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var color = (Color)value;
            return System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
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
                    ?.Description ?? memberInfo?.Name);
        }
    }

    public class CelestialObjectToIconConverter : ValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            CelestialObject body = value as CelestialObject;
            if (body != null && body.Type != null)
            {
                string key = $"Icon{body.Type.Split('.').First()}";
                if (key != null && Application.Current.Resources.Contains(key))
                {
                    return Application.Current.Resources[key];
                }
            }
            return null;
        }
    }

    public class CelestialObjectTypeToIconConverter : ValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string key = null;
            if (value is string bodyType)
            {
                key = $"Icon{bodyType.Split('.').First()}";
            }
            else if (value is Type type)
            {
                key = $"Icon{type.Name}";
            }

            if (key != null && Application.Current.Resources.Contains(key))
            {
                return Application.Current.Resources[key];
            }
            else
            {
                return null;
            }
        }
    }

    public class StringCollectionToStringConverter : ValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ICollection<string> collection = value as ICollection<string>;
            if (collection != null)
            {
                return string.Join(", ", collection);
            }
            return null;
        }
    }

    public class CelestialObjectFullNameConverter : ValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            CelestialObject body = value as CelestialObject;
            if (body != null)
            {
                return string.Join(", ", body.Names);
            }
            return null;
        }
    }

    public class CelestialObjectTypeDescriptionConverter : ValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string strType)
            {
                return Text.Get($"{strType}.Type");
            }
            else if (value is CelestialObject body)
            {
                return Text.Get($"{body.Type}.Type");
            }
            else if (value is Type type)
            {
                return Text.Get($"{type.Name}.Type");
            }
            else
            {
                throw new NotImplementedException("Value type is not supported");
            }
        }
    }

    public class TimeSpanToStringConverter : ValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((TimeSpan)value).ToString(@"hh\:mm");
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DateToStringConverter : ValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Formatters.Date.Format(value);
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class JulianDayToStringConverter : MultiValueConverterBase
    {
        public override object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 2 && values[0] is double jd && values[1] is double utcOffset)
            {
                return Formatters.DateTime.Format(new Algorithms.Date(jd, utcOffset));
            }
            else
            {
                return "?";
            }
        }
    }

    public class DateTimeToStringConverter : ValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Formatters.DateTime.Format(value);
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class LongitudeConverter : ValueConverterBase
    {
        private static IEphemFormatter formatter = Formatters.Longitude;

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double longitude = (double)value;
            return $"{formatter.Format(Math.Abs(longitude))} {Text.Get(longitude <= 0 ? "LocationWindow.East" : "LocationWindow.West")}";
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class FormatterConverter : ValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Type type = parameter as Type;
            if (typeof(IEphemFormatter).IsAssignableFrom(type))
            {
                var formatter = Activator.CreateInstance(type) as IEphemFormatter;
                return formatter.Format(value);
            }
            else
            {
                throw new ArgumentException($"Parameter must implement {nameof(IEphemFormatter)} interface.");
            }
        }
    }

    public class LogScaleConverter : ValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double x = (double)value;
            return Math.Log(x);
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double x = (double)value;
            return Math.Exp(x);
        }
    }

    public class LatitudeConverter : ValueConverterBase
    {
        private static IEphemFormatter formatter = Formatters.Latitude;

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double latitude = (double)value;
            return $"{formatter.Format(Math.Abs(latitude))} {Text.Get(latitude < 0 ? "LocationWindow.South" : "LocationWindow.North")}";
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class TimeZoneConverter : ValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            if (value is TimeSpan)
            {
                TimeSpan offset = (TimeSpan)value;
                return $"UTC{(offset.Ticks < 0 ? "−" : "+")}{offset:hh\\:mm}";
            }
            else if (value is double)
            {
                TimeSpan offset = TimeSpan.FromHours((double)value);
                return $"UTC{(offset.Ticks < 0 ? "−" : "+")}{offset:hh\\:mm}";
            }
            else
                return null;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
