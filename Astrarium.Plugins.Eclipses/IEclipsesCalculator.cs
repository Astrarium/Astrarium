using Astrarium.Algorithms;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Eclipses
{
    public interface IEclipsesCalculator
    {
        SolarEclipse GetNearestSolarEclipse(double jd, bool next, bool saros);
        LunarEclipse GetNearestLunarEclipse(double jd, bool next, bool saros);

        /// <summary>
        /// Calculates Besselian for a solar eclipse.
        /// </summary>
        /// <param name="jdMaximum">Julian Day of eclipse maximum.</param>
        /// <returns>Polynomial Besselian elements for the solar eclipse.</returns>
        PolynomialBesselianElements GetBesselianElements(double jd);

        /// <summary>
        /// Calculates Besselian for a lunar eclipse.
        /// </summary>
        /// <param name="jdMaximum">Julian Day of eclipse maximum.</param>
        /// <returns>Polynomial Besselian elements for the lunar eclipse.</returns>
        PolynomialLunarEclipseElements GetLunarEclipseElements(double jd);

        string GetLocalVisibilityString(SolarEclipse eclipse, SolarEclipseLocalCircumstances localCirc);

        /// <summary>
        /// Finds local circumstance of an eclipse for places located on the eclipse totality path
        /// </summary>
        /// <param name="be">Besselian elements of the eclipse.</param>
        /// <param name="centralLine">Points of central line of the eclipse.</param>
        /// <param name="cancelToken">Optional cancellation token.</param>
        /// <param name="progress">Interface for reporting calculation progress.</param>
        /// <returns>Collection of local circumstances for all places found on the central line.</returns>
        ICollection<SolarEclipseLocalCircumstances> FindCitiesOnCentralLine(PolynomialBesselianElements be, ICollection<CrdsGeographical> centralLine, CancellationToken? cancelToken = null, IProgress<double> progress = null);

        ICollection<SolarEclipseLocalCircumstances> FindLocalCircumstancesForCities(PolynomialBesselianElements be, ICollection<CrdsGeographical> cities, CancellationToken? cancelToken = null, IProgress<double> progress = null);
    }
}