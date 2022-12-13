using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Journal.OAL
{
    public interface IOALConverter
    {
        object Convert(object value, object context);
    } 

    public class SimpleConverter : IOALConverter
    {
        public object Convert(object value, object context)
        {
            return value;
        }
    }

    public class ToStringConverter : IOALConverter
    {
        public object Convert(object value, object context)
        {
            return value?.ToString();
        }
    }

    public class ToNullableIntConverter : IOALConverter
    {
        public object Convert(object value, object context)
        {
            if (value == null || (value is string s && s.Trim().Equals("")))
                return null;
            else 
                return System.Convert.ToInt32(value);
        }
    }
}
