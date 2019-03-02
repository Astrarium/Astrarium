namespace ADK.Demo.UI
{
    partial class FormEphemerisSettings
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
            this.lstCategories = new ADK.Demo.UI.TreeViewEx();
            this.dtTo = new ADK.Demo.UI.DateTimeSelector();
            this.dtFrom = new ADK.Demo.UI.DateTimeSelector();
            this.lblFrom = new System.Windows.Forms.Label();
            this.lblEndDate = new System.Windows.Forms.Label();
            this.lblBody = new System.Windows.Forms.Label();
            this.lblStep = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(400, 427);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(72, 23);
            this.btnCancel.TabIndex = 10;
            this.btnCancel.Text = "&Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.Enabled = false;
            this.btnOK.Location = new System.Drawing.Point(321, 427);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(72, 23);
            this.btnOK.TabIndex = 9;
            this.btnOK.Text = "&OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // lstCategories
            // 
            this.lstCategories.AllowCollapse = false;
            this.lstCategories.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstCategories.CheckBoxes = true;
            this.lstCategories.FullRowSelect = true;
            this.lstCategories.Location = new System.Drawing.Point(13, 141);
            this.lstCategories.Name = "lstCategories";
            this.lstCategories.ShowLines = false;
            this.lstCategories.ShowPlusMinus = false;
            this.lstCategories.ShowRootLines = false;
            this.lstCategories.Size = new System.Drawing.Size(459, 275);
            this.lstCategories.TabIndex = 14;
            this.lstCategories.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.lstCategories_AfterCheck);
            // 
            // dtTo
            // 
            this.dtTo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.dtTo.DateFormat = ADK.Demo.UI.DateOptions.DateTime;
            this.dtTo.JulianDay = 2458544.0400693328D;
            this.dtTo.Location = new System.Drawing.Point(333, 77);
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
            this.dtFrom.Location = new System.Drawing.Point(333, 45);
            this.dtFrom.Name = "dtFrom";
            this.dtFrom.Size = new System.Drawing.Size(139, 22);
            this.dtFrom.TabIndex = 12;
            this.dtFrom.UtcOffset = 0D;
            // 
            // lblFrom
            // 
            this.lblFrom.AutoSize = true;
            this.lblFrom.Location = new System.Drawing.Point(12, 50);
            this.lblFrom.Name = "lblFrom";
            this.lblFrom.Size = new System.Drawing.Size(53, 13);
            this.lblFrom.TabIndex = 15;
            this.lblFrom.Text = "Start date";
            // 
            // lblEndDate
            // 
            this.lblEndDate.AutoSize = true;
            this.lblEndDate.Location = new System.Drawing.Point(12, 82);
            this.lblEndDate.Name = "lblEndDate";
            this.lblEndDate.Size = new System.Drawing.Size(50, 13);
            this.lblEndDate.TabIndex = 16;
            this.lblEndDate.Text = "End date";
            // 
            // lblBody
            // 
            this.lblBody.AutoSize = true;
            this.lblBody.Location = new System.Drawing.Point(12, 19);
            this.lblBody.Name = "lblBody";
            this.lblBody.Size = new System.Drawing.Size(72, 13);
            this.lblBody.TabIndex = 17;
            this.lblBody.Text = "Celestial body";
            // 
            // lblStep
            // 
            this.lblStep.AutoSize = true;
            this.lblStep.Location = new System.Drawing.Point(12, 114);
            this.lblStep.Name = "lblStep";
            this.lblStep.Size = new System.Drawing.Size(29, 13);
            this.lblStep.TabIndex = 18;
            this.lblStep.Text = "Step";
            // 
            // FormEphemerisSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 462);
            this.Controls.Add(this.lblStep);
            this.Controls.Add(this.lblBody);
            this.Controls.Add(this.lblEndDate);
            this.Controls.Add(this.lblFrom);
            this.Controls.Add(this.lstCategories);
            this.Controls.Add(this.dtTo);
            this.Controls.Add(this.dtFrom);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.MinimumSize = new System.Drawing.Size(400, 400);
            this.Name = "FormEphemerisSettings";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Celestial Body Ephemeris";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private DateTimeSelector dtFrom;
        private DateTimeSelector dtTo;
        private TreeViewEx lstCategories;
        private System.Windows.Forms.Label lblFrom;
        private System.Windows.Forms.Label lblEndDate;
        private System.Windows.Forms.Label lblBody;
        private System.Windows.Forms.Label lblStep;
    }
}