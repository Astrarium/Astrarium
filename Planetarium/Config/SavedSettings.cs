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
   
    public class SavedSetting
    {
        public string Name { get; set; }
        public object Value { get; set; }
    }

    public class SettingValueConverter : JsonConverter
    {
        private IDictionary<string, Type> settingsTypes;

        public SettingValueConverter(IDictionary<string, Type> settingsTypes)
        {
            this.settingsTypes = settingsTypes;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(SavedSetting);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken jObject = JToken.ReadFrom(reader);

            string name = jObject["Name"].ToObject<string>();
            Type type = settingsTypes[name];

            SavedSetting result = new SavedSetting();

            result.Name = name;
            result.Value = jObject["Value"].ToObject(type);

            return result;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            SavedSetting savedSetting = (SavedSetting)value;

            JObject jObject = new JObject();

            jObject["Name"] = savedSetting.Name;
            jObject["Value"] = JToken.FromObject(savedSetting.Value, serializer);

            jObject.WriteTo(writer);
        }
    }
}
