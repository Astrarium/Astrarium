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
    }
}
