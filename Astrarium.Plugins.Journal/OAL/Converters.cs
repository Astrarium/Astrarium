using Astrarium.Plugins.Journal.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Astrarium.Plugins.Journal.OAL
{
    public interface IOALConverter
    {
        object Convert(object value);
    }

    public class SimpleConverter : IOALConverter
    {
        public object Convert(object value)
        {
            return value;
        }
    }

    public class ToStringConverter : IOALConverter
    {
        public object Convert(object value)
        {
            return value?.ToString();
        }
    }

    public class ImportNullableIntConverter : IOALConverter
    {
        public object Convert(object value)
        {
            if (value == null || (value is string s && s.Trim().Equals("")))
                return null;
            else 
                return System.Convert.ToInt32(value);
        }
    }

    public class ExportNullableBoolConverter : IOALConverter
    {
        public object Convert(object value)
        {
            if (value == null)
                return null;
            else
                return (bool)value;
        }
    }

    public abstract class ExportStringAsEnumConverter<T> : IOALConverter where T : Enum
    {
        private static string GetXmlAttrNameFromEnumValue(T enumValue)
        {
            Type type = enumValue.GetType();
            FieldInfo info = type.GetField(Enum.GetName(typeof(T), enumValue));
            XmlEnumAttribute att = (XmlEnumAttribute)info.GetCustomAttributes(typeof(XmlEnumAttribute), false)[0];
            return att.Name;
        }

        protected T GetValueFromString(string value)
        {
            return Enum.GetValues(typeof(T)).OfType<T>().FirstOrDefault(x => GetXmlAttrNameFromEnumValue(x).Equals(value, StringComparison.OrdinalIgnoreCase));
        }

        public object Convert(object value)
        {
            return GetValueFromString(value as string);
        }
    }


    public class ExportStarColorConverter : ExportStringAsEnumConverter<OALStarColor> { }

    public class ExportClusterCharacterConverter : ExportStringAsEnumConverter<OALClusterCharacter> { }

    public class ExportArcSecondsConverter : IOALConverter
    {
        public object Convert(object value)
        {
            if (value == null)
                return null;
            else
                return new OALNonNegativeAngle() { Value = System.Convert.ToDouble(value), Unit = OALAngleUnit.ArcSec };
        }
    }

    /// <summary>
    /// (OALNonNegativeAngle) => (double?) in arcseconds 
    /// </summary>
    public class ImportArcSecondsConverter : IOALConverter
    {
        public object Convert(object value)
        {
            if (value == null)
                return null;
            else
            {
                var angle = value as OALNonNegativeAngle;
                //double value = angle.Value;
                switch (angle.Unit)
                {
                    case OALAngleUnit.ArcMin:
                        return angle.Value * 60.0;
                    case OALAngleUnit.ArcSec:
                        return angle.Value;
                    case OALAngleUnit.Deg:
                        return angle.Value * 3600.0;
                    case OALAngleUnit.Rad:
                        return Algorithms.Angle.ToDegrees(angle.Value) * 3600.0;
                    default:
                        throw new Exception();
                }
            }
        }
    }

    public class ExportVariableStarVisMagConverter : IOALConverter
    {
        public object Convert(object value)
        {
            var details = value as VariableStarObservationDetails;
            return new OALVariableStarVisMag()
            {
                FainterThan = details.VisMagFainterThan ?? false,
                FainterThanSpecified = details.VisMagFainterThan != null,
                Uncertain = details.VisMagUncertain ?? false,
                UncertainSpecified = details.VisMagUncertain != null,
                Value = details.VisMag
            };
        }
    }

    public class ExportVariableStarChartIdconverter : IOALConverter
    {
        public object Convert(object value)
        {
            var details = value as VariableStarObservationDetails;
            return new OALVariableStarChartId()
            {
                NonAAVSOChart = details.NonAAVSOChart ?? false,
                NonAAVSOChartSpecified = details.NonAAVSOChart != null,
                Value = details.ChartDate
            };
        }
    }

    public class ExportNullableDoubleConverter : IOALConverter
    {
        public object Convert(object value)
        {
            if (value == null)
                return 0;
            else
                return System.Convert.ToDouble(value);
        }
    }

    public class ExportPosAngleConverter : IOALConverter
    {
        public object Convert(object value)
        {
            if (value == null)
                return null;
            else
                return value.ToString();
        }
    }

    public class ExportJsonConverter : IOALConverter
    {
        public object Convert(object value)
        {
            return JsonConvert.DeserializeObject<string[]>(value as string);
        }
    }

    public class ExportSurfBrightnessConverter : IOALConverter
    {
        public object Convert(object value)
        {
            if (value == null)
                return null;
            else
                return new OALSurfaceBrightness() { Value = System.Convert.ToDouble(value), Unit = OALSurfaceBrightnessUnit.MagsPerSquareArcSec };
        }
    }

    public class ImportSurfBrightnessConverter : IOALConverter
    {
        public object Convert(object value)
        {
            if (value == null)
                return null;
            else
            {
                OALSurfaceBrightness surfBr = value as OALSurfaceBrightness;
                switch (surfBr.Unit)
                {
                    case OALSurfaceBrightnessUnit.MagsPerSquareArcMin:
                        return surfBr.Value * 3600;
                    case OALSurfaceBrightnessUnit.MagsPerSquareArcSec:
                        return surfBr.Value;
                    default:
                        throw new Exception();
                }
            }
        }
    }

    public class OALTargetVariableStarDicriminator : ICelestialObjectTypeDiscriminator
    {
        public string Discriminate(object dataObject)
        {
            OALTargetVariableStar varStar = dataObject as OALTargetVariableStar;
            string[] novae = new string[] { "Nova", "Novae", "NA", "NB", "NC", "NR", "RN" };
            return 
                varStar.Name?.Contains("Nova") == true || 
                varStar.Alias?.Any(x => x.Contains("Nova")) == true || 
                (varStar.Type != null && novae.Any(x => varStar.Type.Equals(x, StringComparison.OrdinalIgnoreCase))) ? "Nova" : "VarStar";
        }
    }
}
