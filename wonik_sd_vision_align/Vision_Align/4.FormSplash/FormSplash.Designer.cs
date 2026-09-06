namespace Vision_Align
{
    partial class FormSplash
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.panelSpinner = new System.Windows.Forms.Panel();
            this.labelTitle = new System.Windows.Forms.Label();
            this.labelStatus = new System.Windows.Forms.Label();
            this.labelVersion = new System.Windows.Forms.Label();
            this.pictureBoxLogo = new System.Windows.Forms.PictureBox();
            this.timerAnimation = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxLogo)).BeginInit();
            this.SuspendLayout();
            // 
            // panelSpinner
            // 
            this.panelSpinner.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(70)))));
            this.panelSpinner.Location = new System.Drawing.Point(200, 135);
            this.panelSpinner.Name = "panelSpinner";
            this.panelSpinner.Size = new System.Drawing.Size(80, 80);
            this.panelSpinner.TabIndex = 2;
            this.panelSpinner.Paint += new System.Windows.Forms.PaintEventHandler(this.panelSpinner_Paint);
            // 
            // labelTitle
            // 
            this.labelTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 16F, System.Drawing.FontStyle.Bold);
            this.labelTitle.ForeColor = System.Drawing.Color.White;
            this.labelTitle.Location = new System.Drawing.Point(0, 85);
            this.labelTitle.Name = "labelTitle";
            this.labelTitle.Size = new System.Drawing.Size(480, 35);
            this.labelTitle.TabIndex = 1;
            this.labelTitle.Text = "Vision Align";
            this.labelTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelStatus
            // 
            this.labelStatus.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.labelStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(190)))));
            this.labelStatus.Location = new System.Drawing.Point(0, 230);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(480, 25);
            this.labelStatus.TabIndex = 3;
            this.labelStatus.Text = "Initializing...";
            this.labelStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelVersion
            // 
            this.labelVersion.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.labelVersion.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(130)))));
            this.labelVersion.Location = new System.Drawing.Point(0, 280);
            this.labelVersion.Name = "labelVersion";
            this.labelVersion.Size = new System.Drawing.Size(480, 20);
            this.labelVersion.TabIndex = 4;
            this.labelVersion.Text = "v1.0.0.0";
            this.labelVersion.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pictureBoxLogo
            // 
            this.pictureBoxLogo.Location = new System.Drawing.Point(165, 20);
            this.pictureBoxLogo.Name = "pictureBoxLogo";
            this.pictureBoxLogo.Size = new System.Drawing.Size(150, 60);
            this.pictureBoxLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxLogo.TabIndex = 0;
            this.pictureBoxLogo.TabStop = false;
            // 
            // timerAnimation
            // 
            this.timerAnimation.Interval = 80;
            this.timerAnimation.Tick += new System.EventHandler(this.timerAnimation_Tick);
            // 
            // FormSplash
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(70)))));
            this.ClientSize = new System.Drawing.Size(480, 320);
            this.Controls.Add(this.pictureBoxLogo);
            this.Controls.Add(this.labelTitle);
            this.Controls.Add(this.panelSpinner);
            this.Controls.Add(this.labelStatus);
            this.Controls.Add(this.labelVersion);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FormSplash";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "FormSplash";
            this.Load += new System.EventHandler(this.FormSplash_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxLogo)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBoxLogo;
        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.Panel panelSpinner;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.Label labelVersion;
        private System.Windows.Forms.Timer timerAnimation;
    }
}
