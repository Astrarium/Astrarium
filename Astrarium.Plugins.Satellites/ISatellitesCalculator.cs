namespace Astrarium.Plugins.Satellites
{
    public interface ISatellitesCalculator
    {
        void LoadSatellites(string directory, TLESource tleSource);
        void Calculate();
    }
}