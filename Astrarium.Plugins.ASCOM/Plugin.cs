using Astrarium.Algorithms;
using Astrarium.Plugins.ASCOM.Controls;
using Astrarium.Plugins.ASCOM.ViewModels;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Astrarium.Plugins.ASCOM
{
    public class Plugin : AbstractPlugin
    {
        private readonly IAscomProxy ascom;
        private readonly ISkyMap map;
        private readonly ISky sky;
        private readonly ISettings settings;
        private readonly IJoystickManager joystickManager;

        public Plugin(ISkyMap map, ISky sky, IJoystickManager joystickManager, ISettings settings)
        {
            var menuAscom = new MenuItem("$Menu.Telescope");

            this.ascom = Ascom.Proxy;
            this.joystickManager = joystickManager;
            this.map = map;
            this.sky = sky;
            this.settings = settings;

            this.joystickManager.ButtonStateChanged += HandleButtonStateChanged;

            DefineSetting("ASCOMTelescopeId", "", isPermanent: true);

            var menuConnectTelescope = new MenuItem("$Menu.ConnectToTelescope", new Command(ConnectTelescope));
            menuConnectTelescope.AddBinding(new SimpleBinding(this, nameof(IsConnectTelescopeVisible), nameof(MenuItem.IsVisible)));

            var menuDisconnectTelescope = new MenuItem("$Menu.DisconnectTelescope", new Command(DisconnectTelescope));
            menuDisconnectTelescope.AddBinding(new SimpleBinding(ascom, nameof(ascom.IsConnected), nameof(MenuItem.IsVisible)));
            menuDisconnectTelescope.AddBinding(new SimpleBinding(this, nameof(DisconnectTitle), nameof(MenuItem.Header)));

            var menuFindCurrentPoint = new MenuItem("$Menu.FindCurrentPoint", new Command(FindCurrentPoint));
            menuFindCurrentPoint.AddBinding(new SimpleBinding(ascom, nameof(ascom.IsConnected), nameof(MenuItem.IsEnabled)));

            var menuAbortSlew = new MenuItem("$Menu.AbortSlew", new Command(AbortSlew));
            menuAbortSlew.AddBinding(new SimpleBinding(ascom, nameof(ascom.IsSlewing), nameof(MenuItem.IsEnabled)));

            var menuFindHome = new MenuItem("$Menu.Home", new Command(FindHome));
            menuFindHome.AddBinding(new SimpleBinding(ascom, nameof(ascom.AtHome), nameof(MenuItem.IsChecked)));
            menuFindHome.AddBinding(new SimpleBinding(ascom, nameof(ascom.IsConnected), nameof(MenuItem.IsEnabled)));

            var menuPark = new MenuItem("$Menu.Park", new Command(ParkOrUnpark));
            menuPark.AddBinding(new SimpleBinding(ascom, nameof(ascom.AtPark), nameof(MenuItem.IsChecked)));
            menuPark.AddBinding(new SimpleBinding(ascom, nameof(ascom.IsConnected), nameof(MenuItem.IsEnabled)));

            var menuTrack = new MenuItem("$Menu.Track", new Command(SwitchTracking));
            menuTrack.AddBinding(new SimpleBinding(ascom, nameof(ascom.IsTracking), nameof(MenuItem.IsChecked)));
            menuTrack.AddBinding(new SimpleBinding(ascom, nameof(ascom.IsConnected), nameof(MenuItem.IsEnabled)));

            var menuSetup = new MenuItem("$Menu.Setup", new Command(ShowSetupDialog));
            menuSetup.AddBinding(new SimpleBinding(ascom, nameof(ascom.IsConnected), nameof(MenuItem.IsEnabled)));

            var menuInfo = new MenuItem("$Menu.AscomInformation", new Command(ShowInfo));
            menuInfo.AddBinding(new SimpleBinding(ascom, nameof(ascom.IsConnected), nameof(MenuItem.IsEnabled)));

            menuAscom.SubItems.Add(menuConnectTelescope);
            menuAscom.SubItems.Add(menuDisconnectTelescope);
            menuAscom.SubItems.Add(null);
            menuAscom.SubItems.Add(menuFindCurrentPoint);
            menuAscom.SubItems.Add(menuAbortSlew);
            menuAscom.SubItems.Add(null);
            menuAscom.SubItems.Add(menuFindHome);
            menuAscom.SubItems.Add(menuPark);
            menuAscom.SubItems.Add(menuTrack);
            menuAscom.SubItems.Add(null);
            menuAscom.SubItems.Add(menuSetup);
            menuAscom.SubItems.Add(menuInfo);

            MenuItems.Add(MenuItemPosition.MainMenuTop, menuAscom);

            var contextMenuAscom = new MenuItem("$ContextMenu.Telescope");
            contextMenuAscom.AddBinding(new SimpleBinding(this, nameof(IsContextMenuEnabled), nameof(MenuItem.IsEnabled)));

            var contextMenuSyncTo = new MenuItem("$ContextMenu.Telescope.Sync", new Command(SyncToPosition));
            contextMenuSyncTo.AddBinding(new SimpleBinding(this, nameof(IsContextMenuEnabled), nameof(MenuItem.IsEnabled)));

            var contextMenuSlewTo = new MenuItem("$ContextMenu.Telescope.Slew", new Command(SlewToPosition));
            contextMenuSlewTo.AddBinding(new SimpleBinding(this, nameof(IsContextMenuEnabled), nameof(MenuItem.IsEnabled)));

            contextMenuAscom.SubItems.Add(contextMenuSyncTo);
            contextMenuAscom.SubItems.Add(contextMenuSlewTo);

            MenuItems.Add(MenuItemPosition.ContextMenu, contextMenuAscom);

            ascom.PropertyChanged += Ascom_PropertyChanged;
            ascom.OnMessageShow += Ascom_OnMessageShow;

            DefineSetting("TelescopeMarkerColor", new SkyColor(Color.DarkOrange));
            DefineSetting("TelescopeMarkerFont", SystemFonts.DefaultFont);

            DefineSetting("TelescopeMarkerLabel", true);
            DefineSetting("TelescopeFindCurrentPointAfterConnect", false);
            DefineSetting("TelescopePollingPeriod", 500m);

            DefineSetting("TelescopeControlJoystick", false, true);
            DefineSetting("TelescopeControlJoystickDevice", Guid.Empty, true);
            DefineSetting("TelescopeControlJoystickButtons", new List<JoystickButton>(), true);

            DefineSettingsSection<AscomSettingsSection, AscomSettingsViewModel>();

            settings.SettingValueChanged += Settings_SettingValueChanged;
            settings.OnSaving += Settings_OnSaving;
        }

        private void Settings_OnSaving()
        {
            settings.Set("TelescopeControlJoystickButtons", this.joystickManager.SelectedDevice?.Buttons);
        }

        public override void Initialize()
        {
            this.ascom.PollingPeriod = (int)settings.Get<decimal>("TelescopePollingPeriod");

            var device = this.joystickManager.Devices.FirstOrDefault(x => x.Id == settings.Get<Guid>("TelescopeControlJoystickDevice"));

            if (device != null)
            {
                var buttonsMappings = settings.Get<List<JoystickButton>>("TelescopeControlJoystickButtons").Where(x => x.Action != ButtonAction.None);
                foreach (var map in buttonsMappings)
                {
                    var button = device.Buttons.FirstOrDefault(x => x.Button == map.Button);
                    if (button != null)
                    { 
                         button.Action = map.Action;
                    }
                }
            }

            this.joystickManager.SelectedDevice = device;
        }

        private void Settings_SettingValueChanged(string settingName, object value)
        {
            if (settingName == "TelescopePollingPeriod")
            {
                int period = (int)(decimal)value;
                if (period >= 100 && period <= 5000)
                {
                    ascom.PollingPeriod = period;
                }
            }
        }

        private void HandleButtonStateChanged(string buttonName, bool isPressed)
        {
            Task.Run(() =>
            {
                if (settings.Get("TelescopeControlJoystick"))
                {
                    try
                    {
                        var button = joystickManager.SelectedDevice?.Buttons.FirstOrDefault<JoystickButton>(b => b.Button == buttonName);
                        if (button != null)
                        {
                            ascom.ProcessCommand(new ButtonCommand() { Action = button.Action, IsPressed = button.IsPressed });
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"ERROR: {ex.Message}");
                    }
                }
            });
        }

        private void Ascom_OnMessageShow(string message)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() => ViewManager.ShowPopupMessage(message));
        }

        private void Ascom_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            NotifyPropertyChanged(
                nameof(IsShowSettingEnabled),
                nameof(IsConnectTelescopeVisible),
                nameof(DisconnectTitle),
                nameof(IsContextMenuEnabled)
            );
        }

        public bool IsConnectTelescopeVisible
        {
            get
            {
                return !ascom.IsAscomPlatformInstalled || (ascom.IsAscomPlatformInstalled && !ascom.IsConnected);
            }
        }

        public bool IsContextMenuEnabled
        {
            get
            {
                return ascom.IsAscomPlatformInstalled && ascom.IsConnected;
            }
        }

        public bool IsShowSettingEnabled
        {
            get
            {
                return ascom.IsAscomPlatformInstalled && !ascom.IsConnected;
            }
        }

        public string DisconnectTitle
        {
            get
            {
                return ascom.IsAscomPlatformInstalled && ascom.IsConnected ? $"{Text.Get("Menu.DisconnectTelescope")} {ascom.TelescopeName}" : Text.Get("Menu.DisconnectTelescope");
            }
        }

        private async void ConnectTelescope()
        {
            if (ascom.IsAscomPlatformInstalled)
            {
                string savedTelescopeId = settings.Get<string>("ASCOMTelescopeId");
                var telescopeId = await ascom.Connect(savedTelescopeId);
                if (!string.IsNullOrEmpty(telescopeId))
                {
                    ascom.SetDateTime(DateTime.UtcNow);
                    ascom.SetLocation(settings.Get<CrdsGeographical>("ObserverLocation"));
                    if (!string.Equals(telescopeId, savedTelescopeId))
                    {
                        settings.SetAndSave("ASCOMTelescopeId", telescopeId);
                    }
                    if (settings.Get("TelescopeFindCurrentPointAfterConnect"))
                    {
                        int period = (int)settings.Get<decimal>("TelescopePollingPeriod");
                        await Task.Delay(period);
                        FindCurrentPoint();
                    }
                }
            }
            else
            {
                ViewManager.ShowMessageBox("$NoAscom.Title", "$NoAscom.Message");
            }
        }

        private void DisconnectTelescope()
        {
            ascom.Disconnect();
        }

        private void FindCurrentPoint()
        {
            map.GoToPoint(ascom.Position.ToHorizontal(sky.Context.GeoLocation, sky.Context.SiderealTime), TimeSpan.FromSeconds(1));
        }

        private void AbortSlew()
        {
            ascom.AbortSlewing();
        }

        private void FindHome()
        {
            ascom.FindHome();
        }

        private void ParkOrUnpark()
        {
            if (ascom.AtPark)
            {
                ascom.Unpark();
            }
            else
            {
                ascom.Park();
            }
        }

        private void SwitchTracking()
        {
            ascom.SwitchTracking();
        }

        private void ShowSetupDialog()
        {
            ascom.ShowSetupDialog();
        }

        private void ShowInfo()
        {
            StringBuilder sb = new StringBuilder();

            var info = ascom.Info;

            sb.AppendLine($"**{Text.Get("TelescopeInfo.TelescopeName")}**  ");
            sb.AppendLine(info.TelescopeName);
            sb.AppendLine();

            sb.AppendLine($"**{Text.Get("TelescopeInfo.TelescopeDescription")}**  ");
            sb.AppendLine(info.TelescopeDescription);
            sb.AppendLine();

            sb.AppendLine($"**{Text.Get("TelescopeInfo.DriverVersion")}**  ");
            sb.AppendLine(info.DriverVersion);
            sb.AppendLine();

            sb.AppendLine($"**{Text.Get("TelescopeInfo.DriverDescription")}**  ");
            sb.AppendLine(info.DriverDescription);
            sb.AppendLine();

            sb.AppendLine($"**{Text.Get("TelescopeInfo.InterfaceVersion")}**  ");
            sb.AppendLine(info.InterfaceVersion);
            sb.AppendLine();

            sb.AppendLine($"**{Text.Get("TelescopeInfo.Capabilities")}**  ");
            sb.AppendLine($"CanFindHome: {info.CanFindHome}  ");
            sb.AppendLine($"CanSetTracking: {info.CanSetTracking}  ");
            sb.AppendLine($"CanSlew: {info.CanSlew}  ");
            sb.AppendLine($"CanSync: {info.CanSync}  ");
            sb.AppendLine($"CanPark: {info.CanPark}  ");
            sb.AppendLine($"CanUnpark: {info.CanUnpark}  ");

            ViewManager.ShowMessageBox("$TelescopeInfo.Title", sb.ToString());
        }

        private CrdsEquatorial GetMouseCoordinates()
        {
            var hor = map.SelectedObject?.Horizontal ?? map.MousePosition;
            var eq = hor.ToEquatorial(sky.Context.GeoLocation, sky.Context.SiderealTime);
            return eq;
        }

        private void SyncToPosition()
        {
            ascom.Sync(GetMouseCoordinates());
        }

        private void SlewToPosition()
        {
            ascom.Slew(GetMouseCoordinates());
        }
    }
}
