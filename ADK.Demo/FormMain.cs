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
        private CrdsGeographical location = new CrdsGeographical(56.3333, 44);
        private double siderealTime = 17;

        public FormMain()
        {
            InitializeComponent();
            skyView.SkyMap = new SkyMap();
        }

        private void skyView_MouseMove(object sender, MouseEventArgs e)
        {
            Text = 
                skyView.SkyMap.CoordinatesByPoint(e.Location).ToString() + " / " +
                skyView.SkyMap.CoordinatesByPoint(e.Location).ToEquatorial(location, siderealTime).ToString() + " / " +
                skyView.SkyMap.ViewAngle;
        }
    }
}
