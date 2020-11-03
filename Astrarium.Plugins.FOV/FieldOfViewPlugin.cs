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
        private ISettings settings;
        private List<FovFrame> fovFrames;

        public FieldOfViewPlugin(ISettings settings)
        {
            this.settings = settings;

            SettingItems.Add(null, new SettingItem("FovFrames", new List<FovFrame>()));

            MenuItem fovMenu = new MenuItem("$FovPlugin.Menu.FOV");
            fovMenu.AddBinding(new SimpleBinding(this, nameof(FrameMenuItems), nameof(MenuItem.SubItems)));
            MenuItems.Add(MenuItemPosition.MainMenuTools, fovMenu);
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

        private void MenuItemChecked(MenuItemCommandParameter param)
        {
            param.MenuItem.IsChecked = !param.MenuItem.IsChecked;
            param.Frame.Enabled = param.MenuItem.IsChecked;

            settings.Set("FovFrames", fovFrames);
            settings.Save();
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
                NotifyPropertyChanged(nameof(FrameMenuItems));
            }
        }

        private void OpenFovFramesList()
        {
            var vm = ViewManager.CreateViewModel<FovFramesListVM>();
            ViewManager.ShowDialog(vm);
            NotifyPropertyChanged(nameof(FrameMenuItems));
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
