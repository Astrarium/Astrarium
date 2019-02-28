using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ADK.Demo.UI
{
    partial class FormDateTime : Form
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.panControls = new System.Windows.Forms.Panel();
            this.lnkCurrentTime = new System.Windows.Forms.LinkLabel();
            this.panTime = new System.Windows.Forms.Panel();
            this.lblSecond = new System.Windows.Forms.Label();
            this.lblMinute = new System.Windows.Forms.Label();
            this.lblHour = new System.Windows.Forms.Label();
            this.panDate = new System.Windows.Forms.Panel();
            this.panMonthYear = new System.Windows.Forms.Panel();
            this.lblMonth = new System.Windows.Forms.Label();
            this.lblYear = new System.Windows.Forms.Label();
            this.cmbMonth = new System.Windows.Forms.ComboBox();
            this.panDay = new System.Windows.Forms.Panel();
            this.lblDay = new System.Windows.Forms.Label();
            this.panButtons = new System.Windows.Forms.Panel();
            this.updownHour = new System.Windows.Forms.NumericUpDownEx();
            this.updownMinute = new System.Windows.Forms.NumericUpDownEx();
            this.updownSecond = new System.Windows.Forms.NumericUpDownEx();
            this.updownYear = new System.Windows.Forms.NumericUpDownEx();
            this.updownDay = new System.Windows.Forms.NumericUpDownEx();
            this.panControls.SuspendLayout();
            this.panTime.SuspendLayout();
            this.panDate.SuspendLayout();
            this.panMonthYear.SuspendLayout();
            this.panDay.SuspendLayout();
            this.panButtons.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.updownHour)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.updownMinute)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.updownSecond)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.updownYear)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.updownDay)).BeginInit();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(60, 9);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(71, 23);
            this.btnCancel.TabIndex = 32;
            this.btnCancel.Text = "&Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.Location = new System.Drawing.Point(137, 9);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(71, 23);
            this.btnOK.TabIndex = 31;
            this.btnOK.Text = "&OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // panControls
            // 
            this.panControls.AutoSize = true;
            this.panControls.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panControls.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panControls.Controls.Add(this.panButtons);
            this.panControls.Controls.Add(this.lnkCurrentTime);
            this.panControls.Controls.Add(this.panTime);
            this.panControls.Controls.Add(this.panDate);
            this.panControls.Dock = System.Windows.Forms.DockStyle.Top;
            this.panControls.Location = new System.Drawing.Point(0, 0);
            this.panControls.Name = "panControls";
            this.panControls.Size = new System.Drawing.Size(218, 156);
            this.panControls.TabIndex = 37;
            // 
            // lnkCurrentTime
            // 
            this.lnkCurrentTime.ActiveLinkColor = System.Drawing.SystemColors.ControlText;
            this.lnkCurrentTime.Dock = System.Windows.Forms.DockStyle.Top;
            this.lnkCurrentTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lnkCurrentTime.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.lnkCurrentTime.LinkColor = System.Drawing.Color.RoyalBlue;
            this.lnkCurrentTime.Location = new System.Drawing.Point(0, 89);
            this.lnkCurrentTime.Name = "lnkCurrentTime";
            this.lnkCurrentTime.Padding = new System.Windows.Forms.Padding(4);
            this.lnkCurrentTime.Size = new System.Drawing.Size(216, 23);
            this.lnkCurrentTime.TabIndex = 37;
            this.lnkCurrentTime.TabStop = true;
            this.lnkCurrentTime.Text = "Set current";
            // 
            // panTime
            // 
            this.panTime.AutoSize = true;
            this.panTime.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panTime.Controls.Add(this.lblSecond);
            this.panTime.Controls.Add(this.lblMinute);
            this.panTime.Controls.Add(this.lblHour);
            this.panTime.Controls.Add(this.updownHour);
            this.panTime.Controls.Add(this.updownMinute);
            this.panTime.Controls.Add(this.updownSecond);
            this.panTime.Dock = System.Windows.Forms.DockStyle.Top;
            this.panTime.Location = new System.Drawing.Point(0, 44);
            this.panTime.Name = "panTime";
            this.panTime.Size = new System.Drawing.Size(216, 45);
            this.panTime.TabIndex = 38;
            // 
            // lblSecond
            // 
            this.lblSecond.AutoSize = true;
            this.lblSecond.Location = new System.Drawing.Point(143, 6);
            this.lblSecond.Name = "lblSecond";
            this.lblSecond.Size = new System.Drawing.Size(49, 13);
            this.lblSecond.TabIndex = 39;
            this.lblSecond.Text = "Seconds";
            // 
            // lblMinute
            // 
            this.lblMinute.AutoSize = true;
            this.lblMinute.Location = new System.Drawing.Point(75, 6);
            this.lblMinute.Name = "lblMinute";
            this.lblMinute.Size = new System.Drawing.Size(44, 13);
            this.lblMinute.TabIndex = 38;
            this.lblMinute.Text = "Minutes";
            // 
            // lblHour
            // 
            this.lblHour.AutoSize = true;
            this.lblHour.Location = new System.Drawing.Point(5, 6);
            this.lblHour.Name = "lblHour";
            this.lblHour.Size = new System.Drawing.Size(35, 13);
            this.lblHour.TabIndex = 37;
            this.lblHour.Text = "Hours";
            // 
            // panDate
            // 
            this.panDate.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panDate.Controls.Add(this.panMonthYear);
            this.panDate.Controls.Add(this.panDay);
            this.panDate.Dock = System.Windows.Forms.DockStyle.Top;
            this.panDate.Location = new System.Drawing.Point(0, 0);
            this.panDate.Name = "panDate";
            this.panDate.Size = new System.Drawing.Size(216, 44);
            this.panDate.TabIndex = 39;
            // 
            // panMonthYear
            // 
            this.panMonthYear.AutoSize = true;
            this.panMonthYear.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panMonthYear.Controls.Add(this.lblMonth);
            this.panMonthYear.Controls.Add(this.updownYear);
            this.panMonthYear.Controls.Add(this.lblYear);
            this.panMonthYear.Controls.Add(this.cmbMonth);
            this.panMonthYear.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panMonthYear.Location = new System.Drawing.Point(71, 0);
            this.panMonthYear.Name = "panMonthYear";
            this.panMonthYear.Size = new System.Drawing.Size(145, 44);
            this.panMonthYear.TabIndex = 37;
            // 
            // lblMonth
            // 
            this.lblMonth.AutoSize = true;
            this.lblMonth.Location = new System.Drawing.Point(6, 5);
            this.lblMonth.Name = "lblMonth";
            this.lblMonth.Size = new System.Drawing.Size(37, 13);
            this.lblMonth.TabIndex = 16;
            this.lblMonth.Text = "Month";
            // 
            // lblYear
            // 
            this.lblYear.AutoSize = true;
            this.lblYear.Location = new System.Drawing.Point(72, 5);
            this.lblYear.Name = "lblYear";
            this.lblYear.Size = new System.Drawing.Size(29, 13);
            this.lblYear.TabIndex = 17;
            this.lblYear.Text = "Year";
            // 
            // cmbMonth
            // 
            this.cmbMonth.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbMonth.FormattingEnabled = true;
            this.cmbMonth.Location = new System.Drawing.Point(7, 20);
            this.cmbMonth.Name = "cmbMonth";
            this.cmbMonth.Size = new System.Drawing.Size(61, 21);
            this.cmbMonth.TabIndex = 27;
            // 
            // panDay
            // 
            this.panDay.AutoSize = true;
            this.panDay.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panDay.Controls.Add(this.lblDay);
            this.panDay.Controls.Add(this.updownDay);
            this.panDay.Dock = System.Windows.Forms.DockStyle.Left;
            this.panDay.Location = new System.Drawing.Point(0, 0);
            this.panDay.Name = "panDay";
            this.panDay.Size = new System.Drawing.Size(71, 44);
            this.panDay.TabIndex = 37;
            // 
            // lblDay
            // 
            this.lblDay.AutoSize = true;
            this.lblDay.Location = new System.Drawing.Point(3, 5);
            this.lblDay.Name = "lblDay";
            this.lblDay.Size = new System.Drawing.Size(26, 13);
            this.lblDay.TabIndex = 15;
            this.lblDay.Text = "Day";
            // 
            // panButtons
            // 
            this.panButtons.Controls.Add(this.btnOK);
            this.panButtons.Controls.Add(this.btnCancel);
            this.panButtons.Dock = System.Windows.Forms.DockStyle.Top;
            this.panButtons.Location = new System.Drawing.Point(0, 112);
            this.panButtons.Name = "panButtons";
            this.panButtons.Size = new System.Drawing.Size(216, 42);
            this.panButtons.TabIndex = 40;
            // 
            // updownHour
            // 
            this.updownHour.Format = "D2";
            this.updownHour.Location = new System.Drawing.Point(8, 22);
            this.updownHour.Maximum = new decimal(new int[] {
            23,
            0,
            0,
            0});
            this.updownHour.Name = "updownHour";
            this.updownHour.Sign = false;
            this.updownHour.Size = new System.Drawing.Size(62, 20);
            this.updownHour.TabIndex = 34;
            // 
            // updownMinute
            // 
            this.updownMinute.Format = "D2";
            this.updownMinute.Location = new System.Drawing.Point(76, 22);
            this.updownMinute.Maximum = new decimal(new int[] {
            59,
            0,
            0,
            0});
            this.updownMinute.Name = "updownMinute";
            this.updownMinute.Sign = false;
            this.updownMinute.Size = new System.Drawing.Size(63, 20);
            this.updownMinute.TabIndex = 35;
            // 
            // updownSecond
            // 
            this.updownSecond.Format = "D2";
            this.updownSecond.Location = new System.Drawing.Point(146, 22);
            this.updownSecond.Maximum = new decimal(new int[] {
            59,
            0,
            0,
            0});
            this.updownSecond.Name = "updownSecond";
            this.updownSecond.Sign = false;
            this.updownSecond.Size = new System.Drawing.Size(62, 20);
            this.updownSecond.TabIndex = 36;
            // 
            // updownYear
            // 
            this.updownYear.Format = "D4";
            this.updownYear.Location = new System.Drawing.Point(75, 21);
            this.updownYear.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.updownYear.Minimum = new decimal(new int[] {
            3000,
            0,
            0,
            -2147483648});
            this.updownYear.Name = "updownYear";
            this.updownYear.Sign = false;
            this.updownYear.Size = new System.Drawing.Size(62, 20);
            this.updownYear.TabIndex = 10;
            // 
            // updownDay
            // 
            this.updownDay.Format = "D2";
            this.updownDay.Location = new System.Drawing.Point(6, 21);
            this.updownDay.Maximum = new decimal(new int[] {
            31,
            0,
            0,
            0});
            this.updownDay.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.updownDay.Name = "updownDay";
            this.updownDay.Sign = false;
            this.updownDay.Size = new System.Drawing.Size(62, 20);
            this.updownDay.TabIndex = 8;
            this.updownDay.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // FormDateTime
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(218, 156);
            this.Controls.Add(this.panControls);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormDateTime";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Date and Time";
            this.panControls.ResumeLayout(false);
            this.panControls.PerformLayout();
            this.panTime.ResumeLayout(false);
            this.panTime.PerformLayout();
            this.panDate.ResumeLayout(false);
            this.panDate.PerformLayout();
            this.panMonthYear.ResumeLayout(false);
            this.panMonthYear.PerformLayout();
            this.panDay.ResumeLayout(false);
            this.panDay.PerformLayout();
            this.panButtons.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.updownHour)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.updownMinute)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.updownSecond)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.updownYear)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.updownDay)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private Panel panControls;
        private LinkLabel lnkCurrentTime;
        private Panel panTime;
        private Label lblSecond;
        private Label lblMinute;
        private Label lblHour;
        private NumericUpDownEx updownHour;
        private NumericUpDownEx updownMinute;
        private NumericUpDownEx updownSecond;
        private Panel panDate;
        private Panel panMonthYear;
        private Label lblMonth;
        private NumericUpDownEx updownYear;
        private Label lblYear;
        private ComboBox cmbMonth;
        private Panel panDay;
        private Label lblDay;
        private NumericUpDownEx updownDay;
        private Panel panButtons;
    }
}