using Astrarium.Algorithms;
using System.Collections.Generic;

namespace Astrarium.Types
{
    public abstract class SizeableCelestialObject : CelestialObject
    {
        /// <summary>
        /// Visible semidiameter, in seconds of arc
        /// </summary>
        public virtual float Semidiameter { get; set; }

        /// <summary>
        /// Visible large semidiameter, in seconds of arc.
        /// </summary>
        public virtual float? LargeSemidiameter { get; set; }

        /// <summary>
        /// Visible small semidiameter, in seconds of arc.
        /// </summary>
        public virtual float? SmallSemidiameter { get; set; }

        /// <summary>
        /// Position angle of celestial object shape, measure from <see cref="PositionAngleOrigin"/>
        /// </summary>
        public virtual float? PositionAngle { get; set; }

        public virtual PosAngleOrigin? PositionAngleOrigin { get; }

        /// <summary>
        /// Gets Julian Date of epoch of coordinates describing complex object shape
        /// </summary>
        public virtual double? ShapeEpoch { get; }

        /// <summary>
        /// Gets or sets coordinates that define complex shape of the object
        /// </summary>
        public virtual IEnumerable<CrdsEquatorial> Shape { get; set; }
    }

    public enum PosAngleOrigin
    {
        EquatorialPole,
        HorizontalPole,
        EclipticPole
    }
}
