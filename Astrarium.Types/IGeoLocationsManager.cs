﻿using Astrarium.Algorithms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Types
{
    /// <summary>
    /// Provides methods for searching geographical locations
    /// </summary>
    public interface IGeoLocationsManager
    {
        /// <summary>
        /// Searches geographical locations in a circle with specified center and radius in km.
        /// </summary>
        /// <param name="center">Center of the circle to search locations within.</param>
        /// <param name="radius">Radius, in kilometers, of the circle.</param>
        /// <returns>Collection of geographical locations.</returns>
        ICollection<CrdsGeographical> Search(CrdsGeographical center, float radius);

        /// <summary>
        /// Searches geographical locations by name.
        /// </summary>
        /// <param name="searchString">Prefix of location name.</param>
        /// <param name="maxCount">Maximal count of items to be returned.</param>
        /// <returns>Collection of geographical locations.</returns>
        ICollection<CrdsGeographical> Search(string searchString, int maxCount);
    }
}
