using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium
{
    public class CloseWindowCommand
    {
        public event Action<bool?> OnExecuted;
        public void Execute(bool? dialogResult)
        {
            OnExecuted?.Invoke(dialogResult);
        }

        public void Execute()
        {
            OnExecuted?.Invoke(null);
        }
    }
}
