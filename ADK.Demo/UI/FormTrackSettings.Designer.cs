namespace ADK.Demo.UI
{
    partial class FormTrackSettings
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
            this.lblFrom = new System.Windows.Forms.Label();
            this.lblEndDate = new System.Windows.Forms.Label();
            this.lblBody = new System.Windows.Forms.Label();
            this.lblStep = new System.Windows.Forms.Label();
            this.dtTo = new ADK.Demo.UI.DateTimeSelector();
            this.dtFrom = new ADK.Demo.UI.DateTimeSelector();
            this.selCelestialBody = new ADK.Demo.UI.CelestialObjectSelector();
            this.selTimeInterval = new ADK.Demo.UI.TimeIntervalSelector();
            this.grpLabels = new System.Windows.Forms.GroupBox();
            this.chkLabels = new System.Windows.Forms.CheckBox();
            this.panButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.grpLabels.SuspendLayout();
            this.panButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.AutoSize = true;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(437, 3);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(72, 23);
            this.btnCancel.TabIndex = 10;
            this.btnCancel.Text = "&Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(359, 3);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(72, 23);
            this.btnOK.TabIndex = 9;
            this.btnOK.Text = "&OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // lblFrom
            // 
            this.lblFrom.AutoSize = true;
            this.lblFrom.Location = new System.Drawing.Point(19, 50);
            this.lblFrom.Name = "lblFrom";
            this.lblFrom.Size = new System.Drawing.Size(53, 13);
            this.lblFrom.TabIndex = 15;
            this.lblFrom.Text = "Start date";
            // 
            // lblEndDate
            // 
            this.lblEndDate.AutoSize = true;
            this.lblEndDate.Location = new System.Drawing.Point(19, 82);
            this.lblEndDate.Name = "lblEndDate";
            this.lblEndDate.Size = new System.Drawing.Size(50, 13);
            this.lblEndDate.TabIndex = 16;
            this.lblEndDate.Text = "End date";
            // 
            // lblBody
            // 
            this.lblBody.AutoSize = true;
            this.lblBody.Location = new System.Drawing.Point(19, 19);
            this.lblBody.Name = "lblBody";
            this.lblBody.Size = new System.Drawing.Size(72, 13);
            this.lblBody.TabIndex = 17;
            this.lblBody.Text = "Celestial body";
            // 
            // lblStep
            // 
            this.lblStep.AutoSize = true;
            this.lblStep.Location = new System.Drawing.Point(6, 25);
            this.lblStep.Name = "lblStep";
            this.lblStep.Size = new System.Drawing.Size(29, 13);
            this.lblStep.TabIndex = 18;
            this.lblStep.Text = "Step";
            // 
            // dtTo
            // 
            this.dtTo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.dtTo.DateFormat = ADK.Demo.UI.DateOptions.DateTime;
            this.dtTo.JulianDay = 2458544.0400693328D;
            this.dtTo.Location = new System.Drawing.Point(355, 77);
            this.dtTo.Name = "dtTo";
            this.dtTo.Size = new System.Drawing.Size(139, 22);
            this.dtTo.TabIndex = 13;
            this.dtTo.UtcOffset = 0D;
            // 
            // dtFrom
            // 
            this.dtFrom.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.dtFrom.DateFormat = ADK.Demo.UI.DateOptions.DateTime;
            this.dtFrom.JulianDay = 2458544.0400689626D;
            this.dtFrom.Location = new System.Drawing.Point(355, 45);
            this.dtFrom.Name = "dtFrom";
            this.dtFrom.Size = new System.Drawing.Size(139, 22);
            this.dtFrom.TabIndex = 12;
            this.dtFrom.UtcOffset = 0D;
            // 
            // selCelestialBody
            // 
            this.selCelestialBody.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.selCelestialBody.Location = new System.Drawing.Point(355, 13);
            this.selCelestialBody.Name = "selCelestialBody";
            this.selCelestialBody.Searcher = null;
            this.selCelestialBody.SelectedObject = null;
            this.selCelestialBody.Size = new System.Drawing.Size(139, 22);
            this.selCelestialBody.TabIndex = 19;
            // 
            // selTimeInterval
            // 
            this.selTimeInterval.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.selTimeInterval.Location = new System.Drawing.Point(343, 21);
            this.selTimeInterval.Name = "selTimeInterval";
            this.selTimeInterval.Size = new System.Drawing.Size(139, 22);
            this.selTimeInterval.TabIndex = 20;
            // 
            // grpLabels
            // 
            this.grpLabels.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpLabels.Controls.Add(this.chkLabels);
            this.grpLabels.Controls.Add(this.lblStep);
            this.grpLabels.Controls.Add(this.selTimeInterval);
            this.grpLabels.Location = new System.Drawing.Point(12, 105);
            this.grpLabels.Name = "grpLabels";
            this.grpLabels.Size = new System.Drawing.Size(488, 60);
            this.grpLabels.TabIndex = 21;
            this.grpLabels.TabStop = false;
            // 
            // chkLabels
            // 
            this.chkLabels.AutoSize = true;
            this.chkLabels.Location = new System.Drawing.Point(10, 0);
            this.chkLabels.Name = "chkLabels";
            this.chkLabels.Size = new System.Drawing.Size(57, 17);
            this.chkLabels.TabIndex = 21;
            this.chkLabels.Text = "Labels";
            this.chkLabels.UseVisualStyleBackColor = true;
            // 
            // panButtons
            // 
            this.panButtons.Controls.Add(this.btnCancel);
            this.panButtons.Controls.Add(this.btnOK);
            this.panButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.panButtons.Location = new System.Drawing.Point(0, 268);
            this.panButtons.Name = "panButtons";
            this.panButtons.Size = new System.Drawing.Size(512, 30);
            this.panButtons.TabIndex = 22;
            // 
            // FormTrackSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(512, 298);
            this.Controls.Add(this.panButtons);
            this.Controls.Add(this.grpLabels);
            this.Controls.Add(this.selCelestialBody);
            this.Controls.Add(this.lblBody);
            this.Controls.Add(this.lblEndDate);
            this.Controls.Add(this.lblFrom);
            this.Controls.Add(this.dtTo);
            this.Controls.Add(this.dtFrom);
            this.MinimumSize = new System.Drawing.Size(400, 230);
            this.Name = "FormTrackSettings";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Track Properties";
            this.grpLabels.ResumeLayout(false);
            this.grpLabels.PerformLayout();
            this.panButtons.ResumeLayout(false);
            this.panButtons.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private DateTimeSelector dtFrom;
        private DateTimeSelector dtTo;
        private System.Windows.Forms.Label lblFrom;
        private System.Windows.Forms.Label lblEndDate;
        private System.Windows.Forms.Label lblBody;
        private System.Windows.Forms.Label lblStep;
        private CelestialObjectSelector selCelestialBody;
        private TimeIntervalSelector selTimeInterval;
        private System.Windows.Forms.GroupBox grpLabels;
        private System.Windows.Forms.CheckBox chkLabels;
        private System.Windows.Forms.FlowLayoutPanel panButtons;
    }
}