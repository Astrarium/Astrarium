using System.Collections.Generic;

namespace Astrarium.Workers
{
    internal class WorkersCollection : IWorkersCollection
    {
        private List<IWorker> workers = new List<IWorker>();

        public void AddWorker(IWorker worker)
        {
            workers.Add(worker);
        }

        public void RunWorkers()
        {
            foreach (IWorker worker in workers)
            {
                worker.Run();
            }
        }
    }
}
