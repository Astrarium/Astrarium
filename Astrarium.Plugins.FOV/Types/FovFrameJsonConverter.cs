using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.FOV
{
    /// <summary>
    /// Serializes/deserializes <see cref="FovFrame"/> to/from JSON
    /// </summary>
    public class FovFrameJsonConverter : JsonConverter
    {
        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (objectType == typeof(FovFrame))
            {
                var jObject = JObject.Load(reader);

                if (jObject.ContainsKey("EyepieceId"))
                {
                    return jObject.ToObject<TelescopeFovFrame>();
                }
                else if (jObject.ContainsKey("CameraId"))
                {
                    return jObject.ToObject<CameraFovFrame>();
                }
                else if (jObject.ContainsKey("BinocularId"))
                {
                    return jObject.ToObject<BinocularFovFrame>();
                }
                else
                {
                    throw new FormatException("Unrecognized type");
                }
            }
            else
            {
                serializer.ContractResolver.ResolveContract(objectType).Converter = null;
                return serializer.Deserialize(reader, objectType);
            }
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(FovFrame) == objectType;
        }
    }
}
