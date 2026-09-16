using Astrarium.Algorithms;
using Newtonsoft.Json;
using System;

namespace Astrarium.Plugins.RestAPI
{
    internal class DateJsonConverter : JsonConverter<Date>
    {
        public override bool CanRead => false;

        public override Date ReadJson(JsonReader reader, Type objectType, Date existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, Date value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToJulianEphemerisDay());
        }
    }
}
