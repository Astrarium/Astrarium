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
        public FormTimeInterval(TimeSpan timeInterval)
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

        public TimeSpan TimeInterval
        {
            get
            {
                var value = (double)updInterval.Value;
                switch (cmbUnit.SelectedIndex)
                {
                    case 0:
                        return TimeSpan.FromSeconds(value);
                    case 1:
                        return TimeSpan.FromMinutes(value);
                    case 2:
                        return TimeSpan.FromHours(value);
                    default:
                        return TimeSpan.FromDays(value);
                }
            }
            set
            {
                if (value.TotalDays >= 1)
                {
                    cmbUnit.SelectedIndex = 3;
                    updInterval.Value = (int)value.TotalDays;
                }
                else if (value.TotalHours >= 1)
                {
                    cmbUnit.SelectedIndex = 2;
                    updInterval.Value = (int)value.TotalHours;
                }
                else if (value.TotalMinutes >= 1)
                {
                    cmbUnit.SelectedIndex = 1;
                    updInterval.Value = (int)value.TotalMinutes;
                }
                else if (value.TotalSeconds >= 1)
                {
                    cmbUnit.SelectedIndex = 0;
                    updInterval.Value = (int)value.TotalSeconds;
                }
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }
    }
}
