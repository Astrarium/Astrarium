using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ADK.Demo.UI
{
    public partial class FormTimeInterval : Form
    {
        public FormTimeInterval(TimeInterval timeInterval)
        {
            InitializeComponent();

            cmbUnit.Items.AddRange(new string[]
            {
                "Second",
                "Minute",
                "Hour",
                "Day"
            });

            TimeInterval = timeInterval;
        }

        public TimeInterval TimeInterval
        {
            get
            {
                return new TimeInterval((double)updInterval.Value, (TimeIntervalUnit)cmbUnit.SelectedIndex);
            }
            set
            {
                cmbUnit.SelectedIndex = (int)(value.IntervalUnit);
                updInterval.Value = (decimal)(value.IntervalValue);
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }
    }
}
