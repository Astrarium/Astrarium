using Astrarium.Plugins.Tracks.ViewModels;
using Astrarium.Types;
using System;
using System.Windows.Input;

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
            this.trackCalc.Tracks.CollectionChanged += (s, e) => NotifyPropertyChanged(nameof(IsTracksListEnabled));

            var contextMenuTrack = new MenuItem("$Astrarium.Plugins.Tracks.ContextMenu", new Command(ShowMotionTrackWindow));
            contextMenuTrack.AddBinding(new SimpleBinding(this, nameof(IsMotionTrackEnabled), nameof(MenuItem.IsEnabled)));
            MenuItems.Add(MenuItemPosition.ContextMenu, contextMenuTrack);

            var toolsMenuTracks = new MenuItem("$Astrarium.Plugins.Tracks.ToolsMenu", new Command(ShowTracksListWindow));
            
            var menuAddTrack = new MenuItem("$Astrarium.Plugins.Tracks.ToolsMenu.Add", new Command(ShowMotionTrackWindow));
            menuAddTrack.HotKey = new KeyGesture(Key.T, ModifierKeys.Control, "Ctrl+T");

            var menuTracksList = new MenuItem("$Astrarium.Plugins.Tracks.ToolsMenu.List", new Command(ShowTracksListWindow));
            menuTracksList.AddBinding(new SimpleBinding(this, nameof(IsTracksListEnabled), nameof(MenuItem.IsEnabled)));
            menuTracksList.HotKey = new KeyGesture(Key.T, ModifierKeys.Control | ModifierKeys.Shift, "Ctrl+Shift+T");

            toolsMenuTracks.SubItems.Add(menuAddTrack);
            toolsMenuTracks.SubItems.Add(menuTracksList);
           
            MenuItems.Add(MenuItemPosition.MainMenuTools, toolsMenuTracks);
        }

        public bool IsMotionTrackEnabled
        {
            get
            {
                var body = map.SelectedObject;
                return body != null && body is IMovingObject;
            }
        }

        public bool IsTracksListEnabled
        {
            get
            {
                return trackCalc.Tracks.Count > 0;
            }
        }

        private void ShowMotionTrackWindow()
        {
            var body = IsMotionTrackEnabled ? map.SelectedObject : null;
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
            if (IsTracksListEnabled)
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
