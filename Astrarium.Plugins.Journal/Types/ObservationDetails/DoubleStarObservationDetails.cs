using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Journal.Types
{
    [CelestialObjectType("DeepSky.DoubleStar")]
    public class DoubleStarObservationDetails : DeepSkyObservationDetails
    {
        /// <summary>
        /// Color of main component, string with possible values:
        /// "white", "red", "orange", "yellow", "green", "blue"
        /// </summary>
        public string ColorMainComponent
        {
            get => GetValue<string>(nameof(ColorMainComponent));
            set => SetValue(nameof(ColorMainComponent), value);
        }

        /// <summary>
        /// Color of main component, string with possible values:
        /// "white", "red", "orange", "yellow", "green", "blue"
        /// </summary>
        public string ColorCompanionComponent
        {
            get => GetValue<string>(nameof(ColorCompanionComponent));
            set => SetValue(nameof(ColorCompanionComponent), value);
        }

        /// <summary>
        /// Flag indicating components have equal brightness
        /// </summary>
        public bool? EqualBrightness
        {
            get => GetValue<bool?>(nameof(EqualBrightness));
            set => SetValue(nameof(EqualBrightness), value);
        }

        /// <summary>
        /// Flag indicating nice sirrounding of components
        /// </summary>
        public bool? NiceSurrounding
        {
            get => GetValue<bool?>(nameof(NiceSurrounding));
            set => SetValue(nameof(NiceSurrounding), value);
        }
    }
}
