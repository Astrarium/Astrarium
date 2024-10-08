﻿using Astrarium.Types;

namespace Astrarium.Plugins.UCAC4
{
    public class UCAC4Star : CelestialObject, IMagnitudeObject, IObservableObject
    {
        /// <inheritdoc />
        public override string Type => "Star";

        /// <inheritdoc />
        public override string CommonName => ToString();

        /// <summary>
        /// Magnitude
        /// </summary>
        public float Magnitude { get; set; }

        /// <summary>
        /// Proper name, if exists
        /// </summary>
        public string ProperName { get; set; }

        /// <summary>
        /// Gets star names
        /// </summary>
        public override string[] Names
        {
            get
            {
                if (ProperName != null)
                {
                    return new string[] { ProperName, ToString() };
                }
                else
                {
                    return new string[] { ToString() };
                }
            }
        }

        /// <summary>
        /// UCAC4 zone number for this star, 1...9000
        /// </summary>
        internal ushort ZoneNumber { get; set; }

        /// <summary>
        /// Running number of the star inside the zone
        /// </summary>
        internal uint RunningNumber { get; set; }

        /// <summary>
        /// Approximate spectral class, calculated from B-V index, used only for displaying stars colors
        /// </summary>
        internal char SpectralClass { get; set; }

        /// <summary>
        /// Name of the setting(s) responsible for displaying the object
        /// </summary>
        public override string[] DisplaySettingNames => new[] { "Stars", "UCAC4" };

        /// <summary>
        /// TODO: description
        /// </summary>
        internal UCAC4StarPosData PositionData { get; set; }

        public override string ToString()
        {
            return $"UCAC4 {ZoneNumber:000}-{RunningNumber:000000}";
        }

        public override bool Equals(object obj)
        {
            if (obj is UCAC4Star)
            {
                UCAC4Star star = obj as UCAC4Star;
                return star.ZoneNumber == ZoneNumber && star.RunningNumber == RunningNumber;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
    }
}
