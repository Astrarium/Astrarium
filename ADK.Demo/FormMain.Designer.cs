namespace ADK.Demo
{
    partial class FormMain
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.skyView = new ADK.Demo.SkyView();
            this.SuspendLayout();
            // 
            // skyView
            // 
            this.skyView.BackColor = System.Drawing.Color.Black;
            this.skyView.Cursor = System.Windows.Forms.Cursors.Cross;
            this.skyView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.skyView.Location = new System.Drawing.Point(0, 0);
            this.skyView.Name = "skyView";
            this.skyView.Size = new System.Drawing.Size(800, 450);
            this.skyView.SkyMap = null;
            this.skyView.TabIndex = 0;
            this.skyView.TabStop = false;
            this.skyView.MouseMove += new System.Windows.Forms.MouseEventHandler(this.skyView_MouseMove);
            this.skyView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.skyView_KeyDown);
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.skyView);
            this.Name = "FormMain";
            this.Text = "ADK Demo App";            
            this.ResumeLayout(false);

        }

        #endregion

        private SkyView skyView;
    }
}

