using ADK.Demo.Calculators;
using ADK.Demo.Objects;
using ADK.Demo.Renderers;
using ADK.Demo.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ADK.Demo
{
    public partial class FormMain : Form
    {
        private Sky sky;

        public FormMain()
        {
            InitializeComponent();

            sky = new Sky();
            sky.Calculators.Add(new MilkyWayCalc(sky));
            sky.Calculators.Add(new CelestialGridCalc(sky));
            sky.Calculators.Add(new BordersCalc(sky));
            sky.Calculators.Add(new StarsCalc(sky));
            sky.Calculators.Add(new SolarCalc(sky));
            sky.Calculators.Add(new LunarCalc(sky));
            sky.Calculators.Add(new PlanetsCalc(sky));

            sky.Initialize();
            sky.Calculate();

            ISkyMap map = new SkyMap();
            map.Renderers.Add(new MilkyWayRenderer(sky, map));
            map.Renderers.Add(new BordersRenderer(sky, map));
            map.Renderers.Add(new CelestialGridRenderer(sky, map));
            map.Renderers.Add(new StarsRenderer(sky, map));
            map.Renderers.Add(new SolarSystemRenderer(sky, map));
            //map.Renderers.Add(new GroundRenderer(sky, map));
            map.Initialize();

            skyView.SkyMap = map;
        }

        private void skyView_MouseMove(object sender, MouseEventArgs e)
        {
            var hor = skyView.SkyMap.Projection.Invert(e.Location);

            var eq = hor.ToEquatorial(sky.GeoLocation, sky.SiderealTime);

            // precessional elements for converting from current to B1875 epoch
            var p1875 = Precession.ElementsFK5(sky.JulianDay, Date.EPOCH_B1875);

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
                double jd = sky.JulianDay;
                double deltaT = Date.DeltaT(sky.JulianDay) / 86400;
                double tzone = 3.0 / 24;

                using (var frmDateTime = new FormDateTime(jd + tzone - deltaT))
                {
                    if (frmDateTime.ShowDialog(skyView) == DialogResult.OK)
                    {
                        jd = frmDateTime.JulianDay;
                        deltaT = Date.DeltaT(jd) / 86400;
                        sky.JulianDay = jd - tzone + deltaT;
                        sky.Calculate();
                        skyView.Invalidate();

                        var moon = sky.Get<Moon>("Moon");
                        skyView.SkyMap.Center = new CrdsHorizontal(moon.Horizontal);
                        skyView.SkyMap.ViewAngle = moon.Semidiameter * 2 * 10 / 3600; ;
                        skyView.Invalidate();
                    }
                }
            }
            else if (e.KeyCode == Keys.A)
            {
                sky.JulianDay += 1 / 24.0 / 2;
                sky.Calculate();
                //skyView.Invalidate();

                var jup = sky.Get<ICollection<Planet>>("Planets").FirstOrDefault(p => p.Number == Planet.JUPITER);
                skyView.SkyMap.Center = new CrdsHorizontal(jup.Horizontal);
                skyView.SkyMap.ViewAngle = jup.Semidiameter * 2 * 1 / 3600;
                skyView.Invalidate();
            }
            else if (e.KeyCode == Keys.S)
            {
                sky.JulianDay -= 1 / 24.0 / 2 ;
                sky.Calculate();
                //skyView.Invalidate();

                var jup = sky.Get<ICollection<Planet>>("Planets").FirstOrDefault(p => p.Number == Planet.JUPITER);
                skyView.SkyMap.Center = new CrdsHorizontal(jup.Horizontal);
                skyView.SkyMap.ViewAngle = jup.Semidiameter * 2 * 1 / 3600;
                skyView.Invalidate();
            }
        }

        private void skyView_DoubleClick(object sender, EventArgs e)
        {
            MouseEventArgs me = e as MouseEventArgs;
            MouseButtons buttonPushed = me.Button;
            var hor = skyView.SkyMap.Projection.Invert(me.Location);
            skyView.SkyMap.SelectedObject = skyView.SkyMap.VisibleObjects.FirstOrDefault(c => Angle.Separation(hor, c.Horizontal) < 1);
        }
    }
}
