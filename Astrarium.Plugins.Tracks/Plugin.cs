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
    public class Plugin : AbstractPlugin
    {
        private readonly ISky sky;
        private readonly ISkyMap map;
        private readonly TrackCalc trackCalc;

        public Plugin(ISky sky, ISkyMap map, TrackCalc trackCalc)
        {
            this.sky = sky;
            this.map = map;
            this.trackCalc = trackCalc;

            this.map.SelectedObjectChanged += (o) => NotifyPropertyChanged(nameof(IsMotionTrackEnabled));

            var menuTrack = new MenuItem("Motion track", new Command(ShowMotionTrackWindow));
            menuTrack.AddBinding(new SimpleBinding(this, nameof(IsMotionTrackEnabled), nameof(MenuItem.IsEnabled)));
            MenuItems.Add(MenuItemPosition.ContextMenu, menuTrack);

            var menuTracksList = new MenuItem("Motion tracks", new Command(ShowTracksListWindow));

            MenuItems.Add(MenuItemPosition.MainMenuTools, menuTracksList);
        }

        public bool IsMotionTrackEnabled
        {
            get
            {
                var body = map.SelectedObject;
                return body != null && body is IMovingObject;
            }
        }

        private void ShowMotionTrackWindow()
        {
            var body = map.SelectedObject;
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

        private void ShowTracksListWindow()
        {
            if (IsMotionTrackEnabled)
            {
                ShowMotionTrackWindow();
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
