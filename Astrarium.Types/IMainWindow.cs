using Astrarium.Algorithms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Types
{
    /// <summary>
    /// Provides access to the main application window.
    /// </summary>
    public interface IMainWindow
    {
        /// <summary>
        /// Centers the sky map on the specified object.
        /// </summary>
        /// <param name="body"></param>
        /// <returns>True if object was found on the sky and can be centered, false otherwise.</returns>
        bool CenterOnObject(CelestialObject body);

        /// <summary>
        /// Centers sky map on the specified point
        /// </summary>
        /// <param name="hor">Equatorial coordinates of the target point</param>
        /// <param name="targetViewAngle">Target view angle to be set.</param>
        void CenterOnPoint(CrdsEquatorial eq, double targetViewAngle);

        void Focus();
    }
}
