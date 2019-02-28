using ADK.Demo.Calculators;
using ADK.Demo.Config;
using ADK.Demo.Objects;
using ADK.Demo.Renderers;
using ADK.Demo.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ADK.Demo
{
    public partial class FormMain : Form
    {
        private Sky sky;
        private ISkyMap skyMap;
        private Settings settings;
        private bool fullScreen = false;

        public FormMain(Sky sky, ISkyMap skyMap, Settings settings)
        {
            InitializeComponent();

            this.sky = sky;
            this.skyMap = skyMap;
            this.settings = settings;

            sky.Initialize();
            skyMap.Initialize();

            //sky.Context = new SkyContext(Date.JulianEphemerisDay(new Date(2019, 1, 21 + 5 / 24.0)), sky.Context.GeoLocation);

            sky.Calculate();

            //var moon = sky.GetEphemeris()
            //var watch = System.Diagnostics.Stopwatch.StartNew();
            //var ephems = sky.GetEphemeris(moon, sky.Context.JulianDay, sky.Context.JulianDay + 365, new string[] { "RTS.Rise", "RTS.Transit", "RTS.Set", "RTS.RiseAzimuth", "RTS.TransitAltitude", "RTS.SetAzimuth"/*, "Equatorial.Alpha", "Equatorial.Delta"*/ });
            //watch.Stop();
            //Console.WriteLine("ELASPSED ms: " + watch.ElapsedMilliseconds);

            //foreach (var e in ephems)
            //{
            //    Console.WriteLine(e["RTS.Rise"].ToString() + " " + e["RTS.Transit"].ToString() + " " + e["RTS.Set"].ToString());
            //}

            //var moon = sky.Get<Moon>("Moon");
            //var mercury = sky.Get<ICollection<Planet>>("Planets").ElementAt(0);

            //var mercuryTrack = new Track()
            //{
            //    Body = mercury,
            //    From = sky.Context.JulianDay,
            //    To = sky.Context.JulianDay + 30,
            //    LabelsStep = TimeSpan.FromDays(7)
            //};

            //var moonTrack = new Track()
            //{
            //    Body = moon,
            //    From = sky.Context.JulianDay,
            //    To = sky.Context.JulianDay + 365,
            //    LabelsStep = TimeSpan.FromDays(1)
            //};

            //sky.AddTrack(mercuryTrack);
            //sky.AddTrack(moonTrack);

            //var watch = System.Diagnostics.Stopwatch.StartNew();

            //SkyContext ctx = new SkyContext(new Date(2019, 1, 1).ToJulianEphemerisDay(), sky.Context.GeoLocation);
            
            //var events = sky.GetEvents(ctx.JulianDayMidnight, ctx.JulianDayMidnight + 365);
            //watch.Stop();
            //Console.WriteLine("ELASPSED ms: " + watch.ElapsedMilliseconds);

            //foreach (var e in events)
            //{
            //    Console.WriteLine($"{Formatters.DateTime.Format(new Date(e.JulianDay, 3))} ({e.JulianDay}): {e.Text}");
            //}

            skyView.SkyMap = skyMap;
        }

        private void skyView_MouseMove(object sender, MouseEventArgs e)
        {
            var hor = skyView.SkyMap.Projection.Invert(e.Location);

            var eq = hor.ToEquatorial(sky.Context.GeoLocation, sky.Context.SiderealTime);

            // precessional elements for converting from current to B1875 epoch
            var p1875 = Precession.ElementsFK5(sky.Context.JulianDay, Date.EPOCH_B1875);

            // Equatorial coordinates for B1875 epoch
            CrdsEquatorial eq1875 = Precession.GetEquatorialCoordinates(eq, p1875);

            Text = 
                hor.ToString() + " / " +
                eq.ToString() + " / " +
                skyView.SkyMap.ViewAngle + " / " +
                Constellations.FindConstellation(eq1875);          
        }

        private void skyView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.D)
            {
                using (var frmDateTime = new FormDateTime(sky.Context.JulianDay, sky.Context.GeoLocation.UtcOffset))
                {
                    if (frmDateTime.ShowDialog(skyView) == DialogResult.OK)
                    {
                        sky.Context.JulianDay = frmDateTime.JulianDay;
                        sky.Calculate();
                        skyView.Invalidate();
                    }
                }
            }
            else if (e.KeyCode == Keys.A)
            {
                sky.Context.JulianDay += 1;
                sky.Calculate();
                skyView.Invalidate();
            }
            else if (e.KeyCode == Keys.S)
            {
                sky.Context.JulianDay -= 1;
                sky.Calculate();
                skyView.Invalidate();
            }
            else if (e.KeyCode == Keys.O)
            {
                using (var frmSettings = new FormSettings(settings))
                {
                    settings.SettingValueChanged += Settings_OnSettingChanged;
                    frmSettings.ShowDialog();
                    settings.SettingValueChanged -= Settings_OnSettingChanged;
                }
            }
            else if (e.KeyCode == Keys.F12)
            {
                fullScreen = !fullScreen;
                ShowFullScreen(fullScreen);
            }

            else if (e.Control && e.KeyCode == Keys.F)
            {
                using (var frmSearch = new FormSearch(sky.Search))
                {
                    var result = frmSearch.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        bool show = true;
                        var body = frmSearch.SelectedObject;
                        if (settings.Get<bool>("Ground") && body.Horizontal.Altitude <= 0)
                        {
                            show = false;
                            if (DialogResult.Yes == MessageBox.Show("The object is under horizon at the moment. Do you want to switch off displaying the ground?", "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
                            {
                                show = true;
                                settings.Set("Ground", false);
                            }
                        }

                        if (show)
                        {
                            skyView.SkyMap.GoToObject(body, TimeSpan.FromSeconds(1));
                        }
                    }
                }
            }
            else if (e.KeyCode == Keys.E)
            {
                var body = skyView.SkyMap.SelectedObject;
                if (body != null)
                {
                    var ephem = sky.GetEphemeris(body, sky.Context.JulianDayMidnight, sky.Context.JulianDayMidnight + 365, 1, new[] {
                        "Visibility.Duration",
                        "Visibility.Period"
                    });
                }                
            }
        }

        private void Settings_OnSettingChanged(string settingName, object settingValue)
        {
            skyView.Invalidate();
        }

        private void skyView_DoubleClick(object sender, EventArgs e)
        {
            MouseEventArgs me = e as MouseEventArgs;
            var body = skyView.SkyMap.FindObject(me.Location);
            skyView.SkyMap.SelectedObject = body;
            skyView.Invalidate();
            if (body != null)
            {
                var info = sky.GetInfo(body);
                if (info != null)
                {
                    FormObjectInfo formInfo = new FormObjectInfo(info);
                    var result = formInfo.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        sky.Context.JulianDay = formInfo.JulianDay;                        
                        sky.Calculate();
                        skyView.SkyMap.GoToObject(body, TimeSpan.Zero);
                    }
                }
            }
        }

        private void ShowFullScreen(bool fullscreen)
        {
            if (fullscreen)
            {
                this.WindowState = FormWindowState.Normal;
                this.FormBorderStyle = FormBorderStyle.None;
                this.Bounds = Screen.FromControl(this).Bounds;
            }
            else
            {
                this.WindowState = FormWindowState.Maximized;
                this.FormBorderStyle = FormBorderStyle.Sizable;
            }
        }
    }
}
