using Astrarium.Algorithms;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Eclipses
{
    /// <summary>
    /// Interface for eclipses calculator.
    /// </summary>
    public interface IEclipsesCalculator
    {
        /// <summary>
        /// Gets nearest solar eclipse by lunation number.
        /// </summary>
        /// <param name="ln">Meeus lunation number, use <see cref="LunarEphem.Lunation(double, LunationSystem)" /> to obtain it by Julian day</param>
        /// <param name="next">Flag indicating search direction: next (true) or previous (false).</param>
        /// <param name="saros">Flag indicating searching step: 1 saros (true) or 1 lunation (false).</param>
        /// <returns>Solar eclipse general info./returns>
        SolarEclipse GetNearestSolarEclipse(int ln, bool next, bool saros);

        /// <summary>
        /// Gets nearest lunar eclipse by lunation number.
        /// </summary>
        /// <param name="ln">Meeus lunation number, use <see cref="LunarEphem.Lunation(double, LunationSystem)" /> to obtain it by Julian day</param>
        /// <param name="next">Flag indicating search direction: next (true) or previous (false).</param>
        /// <param name="saros">Flag indicating searching step: 1 saros (true) or 1 lunation (false).</param>
        /// <returns>Lunar eclipse general info.</returns>
        LunarEclipse GetNearestLunarEclipse(int ln, bool next, bool saros);

        /// <summary>
        /// Calculates Besselian elements for a solar eclipse.
        /// </summary>
        /// <param name="jdMaximum">Julian Day of eclipse maximum.</param>
        /// <returns>Polynomial Besselian elements for the solar eclipse.</returns>
        PolynomialBesselianElements GetBesselianElements(double jd);

        /// <summary>
        /// Calculates Besselian elements for a lunar eclipse.
        /// </summary>
        /// <param name="jdMaximum">Julian Day of eclipse maximum.</param>
        /// <returns>Polynomial Besselian elements for the lunar eclipse.</returns>
        PolynomialLunarEclipseElements GetLunarEclipseElements(double jd);

        /// <summary>
        /// Gets localized string describing local visibility of solar eclipse, like "totally visible" or "visible as partial".
        /// </summary>
        /// <param name="eclipse">Solar eclipse general details.</param>
        /// <param name="localCirc">Local circumstances for given place.</param>
        /// <returns>Gets localized string describing local visibility of solar eclipse, like "totally visible" or "visible as partial".</returns>
        string GetLocalVisibilityString(SolarEclipse eclipse, SolarEclipseLocalCircumstances localCirc);

        /// <summary>
        /// Gets localized string describing local visibility of lunar eclipse, like "totally visible" or "visible begin of eclipse".
        /// </summary>
        /// <param name="eclipse">Lunar eclipse general details.</param>
        /// <param name="localCirc">Local circumstances for given place.</param>
        /// <returns>Gets localized string describing local visibility of lunar eclipse, like "totally visible" or "visible begin of eclipse".</returns>
        string GetLocalVisibilityString(LunarEclipse eclipse, LunarEclipseLocalCircumstances localCirc);

        /// <summary>
        /// Finds local circumstance of an eclipse for places located on the eclipse totality path.
        /// </summary>
        /// <param name="be">Besselian elements of the eclipse.</param>
        /// <param name="centralLine">Points of central line of the eclipse.</param>
        /// <param name="cancelToken">Optional cancellation token.</param>
        /// <param name="progress">Interface for reporting calculation progress.</param>
        /// <returns>Collection of local circumstances for all places found on the central line.</returns>
        ICollection<SolarEclipseLocalCircumstances> FindCitiesOnCentralLine(PolynomialBesselianElements be, ICollection<CrdsGeographical> centralLine, CancellationToken? cancelToken = null, IProgress<double> progress = null);

        /// <summary>
        /// Gets local circumstances of a solar eclipse for list of cities.
        /// </summary>
        /// <param name="be">Besselian elements for the eclipse.</param>
        /// <param name="cities">Collection of cities (geographical coordinates).</param>
        /// <param name="cancelToken">Optional cancellation token.</param>
        /// <param name="progress">Interface for reporting calculation progress.</param>
        /// <returns>Collection of local circumstances of a solar eclipse for list of cities.</returns>
        ICollection<SolarEclipseLocalCircumstances> FindLocalCircumstancesForCities(PolynomialBesselianElements be, ICollection<CrdsGeographical> cities, CancellationToken? cancelToken = null, IProgress<double> progress = null);

        /// <summary>
        /// Gets local circumstances of a lunar eclipse for list of cities.
        /// </summary>
        /// <param name="e">Lunar eclipse general info.</param>
        /// <param name="be">Besselian elements for the eclipse.</param>
        /// <param name="cities">Collection of cities (geographical coordinates).</param>
        /// <param name="cancelToken">Optional cancellation token.</param>
        /// <param name="progress">Interface for reporting calculation progress.</param>
        /// <returns>Collection of local circumstances of a lunar eclipse for list of cities.</returns>
        ICollection<LunarEclipseLocalCircumstances> FindLocalCircumstancesForCities(LunarEclipse e, PolynomialLunarEclipseElements be, ICollection<CrdsGeographical> cities, CancellationToken? cancelToken = null, IProgress<double> progress = null);
    }
}