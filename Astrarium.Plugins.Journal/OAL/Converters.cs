using Astrarium.Plugins.Journal.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                return ((double)value) / 3600.0;
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

    
}
