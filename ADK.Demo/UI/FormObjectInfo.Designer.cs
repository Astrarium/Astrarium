namespace ADK.Demo.UI
{
    partial class FormObjectInfo
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
            this.wbInfo = new System.Windows.Forms.WebBrowser();
            this.SuspendLayout();
            // 
            // wbInfo
            // 
            this.wbInfo.AllowWebBrowserDrop = false;
            this.wbInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.wbInfo.IsWebBrowserContextMenuEnabled = false;
            this.wbInfo.Location = new System.Drawing.Point(12, 12);
            this.wbInfo.MinimumSize = new System.Drawing.Size(20, 20);
            this.wbInfo.Name = "wbInfo";
            this.wbInfo.Size = new System.Drawing.Size(485, 351);
            this.wbInfo.TabIndex = 0;
            this.wbInfo.WebBrowserShortcutsEnabled = false;
            // 
            // FormObjectInfo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(509, 417);
            this.Controls.Add(this.wbInfo);
            this.Name = "FormObjectInfo";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "FormObjectInfo";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.WebBrowser wbInfo;
    }
}