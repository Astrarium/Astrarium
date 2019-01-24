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
        private Settings settings;
        private bool fullScreen = false;

        public FormMain()
        {
            InitializeComponent();

            settings = new Settings();
            settings.Load();

            sky = new Sky();
            sky.Calculators.Add(new MilkyWayCalc(sky));
            sky.Calculators.Add(new CelestialGridCalc(sky));
            sky.Calculators.Add(new ConstellationsCalc(sky));
            sky.Calculators.Add(new StarsCalc(sky));
            sky.Calculators.Add(new SolarCalc(sky));
            sky.Calculators.Add(new LunarCalc(sky));
            sky.Calculators.Add(new PlanetsCalc(sky));
            sky.Calculators.Add(new DeepSkyCalc(sky));

            sky.Initialize();
            sky.Calculate();


            var moon = sky.Get<Moon>("Moon");

            var watch = System.Diagnostics.Stopwatch.StartNew();

            var ephems = sky.GetEphemeris(moon, sky.Context.JulianDay, sky.Context.JulianDay + 30, new string[] { "RTS.Rise", "RTS.Transit", "RTS.Set", "RTS.RiseAzimuth", "RTS.TransitAltitude", "RTS.SetAzimuth"/*, "Equatorial.Alpha", "Equatorial.Delta"*/ });
            watch.Stop();
            Console.WriteLine("ELASPSED ms: " + watch.ElapsedMilliseconds);

            foreach (var e in ephems)
            {
                Console.WriteLine(e["RTS.Rise"].ToString() + " " + e["RTS.Transit"].ToString() + " " + e["RTS.Set"].ToString());
            }

            ISkyMap map = new SkyMap();
            map.Renderers.Add(new DeepSkyRenderer(sky, map, settings));
            map.Renderers.Add(new MilkyWayRenderer(sky, map, settings));
            map.Renderers.Add(new ConstellationsRenderer(sky, map, settings));
            map.Renderers.Add(new CelestialGridRenderer(sky, map, settings));
            map.Renderers.Add(new StarsRenderer(sky, map, settings));
            map.Renderers.Add(new SolarSystemRenderer(sky, map, settings));
            map.Renderers.Add(new GroundRenderer(sky, map, settings));
            map.Initialize();

            skyView.SkyMap = map;
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

            var obj = skyView.SkyMap.VisibleObjects.FirstOrDefault(c => Angle.Separation(hor, c.Horizontal) < 1);            
        }

        private void skyView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.D)
            {
                double jd = sky.Context.JulianDay;
                double deltaT = Date.DeltaT(sky.Context.JulianDay) / 86400;
                double tzone = 3.0 / 24;

                using (var frmDateTime = new FormDateTime(jd + tzone - deltaT))
                {
                    if (frmDateTime.ShowDialog(skyView) == DialogResult.OK)
                    {
                        jd = frmDateTime.JulianDay;
                        deltaT = Date.DeltaT(jd) / 86400;
                        sky.Context.JulianDay = jd - tzone + deltaT;
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
        }

        private void Settings_OnSettingChanged(string settingName, object settingValue)
        {
            skyView.Invalidate();
        }

        private void skyView_DoubleClick(object sender, EventArgs e)
        {
            MouseEventArgs me = e as MouseEventArgs;
            var body = skyView.SkyMap.FindObject(me.Location);
            if (body != null)
            {
                skyView.SkyMap.SelectedObject = body;
                var info = sky.GetInfo(body);
                if (info != null)
                {
                    FormObjectInfo formInfo = new FormObjectInfo(info);
                    var result = formInfo.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        sky.Context.JulianDay = formInfo.JulianDay;                        
                        sky.Calculate();
                        skyView.SkyMap.ViewAngle = 3;
                        skyView.SkyMap.Center = new CrdsHorizontal(body.Horizontal);
                        skyView.Invalidate();
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
