using ADK;
using Planetarium.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.ViewModels
{
    public class MotionTrackVM : ViewModelBase
    {
        public MotionTrackVM(IViewManager viewManager, ISearcher searcher)
        {
            ViewManager = viewManager;
            Searcher = searcher;
        }

        public IViewManager ViewManager { get; private set; }
        public ISearcher Searcher { get; private set; }
        public CelestialObject SelectedBody { get; set; }
        public double JulianDayFrom { get; set; }
        public double JulianDayTo { get; set; }
        public double UtcOffset { get; set; }
    }
}
