using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ADK.Demo.UI
{
    public partial class TimeIntervalSelector : SelectorBase
    {
        public TimeIntervalSelector()
        {
            InitializeComponent();
            SelectorClicked += new EventHandler(TimeItervalSelector_SelectorClicked);
        }

        private void TimeItervalSelector_SelectorClicked(object sender, EventArgs e)
        {
            FormTimeInterval frmTimeInterval = new FormTimeInterval(TimeInterval);
            if (frmTimeInterval.ShowDialog() == DialogResult.OK)
            {
                TimeInterval = frmTimeInterval.TimeInterval;
            }
        }

        private TimeSpan _TimeInterval = TimeSpan.FromDays(1);
        [Browsable(false)]
        public TimeSpan TimeInterval
        {
            get
            {
                return _TimeInterval;
            }
            set
            {
                if (value != null)
                {
                    _TimeInterval = value;
                    lblText.Text = Text;
                }
            }
        }

        [Browsable(false)]
        public override string Text
        {
            get
            {
                if (DesignMode)
                {
                    return "1 d";
                }
                else
                {
                    return TimeInterval.ToString();
                }
            }
        }
    }
}
