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
            this.lblSecond = new System.Windows.Forms.Label();
            this.lblHour = new System.Windows.Forms.Label();
            this.lnkCurrentTime = new System.Windows.Forms.LinkLabel();
            this.panDay = new System.Windows.Forms.Panel();
            this.updownDay = new System.Windows.Forms.NumericUpDownEx();
            this.lblDay = new System.Windows.Forms.Label();
            this.panYear = new System.Windows.Forms.Panel();
            this.updownYear = new System.Windows.Forms.NumericUpDownEx();
            this.lblYear = new System.Windows.Forms.Label();
            this.panMonth = new System.Windows.Forms.Panel();
            this.cmbMonth = new System.Windows.Forms.ComboBox();
            this.lblMonth = new System.Windows.Forms.Label();
            this.panHour = new System.Windows.Forms.Panel();
            this.updownHour = new System.Windows.Forms.NumericUpDownEx();
            this.panMinute = new System.Windows.Forms.Panel();
            this.updownMinute = new System.Windows.Forms.NumericUpDownEx();
            this.lblMinute = new System.Windows.Forms.Label();
            this.panSecond = new System.Windows.Forms.Panel();
            this.updownSecond = new System.Windows.Forms.NumericUpDownEx();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.panTime = new System.Windows.Forms.Panel();
            this.panDate = new System.Windows.Forms.Panel();
            this.panButtons = new System.Windows.Forms.Panel();
            this.panDay.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.updownDay)).BeginInit();
            this.panYear.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.updownYear)).BeginInit();
            this.panMonth.SuspendLayout();
            this.panHour.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.updownHour)).BeginInit();
            this.panMinute.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.updownMinute)).BeginInit();
            this.panSecond.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.updownSecond)).BeginInit();
            this.panTime.SuspendLayout();
            this.panDate.SuspendLayout();
            this.panButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblSecond
            // 
            this.lblSecond.AutoSize = true;
            this.lblSecond.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblSecond.Location = new System.Drawing.Point(0, 0);
            this.lblSecond.Name = "lblSecond";
            this.lblSecond.Size = new System.Drawing.Size(49, 13);
            this.lblSecond.TabIndex = 39;
            this.lblSecond.Text = "Seconds";
            // 
            // lblHour
            // 
            this.lblHour.AutoSize = true;
            this.lblHour.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblHour.Location = new System.Drawing.Point(0, 0);
            this.lblHour.Name = "lblHour";
            this.lblHour.Size = new System.Drawing.Size(35, 13);
            this.lblHour.TabIndex = 37;
            this.lblHour.Text = "Hours";
            // 
            // lnkCurrentTime
            // 
            this.lnkCurrentTime.ActiveLinkColor = System.Drawing.SystemColors.ControlText;
            this.lnkCurrentTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lnkCurrentTime.AutoSize = true;
            this.lnkCurrentTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lnkCurrentTime.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.lnkCurrentTime.LinkColor = System.Drawing.Color.RoyalBlue;
            this.lnkCurrentTime.Location = new System.Drawing.Point(14, 18);
            this.lnkCurrentTime.Name = "lnkCurrentTime";
            this.lnkCurrentTime.Size = new System.Drawing.Size(59, 13);
            this.lnkCurrentTime.TabIndex = 6;
            this.lnkCurrentTime.TabStop = true;
            this.lnkCurrentTime.Text = "Set current";
            this.lnkCurrentTime.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkCurrentTime_LinkClicked);
            // 
            // panDay
            // 
            this.panDay.Controls.Add(this.updownDay);
            this.panDay.Controls.Add(this.lblDay);
            this.panDay.Location = new System.Drawing.Point(12, 8);
            this.panDay.Name = "panDay";
            this.panDay.Size = new System.Drawing.Size(75, 41);
            this.panDay.TabIndex = 39;
            // 
            // updownDay
            // 
            this.updownDay.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.updownDay.Format = "D2";
            this.updownDay.Location = new System.Drawing.Point(2, 18);
            this.updownDay.Margin = new System.Windows.Forms.Padding(3, 3, 3, 10);
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
            this.updownDay.Size = new System.Drawing.Size(70, 20);
            this.updownDay.TabIndex = 0;
            this.updownDay.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.updownDay.ValueChanged += new System.EventHandler(this.CalendarDate_Changed);
            // 
            // lblDay
            // 
            this.lblDay.AutoSize = true;
            this.lblDay.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblDay.Location = new System.Drawing.Point(0, 0);
            this.lblDay.Name = "lblDay";
            this.lblDay.Size = new System.Drawing.Size(26, 13);
            this.lblDay.TabIndex = 16;
            this.lblDay.Text = "Day";
            // 
            // panYear
            // 
            this.panYear.Controls.Add(this.updownYear);
            this.panYear.Controls.Add(this.lblYear);
            this.panYear.Location = new System.Drawing.Point(170, 8);
            this.panYear.Name = "panYear";
            this.panYear.Size = new System.Drawing.Size(75, 41);
            this.panYear.TabIndex = 40;
            // 
            // updownYear
            // 
            this.updownYear.Format = "D4";
            this.updownYear.Location = new System.Drawing.Point(2, 18);
            this.updownYear.Margin = new System.Windows.Forms.Padding(3, 3, 3, 10);
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
            this.updownYear.Size = new System.Drawing.Size(70, 20);
            this.updownYear.TabIndex = 2;
            this.updownYear.ValueChanged += new System.EventHandler(this.CalendarDate_Changed);
            // 
            // lblYear
            // 
            this.lblYear.AutoSize = true;
            this.lblYear.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblYear.Location = new System.Drawing.Point(0, 0);
            this.lblYear.Name = "lblYear";
            this.lblYear.Size = new System.Drawing.Size(29, 13);
            this.lblYear.TabIndex = 18;
            this.lblYear.Text = "Year";
            // 
            // panMonth
            // 
            this.panMonth.Controls.Add(this.cmbMonth);
            this.panMonth.Controls.Add(this.lblMonth);
            this.panMonth.Location = new System.Drawing.Point(91, 8);
            this.panMonth.Name = "panMonth";
            this.panMonth.Size = new System.Drawing.Size(75, 41);
            this.panMonth.TabIndex = 41;
            // 
            // cmbMonth
            // 
            this.cmbMonth.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbMonth.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbMonth.FormattingEnabled = true;
            this.cmbMonth.Location = new System.Drawing.Point(2, 18);
            this.cmbMonth.Margin = new System.Windows.Forms.Padding(3, 3, 3, 10);
            this.cmbMonth.MaxDropDownItems = 12;
            this.cmbMonth.Name = "cmbMonth";
            this.cmbMonth.Size = new System.Drawing.Size(70, 21);
            this.cmbMonth.TabIndex = 1;
            this.cmbMonth.SelectedIndexChanged += new System.EventHandler(this.CalendarDate_Changed);
            // 
            // lblMonth
            // 
            this.lblMonth.AutoSize = true;
            this.lblMonth.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblMonth.Location = new System.Drawing.Point(0, 0);
            this.lblMonth.Name = "lblMonth";
            this.lblMonth.Size = new System.Drawing.Size(37, 13);
            this.lblMonth.TabIndex = 20;
            this.lblMonth.Text = "Month";
            // 
            // panHour
            // 
            this.panHour.Controls.Add(this.updownHour);
            this.panHour.Controls.Add(this.lblHour);
            this.panHour.Location = new System.Drawing.Point(12, 6);
            this.panHour.Name = "panHour";
            this.panHour.Size = new System.Drawing.Size(75, 41);
            this.panHour.TabIndex = 42;
            // 
            // updownHour
            // 
            this.updownHour.Format = "D2";
            this.updownHour.Location = new System.Drawing.Point(2, 18);
            this.updownHour.Maximum = new decimal(new int[] {
            23,
            0,
            0,
            0});
            this.updownHour.Name = "updownHour";
            this.updownHour.Sign = false;
            this.updownHour.Size = new System.Drawing.Size(70, 20);
            this.updownHour.TabIndex = 3;
            this.updownHour.ValueChanged += new System.EventHandler(this.CalendarDate_Changed);
            // 
            // panMinute
            // 
            this.panMinute.Controls.Add(this.updownMinute);
            this.panMinute.Controls.Add(this.lblMinute);
            this.panMinute.Location = new System.Drawing.Point(91, 6);
            this.panMinute.Name = "panMinute";
            this.panMinute.Size = new System.Drawing.Size(75, 41);
            this.panMinute.TabIndex = 43;
            // 
            // updownMinute
            // 
            this.updownMinute.Format = "D2";
            this.updownMinute.Location = new System.Drawing.Point(2, 18);
            this.updownMinute.Maximum = new decimal(new int[] {
            59,
            0,
            0,
            0});
            this.updownMinute.Name = "updownMinute";
            this.updownMinute.Sign = false;
            this.updownMinute.Size = new System.Drawing.Size(70, 20);
            this.updownMinute.TabIndex = 4;
            this.updownMinute.ValueChanged += new System.EventHandler(this.CalendarDate_Changed);
            // 
            // lblMinute
            // 
            this.lblMinute.AutoSize = true;
            this.lblMinute.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblMinute.Location = new System.Drawing.Point(0, 0);
            this.lblMinute.Name = "lblMinute";
            this.lblMinute.Size = new System.Drawing.Size(44, 13);
            this.lblMinute.TabIndex = 39;
            this.lblMinute.Text = "Minutes";
            // 
            // panSecond
            // 
            this.panSecond.Controls.Add(this.updownSecond);
            this.panSecond.Controls.Add(this.lblSecond);
            this.panSecond.Location = new System.Drawing.Point(170, 6);
            this.panSecond.Name = "panSecond";
            this.panSecond.Size = new System.Drawing.Size(75, 41);
            this.panSecond.TabIndex = 44;
            // 
            // updownSecond
            // 
            this.updownSecond.Format = "D2";
            this.updownSecond.Location = new System.Drawing.Point(2, 18);
            this.updownSecond.Maximum = new decimal(new int[] {
            59,
            0,
            0,
            0});
            this.updownSecond.Name = "updownSecond";
            this.updownSecond.Sign = false;
            this.updownSecond.Size = new System.Drawing.Size(70, 20);
            this.updownSecond.TabIndex = 5;
            this.updownSecond.ValueChanged += new System.EventHandler(this.CalendarDate_Changed);
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.Location = new System.Drawing.Point(95, 13);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(72, 23);
            this.btnOK.TabIndex = 7;
            this.btnOK.Text = "&OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(174, 13);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(72, 23);
            this.btnCancel.TabIndex = 8;
            this.btnCancel.Text = "&Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // panTime
            // 
            this.panTime.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panTime.Controls.Add(this.panHour);
            this.panTime.Controls.Add(this.panSecond);
            this.panTime.Controls.Add(this.panMinute);
            this.panTime.Location = new System.Drawing.Point(0, 55);
            this.panTime.Name = "panTime";
            this.panTime.Size = new System.Drawing.Size(258, 55);
            this.panTime.TabIndex = 47;
            // 
            // panDate
            // 
            this.panDate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panDate.Controls.Add(this.panDay);
            this.panDate.Controls.Add(this.panYear);
            this.panDate.Controls.Add(this.panMonth);
            this.panDate.Location = new System.Drawing.Point(0, 0);
            this.panDate.Name = "panDate";
            this.panDate.Size = new System.Drawing.Size(258, 52);
            this.panDate.TabIndex = 48;
            // 
            // panButtons
            // 
            this.panButtons.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panButtons.Controls.Add(this.btnCancel);
            this.panButtons.Controls.Add(this.lnkCurrentTime);
            this.panButtons.Controls.Add(this.btnOK);
            this.panButtons.Location = new System.Drawing.Point(0, 108);
            this.panButtons.Name = "panButtons";
            this.panButtons.Size = new System.Drawing.Size(258, 46);
            this.panButtons.TabIndex = 49;
            // 
            // FormDateTime
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(259, 154);
            this.Controls.Add(this.panButtons);
            this.Controls.Add(this.panTime);
            this.Controls.Add(this.panDate);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormDateTime";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Date and Time";
            this.panDay.ResumeLayout(false);
            this.panDay.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.updownDay)).EndInit();
            this.panYear.ResumeLayout(false);
            this.panYear.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.updownYear)).EndInit();
            this.panMonth.ResumeLayout(false);
            this.panMonth.PerformLayout();
            this.panHour.ResumeLayout(false);
            this.panHour.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.updownHour)).EndInit();
            this.panMinute.ResumeLayout(false);
            this.panMinute.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.updownMinute)).EndInit();
            this.panSecond.ResumeLayout(false);
            this.panSecond.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.updownSecond)).EndInit();
            this.panTime.ResumeLayout(false);
            this.panDate.ResumeLayout(false);
            this.panButtons.ResumeLayout(false);
            this.panButtons.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private Label lblSecond;
        private Label lblHour;
        private LinkLabel lnkCurrentTime;
        private Panel panDay;
        private NumericUpDownEx updownDay;
        private Label lblDay;
        private Panel panYear;
        private NumericUpDownEx updownYear;
        private Label lblYear;
        private Panel panMonth;
        private ComboBox cmbMonth;
        private Label lblMonth;
        private Panel panHour;
        private NumericUpDownEx updownHour;
        private Panel panMinute;
        private NumericUpDownEx updownMinute;
        private Label lblMinute;
        private Panel panSecond;
        private NumericUpDownEx updownSecond;
        private Button btnOK;
        private Button btnCancel;
        private Panel panTime;
        private Panel panDate;
        private Panel panButtons;
    }
}