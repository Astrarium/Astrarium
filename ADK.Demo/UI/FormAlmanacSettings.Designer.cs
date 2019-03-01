namespace ADK.Demo.UI
{
    partial class FormAlmanacSettings
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
            this.lstCategories = new System.Windows.Forms.TreeView();
            this.dtTo = new ADK.Demo.UI.DateTimeSelector();
            this.dtFrom = new ADK.Demo.UI.DateTimeSelector();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(353, 424);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(72, 23);
            this.btnCancel.TabIndex = 10;
            this.btnCancel.Text = "&Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.Location = new System.Drawing.Point(274, 424);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(72, 23);
            this.btnOK.TabIndex = 9;
            this.btnOK.Text = "&OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // lstCategories
            // 
            this.lstCategories.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstCategories.CheckBoxes = true;
            this.lstCategories.FullRowSelect = true;
            this.lstCategories.Location = new System.Drawing.Point(13, 80);
            this.lstCategories.Name = "lstCategories";
            this.lstCategories.ShowLines = false;
            this.lstCategories.ShowPlusMinus = false;
            this.lstCategories.ShowRootLines = false;
            this.lstCategories.Size = new System.Drawing.Size(412, 333);
            this.lstCategories.TabIndex = 14;
            this.lstCategories.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.lstCategories_AfterCheck);
            this.lstCategories.BeforeCollapse += new System.Windows.Forms.TreeViewCancelEventHandler(this.lstCategories_BeforeCollapse);
            this.lstCategories.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.lstCategories_NodeMouseDoubleClick);
            // 
            // drTo
            // 
            this.dtTo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.dtTo.DateFormat = ADK.Demo.UI.DateOptions.DateOnly;
            this.dtTo.JulianDay = 2458544.0400693328D;
            this.dtTo.Location = new System.Drawing.Point(286, 44);
            this.dtTo.Name = "drTo";
            this.dtTo.Size = new System.Drawing.Size(139, 22);
            this.dtTo.TabIndex = 13;
            this.dtTo.UtcOffset = 0D;
            // 
            // dtFrom
            // 
            this.dtFrom.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.dtFrom.DateFormat = ADK.Demo.UI.DateOptions.DateOnly;
            this.dtFrom.JulianDay = 2458544.0400689626D;
            this.dtFrom.Location = new System.Drawing.Point(286, 12);
            this.dtFrom.Name = "dtFrom";
            this.dtFrom.Size = new System.Drawing.Size(139, 22);
            this.dtFrom.TabIndex = 12;
            this.dtFrom.UtcOffset = 0D;
            // 
            // FormAlmanacSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(437, 459);
            this.Controls.Add(this.lstCategories);
            this.Controls.Add(this.dtTo);
            this.Controls.Add(this.dtFrom);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Name = "FormAlmanacSettings";
            this.ShowIcon = false;
            this.Text = "Astronomical Phenomena";
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private DateTimeSelector dtFrom;
        private DateTimeSelector dtTo;
        private System.Windows.Forms.TreeView lstCategories;
    }
}