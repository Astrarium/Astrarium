using ADK.Demo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Planetarium.ViewModels
{
    public class ObjectInfoVM : ViewModelBase
    {
        public string Title { get; private set; }
        public string Subtitle { get; private set; }
        public IList<InfoElement> InfoElements { get; private set; }
        public ICommand LinkClickedCommand { get; private set; }
        public double JulianDay { get; private set; }

        public ObjectInfoVM(CelestialObjectInfo info)
        {
            Title = info.Title;
            Subtitle = info.Subtitle;
            InfoElements = info.InfoElements;
            LinkClickedCommand = new Command<double>(SelectJulianDay);
        }

        private void SelectJulianDay(double jd)
        {
            JulianDay = jd;
            Close(true);
        }
    }
}
