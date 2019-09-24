using Planetarium.Config;
using Planetarium.Objects;
using Planetarium.Plugins.Tracks.ViewModels;
using Planetarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.Plugins.Tracks
{
    public class Plugin : AbstractPlugin
    {
        private IViewManager viewManager;
        private ISky sky;
        private ISkyMap map;

        public Plugin(ISky sky, ISkyMap map, IViewManager viewManager)
        {
            this.sky = sky;
            this.map = map;
            this.viewManager = viewManager;

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
                var vm = viewManager.CreateViewModel<MotionTrackVM>();
                vm.TrackId = Guid.NewGuid();
                vm.SelectedBody = body;
                vm.JulianDayFrom = sky.Context.JulianDay;
                vm.JulianDayTo = sky.Context.JulianDay + 30;
                vm.UtcOffset = sky.Context.GeoLocation.UtcOffset;

                if (viewManager.ShowDialog(vm) ?? false)
                {
                    sky.Calculate();
                }
            }
            else
            {
                var vm = viewManager.CreateViewModel<TracksListVM>();
                if (viewManager.ShowDialog(vm) ?? false)
                {
                    sky.Calculate();
                }
            }
        }
    }
}
