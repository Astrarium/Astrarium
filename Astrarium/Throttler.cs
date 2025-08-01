using System;
using System.Threading.Tasks;

namespace Astrarium
{
    public class Throttler
    {
        private readonly object locker = new object();
        private Action pendingAction;
        private bool isRunning;
        private Task currentTask;

        public void Throttle(Action action)
        {
            lock (locker)
            {
                pendingAction = action;
                if (!isRunning)
                {
                    isRunning = true;
                    currentTask = Task.Run(ProcessActions);
                }
            }
        }

        private void ProcessActions()
        {
            while (true)
            {
                Action actionToExecute;
                lock (locker)
                {
                    actionToExecute = pendingAction;
                    pendingAction = null;

                    if (actionToExecute == null)
                    {
                        isRunning = false;
                        return;
                    }
                }

                actionToExecute();
            }
        }
    }
}