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
            this.components = new System.ComponentModel.Container();
            this.tipCelestialObject = new System.Windows.Forms.ToolTip(this.components);
            this.skyView = new ADK.Demo.SkyView();
            this.SuspendLayout();
            // 
            // tipCelestialObject
            // 
            this.tipCelestialObject.AutoPopDelay = 5000;
            this.tipCelestialObject.InitialDelay = 1000;
            this.tipCelestialObject.ReshowDelay = 100;
            this.tipCelestialObject.UseAnimation = false;
            this.tipCelestialObject.UseFading = false;
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
            this.skyView.DoubleClick += new System.EventHandler(this.skyView_DoubleClick);
            this.skyView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.skyView_KeyDown);
            this.skyView.MouseMove += new System.Windows.Forms.MouseEventHandler(this.skyView_MouseMove);
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.skyView);
            this.Name = "FormMain";
            this.Text = "ADK Demo App";
            this.ResumeLayout(false);

        }

        #endregion

        private SkyView skyView;
        private System.Windows.Forms.ToolTip tipCelestialObject;
    }
}

