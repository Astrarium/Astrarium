using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADK.Demo.Calculators
{
    public abstract class BaseSkyCalc
    {
        protected Sky Sky { get; private set; }

        public BaseSkyCalc(Sky sky)
        {
            Sky = sky;
        }

        public virtual void Initialize() { }

        public virtual void Calculate() { }
    }
}
