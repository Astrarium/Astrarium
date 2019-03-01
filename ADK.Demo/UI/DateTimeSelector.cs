using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;
using System.Drawing;

namespace ADK.Demo.UI
{
    public partial class DateTimeSelector : SelectorBase
    {
        public DateTimeSelector()
        {
            InitializeComponent();
            SelectorClicked += new EventHandler(DateTimeSelector_SelectorClicked);
            JulianDay = new Date(DateTime.Now).ToJulianEphemerisDay();
            UtcOffset = TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow).TotalHours;
        }

        private void ShowDateTimeForm()
        {
            FormDateTime frmDateTime = new FormDateTime(JulianDay, UtcOffset, DateFormat);
            if (frmDateTime.ShowDialog(this) == DialogResult.OK)
            {
                JulianDay = frmDateTime.JulianDay;
            }
        }

        [Browsable(false)]
        public override string Text
        {
            get
            {
                switch (DateFormat)
                {
                    default:
                    case DateOptions.DateTime:
                        return Formatters.DateTime.Format(new Date(JulianDay, UtcOffset));
                    case DateOptions.DateOnly:
                        return Formatters.DateOnly.Format(new Date(JulianDay, UtcOffset));
                    case DateOptions.MonthYear:
                        return Formatters.MonthYear.Format(new Date(JulianDay, UtcOffset));
                }
            }
        }

        private double _JulianDay;

        [Browsable(false)]
        public double JulianDay
        {
            get
            {
                return _JulianDay;
            }
            set
            {
                if (value > 0 && value != _JulianDay)
                {
                    _JulianDay = value;
                    if (OnDateChanged != null) OnDateChanged.Invoke(this, new EventArgs());
                    lblText.Text = Text;
                }
            }
        }

        private double _UtcOffset;

        [Browsable(false)]
        public double UtcOffset
        {
            get
            {
                return _UtcOffset;
            }
            set
            {
                _UtcOffset = value;
                if (OnDateChanged != null) OnDateChanged.Invoke(this, new EventArgs());
                lblText.Text = Text;
            }
        }

        private DateOptions _DateFormat;
        public DateOptions DateFormat
        {
            get { return _DateFormat; }
            set
            {
                _DateFormat = value;
                lblText.Text = Text;
            }
        }

        public event EventHandler OnDateChanged;

        private void DateTimeSelector_SelectorClicked(object sender, EventArgs e)
        {
            ShowDateTimeForm();
        }
    }
}
