using ADK.Demo.Calculators;
using ADK.Demo.Objects;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ADK.Demo
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            //IKernel kernel = new StandardKernel();
            //var interfaces = typeof(PlanetsCalc).GetInterfaces();
            //kernel.Bind(interfaces).To(typeof(PlanetsCalc)).InSingletonScope();         
            //var pp = kernel.Get<IPlanetsProvider>();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormMain());
        }
    }
}
