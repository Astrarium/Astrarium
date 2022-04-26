namespace Astrarium.Plugins.Planner.ImportExport
{
    public interface IPlanFactory
    {
        string FormatsString { get; }

        IPlan Create(PlanType type);
        PlanType GetFormat(int index);
        PlanType GetFormat(string extension);
    }
}