namespace Astrarium.Plugins.Planner.ImportExport
{
    public interface IPlanManagerFactory
    {
        string FormatsString { get; }
        IPlanManager Create(PlanType type);
        PlanType GetFormat(int index);
    }
}