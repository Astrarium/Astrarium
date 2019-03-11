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
    public class ObjectInfoWindowViewModel
    {
        public string Title { get; private set; }
        public string Subtitle { get; private set; }
        public IList<InfoElement> InfoElements { get; private set; }
        public ICommand LinkClickedCommand { get; private set; }
        public double JulianDay { get; private set; }

        public CloseWindowCommand CloseCommand { get; private set; } = new CloseWindowCommand();

        public ObjectInfoWindowViewModel(CelestialObjectInfo info)
        {
            Title = info.Title;
            Subtitle = info.Subtitle;
            InfoElements = info.InfoElements;
            LinkClickedCommand = new DelegateCommand<double>(jd => {
                JulianDay = jd;
                CloseCommand.Execute(true);
            });
        }
    }
}
