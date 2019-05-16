using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Planetarium.Config
{
    public class SavedSettings : List<SavedSetting>
    {

    }

    [JsonConverter(typeof(SettingValueConverter))]
    public class SavedSetting
    {
        public string Name { get; set; }
        public object Value { get; set; }
    }

    public class SettingValueConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(SettingValueConverter);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken jObject = JToken.ReadFrom(reader);
            
            string typeName = jObject["Type"].ToObject<string>();
            Type type = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .FirstOrDefault(t => GetShortTypeName(t) == typeName);

            SavedSetting result = new SavedSetting();

            result.Name = jObject["Name"].ToObject<string>();
            result.Value = jObject["Value"].ToObject(type);

            return result;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            SavedSetting savedSetting = (SavedSetting)value;

            JObject jObject = new JObject();

            Type type = savedSetting.Value.GetType();

            jObject["Name"] = savedSetting.Name;
            jObject["Type"] = JToken.FromObject(GetShortTypeName(type));
            jObject["Value"] = JToken.FromObject(savedSetting.Value);

            jObject.WriteTo(writer);
        }

        private string GetShortTypeName(Type type)
        {
            return $"{type.FullName}, {type.Assembly.GetName().Name}";
        }
    }
}
