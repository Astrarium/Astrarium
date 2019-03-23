using ADK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.ViewModels
{
    public class MotionTrackVM : ViewModelBase
    {
        public MotionTrackVM(IViewManager viewManager)
        {
            ViewManager = viewManager;
        }

        public IViewManager ViewManager { get; private set; }
        public double JulianDayFrom { get; set; }
        public double JulianDayTo { get; set; }
        public double UtcOffset { get; set; }
    }
}
