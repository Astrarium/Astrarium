namespace ADK.Demo.Settings
{
    partial class ColorPicker
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
            this.lblCaption = new System.Windows.Forms.Label();
            this.btnPicker = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // lblCaption
            // 
            this.lblCaption.AutoSize = true;
            this.lblCaption.Location = new System.Drawing.Point(17, 4);
            this.lblCaption.Name = "lblCaption";
            this.lblCaption.Size = new System.Drawing.Size(43, 13);
            this.lblCaption.TabIndex = 1;
            this.lblCaption.Text = "Caption";
            this.lblCaption.AutoSizeChanged += new System.EventHandler(this.btnPicker_Click);
            this.lblCaption.Click += new System.EventHandler(this.btnPicker_Click);
            // 
            // btnPicker
            // 
            this.btnPicker.Location = new System.Drawing.Point(0, 4);
            this.btnPicker.Name = "btnPicker";
            this.btnPicker.Size = new System.Drawing.Size(13, 13);
            this.btnPicker.TabIndex = 2;
            this.btnPicker.Click += new System.EventHandler(this.btnPicker_Click);
            // 
            // ColorPicker
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.Controls.Add(this.btnPicker);
            this.Controls.Add(this.lblCaption);
            this.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.Name = "ColorPicker";
            this.Size = new System.Drawing.Size(64, 21);
            this.Click += new System.EventHandler(this.btnPicker_Click);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label lblCaption;
        private System.Windows.Forms.Panel btnPicker;
    }
}
