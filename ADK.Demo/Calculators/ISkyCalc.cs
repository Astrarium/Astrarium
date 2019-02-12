using ADK.Demo.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADK.Demo.Calculators
{
    public interface ISkyCalc
    {
        void Initialize();
        void Calculate(SkyContext context);
    }
}
