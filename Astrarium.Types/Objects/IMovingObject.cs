using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Objects
{
    /// <summary>
    /// Describes moving celestial objects with daily motion speed, in degrees
    /// </summary>
    public interface IMovingObject
    {
        /// <summary>
        /// Mean daily motion of the body, in degrees
        /// </summary>
        double AverageDailyMotion { get; }
    }
}
