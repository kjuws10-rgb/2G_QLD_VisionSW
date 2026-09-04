namespace Vision_Align
{
    partial class FormTopMassage
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
            this.panel_Title = new System.Windows.Forms.Panel();
            this.label_Title = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel_MoveBar = new System.Windows.Forms.Panel();
            this.btn_Ok = new System.Windows.Forms.Button();
            this.btn_Reset = new System.Windows.Forms.Button();
            this.panel_Title.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel_Title
            // 
            this.panel_Title.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.panel_Title.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel_Title.Controls.Add(this.label_Title);
            this.panel_Title.Location = new System.Drawing.Point(11, 11);
            this.panel_Title.Name = "panel_Title";
            this.panel_Title.Size = new System.Drawing.Size(1098, 257);
            this.panel_Title.TabIndex = 0;
            // 
            // label_Title
            // 
            this.label_Title.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label_Title.Font = new System.Drawing.Font("굴림", 72F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label_Title.Location = new System.Drawing.Point(0, 0);
            this.label_Title.Name = "label_Title";
            this.label_Title.Size = new System.Drawing.Size(1096, 255);
            this.label_Title.TabIndex = 0;
            this.label_Title.Text = "HOMING";
            this.label_Title.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.Control;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.btn_Reset);
            this.panel1.Controls.Add(this.panel_MoveBar);
            this.panel1.Controls.Add(this.btn_Ok);
            this.panel1.Controls.Add(this.panel_Title);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1122, 379);
            this.panel1.TabIndex = 1;
            // 
            // panel_MoveBar
            // 
            this.panel_MoveBar.Location = new System.Drawing.Point(11, 15);
            this.panel_MoveBar.Name = "panel_MoveBar";
            this.panel_MoveBar.Size = new System.Drawing.Size(64, 249);
            this.panel_MoveBar.TabIndex = 1;
            this.panel_MoveBar.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panel_MoveBar_MouseDown);
            this.panel_MoveBar.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panel_MoveBar_MouseMove);
            // 
            // btn_Ok
            // 
            this.btn_Ok.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btn_Ok.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_Ok.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_Ok.Location = new System.Drawing.Point(280, 289);
            this.btn_Ok.Name = "btn_Ok";
            this.btn_Ok.Size = new System.Drawing.Size(235, 77);
            this.btn_Ok.TabIndex = 3;
            this.btn_Ok.Text = "OK";
            this.btn_Ok.UseVisualStyleBackColor = true;
            this.btn_Ok.Click += new System.EventHandler(this.btn_Ok_Click);
            // 
            // btn_Reset
            // 
            this.btn_Reset.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btn_Reset.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_Reset.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_Reset.Location = new System.Drawing.Point(544, 289);
            this.btn_Reset.Name = "btn_Reset";
            this.btn_Reset.Size = new System.Drawing.Size(235, 77);
            this.btn_Reset.TabIndex = 4;
            this.btn_Reset.Text = "RESET";
            this.btn_Reset.UseVisualStyleBackColor = true;
            this.btn_Reset.Click += new System.EventHandler(this.btn_Reset_Click);
            // 
            // FormTopMassage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.ClientSize = new System.Drawing.Size(1122, 379);
            this.Controls.Add(this.panel1);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FormTopMassage";
            this.Text = "FormTopMassage";
            this.TopMost = true;
            this.TransparencyKey = System.Drawing.Color.White;
            this.panel_Title.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel_Title;
        private System.Windows.Forms.Label label_Title;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btn_Ok;
        private System.Windows.Forms.Panel panel_MoveBar;
        private System.Windows.Forms.Button btn_Reset;
    }
}