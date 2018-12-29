namespace DynamicSettings.Editors
{
    partial class DirectoryPicker
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.txtDirectory = new System.Windows.Forms.TextBox();
            this.btnPicker = new System.Windows.Forms.Button();
            this.lblTitle = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // txtDirectory
            // 
            this.txtDirectory.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDirectory.Location = new System.Drawing.Point(0, 17);
            this.txtDirectory.Margin = new System.Windows.Forms.Padding(0);
            this.txtDirectory.Name = "txtDirectory";
            this.txtDirectory.ReadOnly = true;
            this.txtDirectory.Size = new System.Drawing.Size(218, 20);
            this.txtDirectory.TabIndex = 2;
            // 
            // btnPicker
            // 
            this.btnPicker.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnPicker.Location = new System.Drawing.Point(218, 16);
            this.btnPicker.Margin = new System.Windows.Forms.Padding(0);
            this.btnPicker.Name = "btnPicker";
            this.btnPicker.Size = new System.Drawing.Size(24, 22);
            this.btnPicker.TabIndex = 3;
            this.btnPicker.Text = "...";
            this.btnPicker.UseVisualStyleBackColor = true;
            this.btnPicker.Click += new System.EventHandler(this.btnPicker_Click);
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Location = new System.Drawing.Point(-3, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(27, 13);
            this.lblTitle.TabIndex = 4;
            this.lblTitle.Text = "Title";
            // 
            // DirectoryPicker
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.txtDirectory);
            this.Controls.Add(this.btnPicker);
            this.Name = "DirectoryPicker";
            this.Size = new System.Drawing.Size(242, 37);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox txtDirectory;
        private System.Windows.Forms.Button btnPicker;
        private System.Windows.Forms.Label lblTitle;
    }
}
