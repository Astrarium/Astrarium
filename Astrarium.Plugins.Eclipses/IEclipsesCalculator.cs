using Astrarium.Algorithms;

namespace Astrarium.Plugins.Eclipses
{
    public interface IEclipsesCalculator
    {
        PolynomialBesselianElements GetBesselianElements(double jd);
        string GetLocalVisibilityString(SolarEclipse eclipse, SolarEclipseLocalCircumstances localCirc);
    }
}