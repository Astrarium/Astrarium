using Astrarium.Algorithms;

namespace Astrarium.Plugins.Eclipses
{
    public interface IEclipsesCalculator
    {
        SolarEclipse GetNearestEclipse(double jd, bool next, bool saros);
        PolynomialBesselianElements GetBesselianElements(double jd);
        string GetLocalVisibilityString(SolarEclipse eclipse, SolarEclipseLocalCircumstances localCirc);
    }
}