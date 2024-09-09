using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Journal.Types
{
    [CelestialObjectType("DeepSky.OpenCluster")]
    public class OpenClusterObservationDetails : DeepSkyObservationDetails
    {
        // TODO: character descriptions

        /// <summary>
        /// Character of the cluster according to "Deep Sky Liste" definition
        /// </summary>
        public string Character
        {
            get => GetValue<string>(nameof(Character));
            set => SetValue(nameof(Character), value);
        }

        public bool? UnusualShape
        {
            get => GetValue<bool?>(nameof(UnusualShape));
            set => SetValue(nameof(UnusualShape), value);
        }

        public bool? PartlyUnresolved
        {
            get => GetValue<bool?>(nameof(PartlyUnresolved));
            set => SetValue(nameof(PartlyUnresolved), value);
        }

        public bool? ColorContrasts
        {
            get => GetValue<bool?>(nameof(ColorContrasts));
            set => SetValue(nameof(ColorContrasts), value);
        }
    }
}
