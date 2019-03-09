using ADK.Demo;
using ADK.Demo.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WF = System.Windows.Forms;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ADK;
using ADK.Demo.Config;
using ADK.Demo.Objects;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using Planetarium.Views;

namespace Planetarium
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Sky sky;
        private SkyView skyView;
        private Settings settings;

        private bool fullScreen;

        public MainWindow(ISkyMap map, Sky sky, Settings settings)
        {
            InitializeComponent();

            sky.Initialize();
            map.Initialize();

            sky.Calculate();

            this.sky = sky;
            this.settings = settings;
            skyView = new SkyView();
            skyView.SkyMap = map;

            skyView.MouseDoubleClick += skyView_DoubleClick;
            skyView.MouseClick += SkyView_MouseClick;
            skyView.MouseMove += skyView_MouseMove;
            skyView.MouseWheel += SkyView_MouseWheel;

            Host.KeyDown += skyView_KeyDown;
            Host.Child = skyView;
        }

        private void SkyView_MouseWheel(object sender, WF.MouseEventArgs e)
        {
            skyView.Zoom(e.Delta);
        }

        private void SkyView_MouseClick(object sender, WF.MouseEventArgs e)
        {
            if (e.Button == WF.MouseButtons.Right && e.Clicks == 1)
            {
                var body = skyView.SkyMap.FindObject(e.Location);
                skyView.SkyMap.SelectedObject = body;
                skyView.Invalidate();
                
                // show context menu
                Host.ContextMenu.IsOpen = true;
            }
        }

        private void skyView_DoubleClick(object sender, EventArgs e)
        {
            var me = e as WF.MouseEventArgs;
            var body = skyView.SkyMap.FindObject(me.Location);
            skyView.SkyMap.SelectedObject = body;
            skyView.Invalidate();
            if (body != null)
            {
                var info = sky.GetInfo(body);
                if (info != null)
                {
                    ObjectInfoWindow wObjectInfo = new ObjectInfoWindow() { Owner = this };
                    wObjectInfo.SetObjectInfo(info);
                    bool? dialogResult = wObjectInfo.ShowDialog();
                    if (dialogResult != null && dialogResult.Value)
                    {
                        sky.Context.JulianDay = wObjectInfo.JulianDay;
                        sky.Calculate();
                        skyView.SkyMap.GoToObject(body, TimeSpan.Zero);
                    }
                }
            }
        }

        private void skyView_MouseMove(object sender, WF.MouseEventArgs e)
        {
            skyView.Focus();

            var hor = skyView.SkyMap.Projection.Invert(e.Location);

            var eq = hor.ToEquatorial(sky.Context.GeoLocation, sky.Context.SiderealTime);

            // precessional elements for converting from current to B1875 epoch
            var p1875 = Precession.ElementsFK5(sky.Context.JulianDay, Date.EPOCH_B1875);

            // Equatorial coordinates for B1875 epoch
            CrdsEquatorial eq1875 = Precession.GetEquatorialCoordinates(eq, p1875);

            lblEquatorialCoordinates.Text = eq.ToString();
            lblHorizontalCoordinates.Text = hor.ToString();
            lblConstellation.Text = Constellations.FindConstellation(eq1875);
            lblViewAngle.Text = skyView.SkyMap.ViewAngle.ToString();
        }

        private void Settings_OnSettingChanged(string settingName, object settingValue)
        {
            skyView.Invalidate();
        }

        private async void skyView_KeyDown(object sender, KeyEventArgs e)
        {
            // Add = Zoom In
            if (e.Key == Key.Add)
            {
                skyView.Zoom(1);
            }

            // Subtract = Zoom Out
            else if (e.Key == Key.Subtract)
            {
                skyView.Zoom(-1);
            }
            else if (e.Key == Key.D)
            {
                using (var frmDateTime = new FormDateTime(sky.Context.JulianDay, sky.Context.GeoLocation.UtcOffset))
                {
                    if (frmDateTime.ShowDialog(skyView) == WF.DialogResult.OK)
                    {
                        sky.Context.JulianDay = frmDateTime.JulianDay;
                        sky.Calculate();
                        skyView.Invalidate();
                    }
                }
            }
            else if (e.Key == Key.A)
            {
                sky.Context.JulianDay += 1;
                sky.Calculate();
                skyView.Invalidate();
            }
            else if (e.Key == Key.S)
            {
                sky.Context.JulianDay -= 1;
                sky.Calculate();
                skyView.Invalidate();
            }
            else if (e.Key == Key.O)
            {
                using (var frmSettings = new FormSettings(settings))
                {
                    settings.SettingValueChanged += Settings_OnSettingChanged;
                    frmSettings.ShowDialog();
                    settings.SettingValueChanged -= Settings_OnSettingChanged;
                }
            }
            else if (e.Key == Key.F12)
            {
                fullScreen = !fullScreen;
                ShowFullScreen(fullScreen);
            }

            else if (e.Key == Key.F)
            {
                var wSearch = new SearchWindow(sky) { Owner = this };
                wSearch.ShowDialog();


                //using (var frmSearch = new FormSearch(sky, (b) => true))
                //{
                //    var result = frmSearch.ShowDialog();
                //    if (result == WF.DialogResult.OK)
                //    {
                //        bool show = true;
                //        var body = frmSearch.SelectedObject;
                //        if (settings.Get<bool>("Ground") && body.Horizontal.Altitude <= 0)
                //        {
                //            show = false;
                //            if (WF.DialogResult.Yes == WF.MessageBox.Show("The object is under horizon at the moment. Do you want to switch off displaying the ground?", "Question", WF.MessageBoxButtons.YesNo, WF.MessageBoxIcon.Question))
                //            {
                //                show = true;
                //                settings.Set("Ground", false);
                //            }
                //        }

                //        if (show)
                //        {
                //            skyView.SkyMap.GoToObject(body, TimeSpan.FromSeconds(1));
                //        }
                //    }
                //}
            }
            else if (e.Key == Key.E)
            {
                var formAlmanacSettings = new FormAlmanacSettings(
                    sky.Context.JulianDayMidnight,
                    sky.Context.GeoLocation.UtcOffset,
                    sky.GetEventsCategories());

                if (formAlmanacSettings.ShowDialog() == WF.DialogResult.OK)
                {
                    var events = await Task.Run(() =>
                    {
                        return sky.GetEvents(
                            formAlmanacSettings.JulianDayFrom,
                            formAlmanacSettings.JulianDayTo,
                            formAlmanacSettings.Categories);
                    });

                    var formAlmanac = new FormAlmanac(events, sky.Context.GeoLocation.UtcOffset);
                    if (formAlmanac.ShowDialog() == WF.DialogResult.OK)
                    {
                        sky.Context.JulianDay = formAlmanac.JulianDay;
                        sky.Calculate();
                        skyView.Invalidate();
                    }
                }


            }
            else if (e.Key == Key.P)
            {
                var body = skyView.SkyMap.SelectedObject;
                if (body != null)
                {
                    using (var formEphemerisSettings = new FormEphemerisSettings(sky, body))
                    {
                        if (formEphemerisSettings.ShowDialog() == WF.DialogResult.OK)
                        {
                            var ephem = await Task.Run(() => sky.GetEphemerides(
                                formEphemerisSettings.SelectedObject,
                                formEphemerisSettings.JulianDayFrom,
                                formEphemerisSettings.JulianDayTo,
                                formEphemerisSettings.Step,
                                formEphemerisSettings.Categories
                            ));

                            var formEphemeris = new FormEphemeris(ephem,
                                formEphemerisSettings.JulianDayFrom,
                                formEphemerisSettings.JulianDayTo,
                                formEphemerisSettings.Step,
                                sky.Context.GeoLocation.UtcOffset);

                            formEphemeris.Show();
                        }
                    }
                }
            }
            else if (e.Key == Key.T)
            {
                var body = skyView.SkyMap.SelectedObject;
                if (body != null && body is IMovingObject)
                {
                    var wTrackProperties = new TrackPropertiesWindow() { Owner = this };
                    wTrackProperties.ShowDialog();



                    /*
                    var formTrackSettings = viewManager.GetForm<FormTrackSettings>();
                    formTrackSettings.Track = new Track() { Body = body, From = sky.Context.JulianDay, To = sky.Context.JulianDay + 30, LabelsStep = TimeSpan.FromDays(1) };
                    if (formTrackSettings.ShowDialog() == DialogResult.OK)
                    {
                        skyView.Invalidate();
                    }
                    */
                }
            }
        }

        private void ShowFullScreen(bool fullscreen)
        {
            if (fullscreen)
            {
                this.WindowState = WindowState.Normal;
                
            }
            else
            {
                this.WindowState = WindowState.Maximized;
            }
        }

        
    }

    


}
