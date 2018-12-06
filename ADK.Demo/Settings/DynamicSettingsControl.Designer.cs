namespace ADK.Demo.Settings
{
    partial class DynamicSettingsControl
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

            if (settings != null)
            {
                settings.OnSettingValueChanged -= UpdateDependentControls;
            }

            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.listSections = new System.Windows.Forms.ListBox();
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // listSections
            // 
            this.listSections.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listSections.FormattingEnabled = true;
            this.listSections.Location = new System.Drawing.Point(0, 0);
            this.listSections.Name = "listSections";
            this.listSections.Size = new System.Drawing.Size(150, 400);
            this.listSections.TabIndex = 0;
            this.listSections.SelectedIndexChanged += new System.EventHandler(this.listSections_SelectedIndexChanged);
            // 
            // splitContainer
            // 
            this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer.Location = new System.Drawing.Point(0, 0);
            this.splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.listSections);
            this.splitContainer.Panel1MinSize = 100;
            this.splitContainer.Size = new System.Drawing.Size(600, 400);
            this.splitContainer.SplitterDistance = 150;
            this.splitContainer.TabIndex = 1;
            // 
            // DynamicSettingsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer);
            this.Name = "DynamicSettingsControl";
            this.Size = new System.Drawing.Size(600, 400);
            this.splitContainer.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox listSections;
        private System.Windows.Forms.SplitContainer splitContainer;
    }
}
