namespace Vision_Align
{
    partial class FormBase
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.panelTitle = new System.Windows.Forms.Panel();
            this.panelMenu = new System.Windows.Forms.Panel();
            this.panelView = new System.Windows.Forms.Panel();
            this.timer_ViewerChange = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // panelTitle
            // 
            this.panelTitle.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelTitle.Location = new System.Drawing.Point(1, 1);
            this.panelTitle.Name = "panelTitle";
            this.panelTitle.Size = new System.Drawing.Size(762, 65);
            this.panelTitle.TabIndex = 1;
            // 
            // panelMenu
            // 
            this.panelMenu.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelMenu.Location = new System.Drawing.Point(1, 430);
            this.panelMenu.Name = "panelMenu";
            this.panelMenu.Size = new System.Drawing.Size(762, 65);
            this.panelMenu.TabIndex = 2;
            // 
            // panelView
            // 
            this.panelView.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(70)))));
            this.panelView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelView.Location = new System.Drawing.Point(12, 72);
            this.panelView.Name = "panelView";
            this.panelView.Size = new System.Drawing.Size(619, 352);
            this.panelView.TabIndex = 3;
            // 
            // timer_ViewerChange
            // 
            this.timer_ViewerChange.Tick += new System.EventHandler(this.timer_ViewerChange_Tick);
            // 
            // FormBase
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(70)))));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.ClientSize = new System.Drawing.Size(765, 498);
            this.ControlBox = false;
            this.Controls.Add(this.panelView);
            this.Controls.Add(this.panelMenu);
            this.Controls.Add(this.panelTitle);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormBase";
            this.Text = "HLD Vision Aligner";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelTitle;
        private System.Windows.Forms.Panel panelMenu;
        private System.Windows.Forms.Panel panelView;
        private System.Windows.Forms.Timer timer_ViewerChange;
    }
}

