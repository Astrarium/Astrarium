using Astrarium.Config;
using Astrarium.Objects;
using Astrarium.Plugins.Tracks.ViewModels;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Tracks
{
    public class Plugin : AbstractPlugin, INotifyPropertyChanged
    {
        private ISky sky;
        private ISkyMap map;

        public Plugin(ISky sky, ISkyMap map)
        {
            this.sky = sky;
            this.map = map;

            this.map.SelectedObjectChanged += (o) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsMotionTrackEnabled)));

            var menuItem = new MenuItem("Motion track") 
            { 
               Command = new Command(ShowMotionTrackWindow)
            };
            menuItem.AddBinding(new SimpleBinding(this, nameof(IsMotionTrackEnabled), "IsEnabled"));
            AddContextMenuItem(menuItem);
        }

        public bool IsMotionTrackEnabled
        {
            get
            {
                var body = map.SelectedObject;
                return body != null && body is IMovingObject;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void ShowMotionTrackWindow()
        {
            var body = map.SelectedObject;

            if (IsMotionTrackEnabled)
            {
                var vm = ViewManager.CreateViewModel<MotionTrackVM>();
                vm.TrackId = Guid.NewGuid();
                vm.SelectedBody = body;
                vm.JulianDayFrom = sky.Context.JulianDay;
                vm.JulianDayTo = sky.Context.JulianDay + 30;
                vm.UtcOffset = sky.Context.GeoLocation.UtcOffset;

                if (ViewManager.ShowDialog(vm) ?? false)
                {
                    sky.Calculate();
                }
            }
            else
            {
                var vm = ViewManager.CreateViewModel<TracksListVM>();
                if (ViewManager.ShowDialog(vm) ?? false)
                {
                    sky.Calculate();
                }
            }
        }
    }
}
