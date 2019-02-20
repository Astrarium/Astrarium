namespace ADK.Demo.Calculators
{
    public interface ILunarCalc
    {
        CrdsEquatorial Equatorial(SkyContext c);
        double Magnitude(SkyContext c);
        double Phase(SkyContext c);
        double Semidiameter(SkyContext c);
    }
}