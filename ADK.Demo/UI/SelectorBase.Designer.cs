using System.Windows.Forms;

namespace ADK.Demo.UI
{
#if DEBUG
    partial class SelectorBase : UserControl
#else
    abstract partial class SelectorBase : UserControl
#endif
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
            this.btnButton = new System.Windows.Forms.Button();
            this.lblText = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();
            // 
            // btnButton
            // 
            this.btnButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnButton.ForeColor = System.Drawing.SystemColors.GrayText;
            this.btnButton.Location = new System.Drawing.Point(121, 1);
            this.btnButton.Margin = new System.Windows.Forms.Padding(0);
            this.btnButton.Name = "btnButton";
            this.btnButton.Size = new System.Drawing.Size(21, 22);
            this.btnButton.TabIndex = 1;
            this.btnButton.TabStop = false;
            this.btnButton.Text = "◢";
            this.btnButton.UseVisualStyleBackColor = true;
            this.btnButton.Click += new System.EventHandler(this.btnButton_Click);
            this.btnButton.Enter += new System.EventHandler(this.btnButton_Enter);
            // 
            // lblText
            // 
            this.lblText.ActiveLinkColor = System.Drawing.Color.RoyalBlue;
            this.lblText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblText.BackColor = System.Drawing.SystemColors.Window;
            this.lblText.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblText.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblText.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.lblText.LinkColor = System.Drawing.SystemColors.ControlText;
            this.lblText.Location = new System.Drawing.Point(0, 0);
            this.lblText.Name = "lblText";
            this.lblText.Size = new System.Drawing.Size(142, 23);
            this.lblText.TabIndex = 3;
            this.lblText.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblText.VisitedLinkColor = System.Drawing.Color.RoyalBlue;
            this.lblText.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lblText_LinkClicked);
            // 
            // SelectorBase
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnButton);
            this.Controls.Add(this.lblText);
            this.Name = "SelectorBase";
            this.Size = new System.Drawing.Size(142, 23);
            this.Resize += new System.EventHandler(this.Selector_Resize);
            this.ResumeLayout(false);

        }

        #endregion

        protected Button btnButton;
        protected LinkLabel lblText;
    }
}
