using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using WF = System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using System.Drawing.Drawing2D;
using ADK;
using System.Reflection;
using System.ComponentModel;

namespace Planetarium.Views
{
    /// <summary>
    /// Interaction logic for EarthMapView.xaml
    /// </summary>
    public partial class EarthMapView : UserControl
    {
        public readonly static DependencyProperty SunHourAngleProperty = DependencyProperty.Register(nameof(SunHourAngle), typeof(double), typeof(EarthMapView), new UIPropertyMetadata(null));
        public readonly static DependencyProperty ObserverLocationProperty = DependencyProperty.Register(nameof(ObserverLocation), typeof(CrdsGeographical), typeof(EarthMapView), new UIPropertyMetadata(new CrdsGeographical(0, 0), null));
        public readonly static DependencyProperty SunEquatorialProperty = DependencyProperty.Register(nameof(SunDeclination), typeof(double), typeof(EarthMapView), new UIPropertyMetadata(null));

        public double SunHourAngle
        {
            get { return (double)GetValue(SunHourAngleProperty); }
            set { SetValue(SunHourAngleProperty, value); }
        }

        public CrdsGeographical ObserverLocation
        {
            get { return (CrdsGeographical)GetValue(ObserverLocationProperty); }
            set { SetValue(ObserverLocationProperty, value); }
        }

        public double SunDeclination
        {
            get { return (double)GetValue(SunEquatorialProperty); }
            set { SetValue(SunEquatorialProperty, value); }
        }

        private System.Drawing.Image earthMap = null;
        private SolidBrush sunBrush = new SolidBrush(System.Drawing.Color.FromArgb(250, 210, 10));
        private SolidBrush nightBrush = new SolidBrush(System.Drawing.Color.FromArgb(100, 0, 0, 0));

        private int OriginX;
        private int OriginY;
        private int ScaledWidth;
        private int ScaledHeight;

        public EarthMapView()
        {
            InitializeComponent();

            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                earthMap = System.Drawing.Image.FromFile(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data", "Earth.jpg"));
            }
        }

        private void picMap_MouseMove(object sender, WF.MouseEventArgs e)
        {
            if (e.Button == WF.MouseButtons.Left)
            {
                picMap_MouseDown(sender, e);
            }
        }

        private void picMap_Paint(object sender, WF.PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.FillRectangle(System.Drawing.Brushes.Black, 0, 0, e.ClipRectangle.Width, e.ClipRectangle.Height);
            DrawMap(e.Graphics);

            e.Graphics.TranslateTransform(OriginX, OriginY);

            DrawDayNight(e.Graphics);
            DrawSubSolarPoint(e.Graphics);
            DrawLocation(e.Graphics);
        }

        /// <summary>
        /// Draws scaled map of the Earth
        /// </summary>
        /// <param name="g">Graphics instance to render on</param>
        private void DrawMap(Graphics g)
        {
            if (earthMap != null)
            {
                float width = g.ClipBounds.Width;
                float height = g.ClipBounds.Height;



                float scale = (float)Zoom * Math.Min(width / earthMap.Width, height / earthMap.Height);

                ScaledWidth = (int)(earthMap.Width * scale);
                ScaledHeight = (int)(earthMap.Height * scale);

                OriginX = ((int)width - ScaledWidth) / 2;
                OriginY = 0; //((int)height - ScaledHeight) / 2;

                g.DrawImage(earthMap, OriginX, OriginY, ScaledWidth, ScaledHeight);
            }
        }

        /// <summary>
        /// Draws Day & Night terminator and night part of the Earth map
        /// </summary>
        /// <param name="g">Graphics instance to render on</param>
        private void DrawDayNight(Graphics g)
        {            
            double K = Math.PI / 180.0;
            double tanLat, arctanLat;

            float y0 = 90;
            float x0 = 180;

            float y1, y2;
            double longitude;

            using (var gp = new GraphicsPath())
            {
                if (SunDeclination >= 0)
                    gp.AddLine(0, ScaledHeight, 0, 0);
                else
                    gp.AddLine(0, 0, 0, ScaledHeight);

                for (int i = -180; i <= 180; i++)
                {
                    longitude = i + SunHourAngle;
                    tanLat = -Math.Cos(longitude * K) / Math.Tan(SunDeclination * K);
                    arctanLat = Math.Atan(tanLat) / K;
                    y1 = y0 - (int)Math.Round(arctanLat);

                    longitude = longitude + 1;
                    tanLat = -Math.Cos(longitude * K) / Math.Tan(SunDeclination * K);
                    arctanLat = Math.Atan(tanLat) / K;
                    y2 = y0 - (int)Math.Round(arctanLat);

                    float _x1 = (float)((x0 + i) / 360.0 * ScaledWidth);
                    float _y1 = (float)(y1 / 180.0 * ScaledHeight);
                    float _x2 = (float)((x0 + i + 1) / 360.0 * ScaledWidth);
                    float _y2 = (float)(y2 / 180.0 * ScaledHeight);

                    gp.AddLine(_x1, _y1, _x2, _y2);
                }

                if (SunDeclination >= 0)
                    gp.AddLine(ScaledWidth, 0, ScaledWidth, ScaledHeight);
                else
                    gp.AddLine(ScaledWidth, ScaledHeight, ScaledWidth, 0);

                g.FillPath(nightBrush, gp);
            }
        }

        /// <summary>
        /// Draws sub-solar point on the map
        /// </summary>
        /// <param name="g">Graphics instance to render on</param>
        private void DrawSubSolarPoint(Graphics g)
        {
            float y0 = 90;
            float x0 = 180;
            float diam = 10;

            var p = new PointF();
            p.X = (float)((x0 - (int)Math.Round(SunHourAngle)) / 360.0 * ScaledWidth);
            p.Y = (float)((y0 - (int)Math.Round(SunDeclination)) / 180.0 * ScaledHeight);

            if (p.X > ScaledWidth) p.X -= ScaledWidth;
            if (p.X < 0) p.X += ScaledWidth;


            g.FillEllipse(sunBrush, p.X - diam / 2, p.Y - diam / 2, diam, diam);
        }

        /// <summary>
        /// Draws current observer location point and crossing lines on the map
        /// </summary>
        /// <param name="g">Graphics instance to render on</param>
        private void DrawLocation(Graphics g)
        {
            double x, y;
            x = (180 - ObserverLocation.Longitude) / 360.0 * ScaledWidth;
            y = (90 - ObserverLocation.Latitude) / 180.0 * ScaledHeight;
            g.DrawEllipse(Pens.Red, (float)x - 5, (float)y - 5, 10, 10);
            g.DrawLine(Pens.Brown, (float)x, 0, (float)x, ScaledHeight);
            g.DrawLine(Pens.Brown, 0, (float)y, ScaledWidth, (float)y);
        }

        private void picMap_MouseDown(object sender, WF.MouseEventArgs e)
        {
            if (e.Button == WF.MouseButtons.Left)
            {
                double Lon, Lat;
                double x = e.X - OriginX;
                double y = e.Y - OriginY;

                if (x < 0) x = 0;
                if (x > ScaledWidth) x = ScaledWidth;
                if (y < 0) y = 0;
                if (y > ScaledHeight) y = ScaledHeight;

                Lon = 180 - 360.0 / ScaledWidth * x;
                Lat = 90 - 180.0 / ScaledHeight * y;

                if (Lon == -180) Lon += 1.0 / 3600.0;
                if (Lon == 180) Lon -= 1.0 / 3600.0;
                if (Lat == -90) Lat += 1.0 / 3600.0;
                if (Lat == 90) Lat -= 1.0 / 3600.0;

                ObserverLocation.Latitude = Lat;
                ObserverLocation.Longitude = Lon;

                picMap.Invalidate();
            }
        }

        private double Zoom = 1;

        private void PicMap_MouseWheel(object sender, WF.MouseEventArgs e)
        {
            double v = Zoom;
            int delta = e.Delta;

            if (delta > 0)
            {
                v *= 1.1;
            }
            else
            {
                v /= 1.1;
            }

            if (v >= 4)
            {
                v = 4;
            }
            if (v < 1)
            {
                v = 1;
            }

            Zoom = v;

            picMap.Invalidate();
        }
    }
}
