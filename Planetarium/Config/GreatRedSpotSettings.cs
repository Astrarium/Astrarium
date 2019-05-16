using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Planetarium.Config
{
    [TypeConverter(typeof(GenericJsonSettingConverter<GreatRedSpotSettings>))]
    [DataContract]
    public class GreatRedSpotSettings
    {
        [DataMember]
        public double Epoch { get; set; }

        [DataMember]
        public double MonthlyDrift { get; set; }

        [DataMember]
        public double Longitude { get; set; }
    }

    [TypeConverter(typeof(GenericJsonSettingConverter<LocationSettings>))]
    [DataContract]
    public class LocationSettings
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public double Longitude { get; set; }

        [DataMember]
        public double Latitude { get; set; }

        [DataMember]
        public double Elevation { get; set; }

        [DataMember]
        public double UtcOffset { get; set; }
    }

    public class GenericJsonSettingConverter<T> : TypeConverter
    {
        private static readonly DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
 
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            var casted = value as string;
            return casted != null
                ? FromJson(casted)
                : base.ConvertFrom(context, culture, value);
        }
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            var casted = (T)value;
            return destinationType == typeof(string) && casted != null
                ? ToJson(casted)
                : base.ConvertTo(context, culture, value, destinationType);
        }

        private string ToJson(T setting)
        {
            using (var ms = new MemoryStream())
            {
                ser.WriteObject(ms, setting);
                ms.Flush();
                ms.Position = 0;
                using (var sr = new StreamReader(ms))
                {
                    return sr.ReadToEnd();
                }
            }
        }

        private T FromJson(string json)
        {
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                return (T)ser.ReadObject(ms);
            }
        }
    }
}
