using Astrarium.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.FOV
{
    /// <summary>
    /// Deserializes <see cref="FovFrame"/> from JSON
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

                if (jObject.ContainsKey(nameof(TelescopeFovFrame.EyepieceId)))
                {
                    return jObject.ToObject<TelescopeFovFrame>();
                }
                else if (jObject.ContainsKey(nameof(CameraFovFrame.CameraId)))
                {
                    return jObject.ToObject<CameraFovFrame>();
                }
                else if (jObject.ContainsKey(nameof(BinocularFovFrame.BinocularId)))
                {
                    return jObject.ToObject<BinocularFovFrame>();
                }
                else if (jObject.ContainsKey(nameof(FinderFovFrame.Crosslines)))
                {
                    return jObject.ToObject<FinderFovFrame>();
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

    /// <summary>
    /// Deserializes <see cref="SkyColor"/> from JSON
    /// </summary>
    public class FovFrameColorJsonConverter : JsonConverter
    {
        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {

        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            try
            {
                if (reader.TokenType == JsonToken.String)
                {
                    return (Color)JValue.Load(reader).ToObject(typeof(Color));
                }
                else
                {
                    return JObject.Load(reader).ToObject<Color>();
                }
            }
            catch
            {
                return Color.Plum;
            }
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(Color) == objectType;
        }
    }
}
