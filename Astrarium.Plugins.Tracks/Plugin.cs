using Astrarium.Config;
using Astrarium.Objects;
using Astrarium.Plugins.Tracks.ViewModels;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Tracks
{
    public class Plugin : AbstractPlugin
    {
        private ISky sky;
        private ISkyMap map;

        public Plugin(ISky sky, ISkyMap map)
        {
            this.sky = sky;
            this.map = map;

            AddContextMenuItem(new ContextMenuItem("Motion track", ShowMotionTrackWindow, IsMotionTrackEnabled, () => true));
        }

        private bool IsMotionTrackEnabled()
        {
            var body = map.SelectedObject;
            return body != null && body is IMovingObject;
        }

        private void ShowMotionTrackWindow()
        {
            var body = map.SelectedObject;

            if (IsMotionTrackEnabled())
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
