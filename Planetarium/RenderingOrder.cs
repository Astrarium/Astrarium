using Newtonsoft.Json;
using Planetarium.Renderers;
using Planetarium.Types.Localization;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium
{    
    public class RenderingOrder : ObservableCollection<RenderingOrderItem>
    {
        public RenderingOrder() : base() { }
        public RenderingOrder(IEnumerable<RenderingOrderItem> items) : base(items) { }
    }

    [JsonConverter(typeof(RenderingOrderItemConverter))]
    public class RenderingOrderItem
    {
        /// <summary>
        /// Full name of the renderer type
        /// </summary>
        public string RendererTypeName { get; private set; }

        /// <summary>
        /// Displayable renderer name
        /// </summary>
        [JsonIgnore]
        public string Name
        {
            get
            {
                return Text.Get($"{RendererTypeName}.Name");
            }
        }

        public RenderingOrderItem(string name)
        {
            RendererTypeName = name;
        }

        public RenderingOrderItem(BaseRenderer renderer)
        {
            RendererTypeName = renderer.GetType().FullName;
        }
    }

    public class RenderingOrderItemConverter : JsonConverter<RenderingOrderItem>
    {
        public override RenderingOrderItem ReadJson(JsonReader reader, Type objectType, RenderingOrderItem existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return new RenderingOrderItem((string)reader.Value);
        }

        public override void WriteJson(JsonWriter writer, RenderingOrderItem value, JsonSerializer serializer)
        {
            writer.WriteValue(value.RendererTypeName);
        }
    }
}
