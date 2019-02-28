using ADK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ADK.Demo.UI
{
    /// <summary>
    /// Options for FormDateTime dialog
    /// </summary>
    public enum FormDateTimeOptions
    {
        All,
        DateOnly,
        MonthAndYearOnly
    }

    /// <summary>
    /// Dialog window for Date & Time selection.
    /// </summary>
    public partial class FormDateTime : Form
    {
        /// <summary>
        /// Gets selected value of julian day
        /// </summary>
        public double JulianDay { get; private set; }

        public double UtcOffset { get; private set; }

        /// <summary>
        /// Initializes new instance of the dialog.
        /// </summary>
        /// <param name="d">Selected date and time.</param>
        /// <param name="options">Options for FormDateTime dialog.</param>
        public FormDateTime(double jd, double utcOffset, FormDateTimeOptions options = FormDateTimeOptions.All)
        {
            InitializeComponent();

            if (options == FormDateTimeOptions.DateOnly ||
                options == FormDateTimeOptions.MonthAndYearOnly)
            {
                panControls.Controls.Remove(panTime);
                
                if (options == FormDateTimeOptions.MonthAndYearOnly)
                {
                    panDate.Controls.Remove(panDay);                 
                }
            }

            ClientSize = panControls.Size;

            cmbMonth.Items.Clear();
            cmbMonth.Items.AddRange(CultureInfo.InvariantCulture.DateTimeFormat.AbbreviatedMonthNames.Take(12).ToArray());
            cmbMonth.SelectedIndex = 0;

            UtcOffset = utcOffset;
            JulianDay = jd;

            SetDate();
        }

        private void SetDate()
        {
            Date d = new Date(JulianDay, UtcOffset);

            cmbMonth.SelectedIndex = d.Month - 1;
            updownDay.Value = (int)d.Day;
            updownYear.Value = d.Year;
            updownHour.Value = d.Hour;
            updownMinute.Value = d.Minute;
            updownSecond.Value = d.Second;
        }

        private void lnkCurrentTime_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            DateTime d = DateTime.Now;

            cmbMonth.SelectedIndex = d.Month - 1;
            updownDay.Value = d.Day;
            updownYear.Value = d.Year;
            updownHour.Value = d.Hour;
            updownMinute.Value = d.Minute;
            updownSecond.Value = d.Second;
        }

        private void CalendarDate_Changed(object sender, EventArgs e)
        {
            SetMaxDay();

            int year = Convert.ToInt32(updownYear.Value);
            int month = Convert.ToInt32(cmbMonth.SelectedIndex + 1);
            int day = Convert.ToInt32(updownDay.Value);
            int hour = Convert.ToInt32(updownHour.Value);
            int minute = Convert.ToInt32(updownMinute.Value);
            int second = Convert.ToInt32(updownSecond.Value);

            Date date = new Date(year, month, day + TimeSpan.FromHours(hour).TotalDays + TimeSpan.FromMinutes(minute).TotalDays + TimeSpan.FromSeconds(second).TotalDays);
            
            JulianDay = Date.JulianDay(date, UtcOffset);
        }

        private void SetMaxDay()
        {
            updownDay.Maximum = Date.DaysInMonth((int)updownYear.Value, cmbMonth.SelectedIndex + 1);
        }

        public DialogResult ShowDropDown(Control c)
        {
            Form parent = c.FindForm();
            parent.Activated += new EventHandler(parent_Activated);
            MouseDown += new MouseEventHandler(parent_OnMouseDown);

            Text = "";
            ControlBox = false;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowIcon = false;
            ShowInTaskbar = false;
            SizeGripStyle = SizeGripStyle.Show;
            TopMost = true;
            Capture = true;
            StartPosition = FormStartPosition.WindowsDefaultLocation;

            Point location = parent.PointToScreen(new Point(c.Left, c.Bottom));
            Left = location.X;
            Top = location.Y;

            return ShowDialog();
        }

        private void parent_OnMouseDown(object sender, MouseEventArgs e)
        {
            if (!RectangleToScreen(this.ClientRectangle).Contains(Cursor.Position))
            {
                Close();
            }
        }

        private void parent_Activated(object sender, EventArgs e)
        {
            Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}