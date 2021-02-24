using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Algorithms
{
    /// <summary>
    /// Different systems of Moon lunations
    /// </summary>
    /// <remarks>
    /// See explanations here: https://en.wikipedia.org/wiki/New_moon#Lunation_Number
    /// </remarks>
    public enum LunationSystem
    {
        /// <summary>
        /// The most commonly used type of lunation, which defines lunation 1 as beginning at the first new moon of 1923, 
        /// the year when Ernest William Brown's lunar theory was introduced in the major national astronomical almanacs. 
        /// </summary>
        Brown = 0,

        /// <summary>
        /// Introduced by Jean Meeus, defines lunation 0 as beginning on the first new moon of 2000 (this occurred at approximately 18:14 UTC, January 6, 2000).
        /// </summary>
        Meeus = 1,

        /// <summary>
        /// Refers to the lunation numbering used by Herman Goldstine in his 1973 book New and Full Moons: 1001 B.C. to A.D. 1651, with lunation 0 beginning on January 11, 1001 BC
        /// </summary>
        Goldstine = 2,

        /// <summary>
        /// The count of lunations in the Hebrew calendar with lunation 1 beginning on October 7, 3761 BC.
        /// </summary>
        Hebrew = 3,

        /// <summary>
        /// The count of lunations in the Islamic calendar with lunation 1 as beginning on July 16, 622.
        /// </summary>
        Islamic = 4,

        /// <summary>
        /// Thai Lunation Number (Maasa-Kendha), defines lunation 0 as beginning of the SouthEast-Asian Calendar on Sunday March 22, 638.
        /// </summary>
        Thai = 5,
    }
}
