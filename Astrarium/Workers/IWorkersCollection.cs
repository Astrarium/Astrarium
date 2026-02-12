namespace Astrarium.Workers
{
    public interface IWorkersCollection
    {
        void AddWorker(IWorker worker);
        void RunWorkers();
    }
}
