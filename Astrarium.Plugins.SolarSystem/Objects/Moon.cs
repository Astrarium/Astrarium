using Astrarium.Algorithms;
using Astrarium.Types;

namespace Astrarium.Plugins.SolarSystem.Objects
{
    /// <summary>
    /// Contains coordinates and visual appearance data for the Moon for given instant of time.
    /// </summary>
    public class Moon : SizeableCelestialObject, ISolarSystemObject, IMagnitudeObject, IMovingObject, IObservableObject
    {
        /// <inheritdoc />
        public override string Type => "Moon";

        /// <inheritdoc />
        public override string CommonName => "Moon";

        /// <summary>
        /// Geocentrical ecliptical corrdinates
        /// </summary>
        public CrdsEcliptical Ecliptical0 { get; set; }

        /// <summary>
        /// Magnitude
        /// </summary>
        public float Magnitude { get; set; }

        /// <summary>
        /// Distance from Earth, in AU
        /// </summary>
        public double DistanceFromEarth => Ecliptical0.Distance / 149597870;

        /// <summary>
        /// Elongation angle, i.e. angular distance from the Sun. 
        /// Positive if eastern elongation, negative if western. 
        /// </summary>
        public double Elongation { get; set; }

        /// <summary>
        /// Phase of the Moon, from 0 (New Moon) to 1 (Full Moon).
        /// </summary>
        public double Phase { get; set; }

        /// <summary>
        /// Phase angle of the Moon, signed
        /// </summary>
        public double PhaseAngle { get; set; }

        /// <summary>
        /// Position angle of Moon axis, in degrees.
        /// </summary>
        public double PAaxis { get; set; }

        /// <summary>
        /// Libration elements for the Moon
        /// </summary>
        public Libration Libration { get; set; }

        /// <summary>
        /// Longitude of ascending node of lunar orbit
        /// </summary>
        public double AscendingNode { get; set; }

        /// <summary>
        /// Appearance details of Earth shadow
        /// </summary>
        public ShadowAppearance EarthShadow { get; set; }

        /// <summary>
        /// Topocentrical coordinates of Earth shadow
        /// </summary>
        public CrdsEquatorial EarthShadowCoordinates { get; set; }

        /// <summary>
        /// Mean daily motion of the Moon, in degrees
        /// </summary>
        public double AverageDailyMotion => LunarEphem.AVERAGE_DAILY_MOTION;

        /// <summary>
        /// Gets Moon names
        /// </summary>
        public override string[] Names => new[] { Name };

        /// <summary>
        /// Primary name
        /// </summary>
        public string Name => Text.Get("Moon.Name");

        /// <summary>
        /// Name of the setting(s) responsible for displaying the object
        /// </summary>
        public override string[] DisplaySettingNames => new[] { "Moon" };
    }
}
