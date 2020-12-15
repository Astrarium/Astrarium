using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Astrarium.Types;

namespace Astrarium.Plugins.FOV
{
    public class FieldOfViewPlugin : AbstractPlugin
    {
        private ISkyMap map;
        private ISettings settings;
        private List<FovFrame> fovFrames;

        public FieldOfViewPlugin(ISkyMap map, ISettings settings)
        {
            this.map = map;
            this.settings = settings;

            SettingItems.Add(null, new SettingItem("FovFrames", new List<FovFrame>()));

            MenuItem fovMenu = new MenuItem("$FovPlugin.Menu.FOV");
            fovMenu.AddBinding(new SimpleBinding(this, nameof(FrameMenuItems), nameof(MenuItem.SubItems)));
            MenuItems.Add(MenuItemPosition.MainMenuTools, fovMenu);

            MenuItem scaleToFOVMenu = new MenuItem("$FovPlugin.ContextMenu.ScaleToFOV");
            scaleToFOVMenu.AddBinding(new SimpleBinding(this, nameof(FrameContextMenuItems), nameof(MenuItem.SubItems)));
            scaleToFOVMenu.AddBinding(new SimpleBinding(this, nameof(IsContextMenuVisible), nameof(MenuItem.IsVisible)));
            MenuItems.Add(MenuItemPosition.ContextMenu, scaleToFOVMenu);
        }

        public ObservableCollection<MenuItem> FrameMenuItems =>
            fovFrames.Any() ?            
            new ObservableCollection<MenuItem>(fovFrames
                .Select(f => {
                    var menuItem = new MenuItem(f.Label, new Command<MenuItemCommandParameter>(MenuItemChecked));
                    menuItem.CommandParameter = new MenuItemCommandParameter() { MenuItem = menuItem, Frame = f };
                    menuItem.IsChecked = f.Enabled;
                    return menuItem;
                }).Concat(new MenuItem[] { null, new MenuItem("$FovPlugin.Menu.FOV.Manage", new Command(OpenFovFramesList)) })) :             
            new ObservableCollection<MenuItem>(new MenuItem[] { new MenuItem("$FovPlugin.Menu.FOV.Add", new Command(AddFovFrame)) });

        public bool IsContextMenuVisible => fovFrames.Any(f => f.Enabled);

        public ObservableCollection<MenuItem> FrameContextMenuItems =>
            fovFrames.Any(f => f.Enabled) ?
            new ObservableCollection<MenuItem>(fovFrames.Where(f => f.Enabled)
                .Select(f => {
                    var menuItem = new MenuItem(f.Label, new Command<MenuItemCommandParameter>(ContextMenuItemSelected));
                    menuItem.CommandParameter = new MenuItemCommandParameter() { MenuItem = menuItem, Frame = f };
                    return menuItem;
                })):
            new ObservableCollection<MenuItem>();

        private void ContextMenuItemSelected(MenuItemCommandParameter param)
        {
            double fov = 1;

            if (param.Frame is CircularFovFrame circularFovFrame)
            {
                fov = circularFovFrame.Size;
            }
            else if (param.Frame is CameraFovFrame cameraFovFrame)
            {
                double w = cameraFovFrame.Width;
                double h = cameraFovFrame.Height;
                fov = Math.Sqrt(w * w + h * h) * 1.7;
            }

            if (map.SelectedObject != null)
            {
                map.GoToObject(map.SelectedObject, TimeSpan.FromSeconds(1), fov);
            }
            else
            {
                map.GoToPoint(map.MousePosition, TimeSpan.FromSeconds(1), fov);
            }
        }

        private void MenuItemChecked(MenuItemCommandParameter param)
        {
            param.MenuItem.IsChecked = !param.MenuItem.IsChecked;
            param.Frame.Enabled = param.MenuItem.IsChecked;

            settings.Set("FovFrames", fovFrames);
            settings.Save();

            NotifyPropertyChanged(
                nameof(FrameContextMenuItems), 
                nameof(IsContextMenuVisible));
        }

        private void AddFovFrame()
        {
            var viewModel = ViewManager.CreateViewModel<FovSettingsVM>();
            viewModel.Frame = new TelescopeFovFrame() { Id = Guid.NewGuid(), Color = new SkyColor(Color.Purple) };
            if (ViewManager.ShowDialog(viewModel) ?? false)
            {
                fovFrames.Add(viewModel.Frame);
                settings.Set("FovFrames", fovFrames);
                settings.Save();
                NotifyPropertyChanged(
                    nameof(FrameMenuItems), 
                    nameof(FrameContextMenuItems), 
                    nameof(IsContextMenuVisible));
            }
        }

        private void OpenFovFramesList()
        {
            var vm = ViewManager.CreateViewModel<FovFramesListVM>();
            ViewManager.ShowDialog(vm);
            NotifyPropertyChanged(
                nameof(FrameMenuItems), 
                nameof(FrameContextMenuItems), 
                nameof(IsContextMenuVisible));
        }

        public override void Initialize()
        {
            fovFrames = settings.Get("FovFrames", new List<FovFrame>());
        }

        private class MenuItemCommandParameter
        {
            public MenuItem MenuItem { get; set; }
            public FovFrame Frame { get; set; }
        }
    }
}
