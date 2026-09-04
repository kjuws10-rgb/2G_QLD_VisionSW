namespace Vision_Align
{
    partial class FormTitle
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
            this.picture_Log = new System.Windows.Forms.PictureBox();
            this.label_Title = new System.Windows.Forms.Label();
            this.panel_RecipeName = new System.Windows.Forms.Panel();
            this.label_Mode = new System.Windows.Forms.Label();
            this.panel_DateTime = new System.Windows.Forms.Panel();
            this.label_DateTime = new System.Windows.Forms.Label();
            this.panel_Var = new System.Windows.Forms.Panel();
            this.label_Ver = new System.Windows.Forms.Label();
            this.panel_Cam = new System.Windows.Forms.Panel();
            this.label_Cam = new System.Windows.Forms.Label();
            this.label_Light = new System.Windows.Forms.Label();
            this.panel_Light = new System.Windows.Forms.Panel();
            this.label_AxisZ = new System.Windows.Forms.Label();
            this.panel_AxisZ = new System.Windows.Forms.Panel();
            this.label_PLC = new System.Windows.Forms.Label();
            this.panel_PLC = new System.Windows.Forms.Panel();
            this.btn_Auto_Manul = new System.Windows.Forms.Button();
            this.btnAlarmClear = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.labelAlarm = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.picture_Log)).BeginInit();
            this.panel_RecipeName.SuspendLayout();
            this.panel_DateTime.SuspendLayout();
            this.panel_Var.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // picture_Log
            // 
            this.picture_Log.Location = new System.Drawing.Point(12, 5);
            this.picture_Log.Name = "picture_Log";
            this.picture_Log.Size = new System.Drawing.Size(197, 50);
            this.picture_Log.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picture_Log.TabIndex = 0;
            this.picture_Log.TabStop = false;
            // 
            // label_Title
            // 
            this.label_Title.Font = new System.Drawing.Font("굴림", 25F);
            this.label_Title.Location = new System.Drawing.Point(222, 5);
            this.label_Title.Name = "label_Title";
            this.label_Title.Size = new System.Drawing.Size(312, 52);
            this.label_Title.TabIndex = 0;
            this.label_Title.Text = "VISION ALIGNER";
            this.label_Title.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panel_RecipeName
            // 
            this.panel_RecipeName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel_RecipeName.Controls.Add(this.label_Mode);
            this.panel_RecipeName.Location = new System.Drawing.Point(534, 9);
            this.panel_RecipeName.Name = "panel_RecipeName";
            this.panel_RecipeName.Size = new System.Drawing.Size(519, 44);
            this.panel_RecipeName.TabIndex = 3;
            // 
            // label_Mode
            // 
            this.label_Mode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label_Mode.Font = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label_Mode.Location = new System.Drawing.Point(0, 0);
            this.label_Mode.Name = "label_Mode";
            this.label_Mode.Size = new System.Drawing.Size(517, 42);
            this.label_Mode.TabIndex = 0;
            this.label_Mode.Text = "Name";
            this.label_Mode.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panel_DateTime
            // 
            this.panel_DateTime.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel_DateTime.Controls.Add(this.label_DateTime);
            this.panel_DateTime.Location = new System.Drawing.Point(1657, 11);
            this.panel_DateTime.Name = "panel_DateTime";
            this.panel_DateTime.Size = new System.Drawing.Size(261, 23);
            this.panel_DateTime.TabIndex = 4;
            // 
            // label_DateTime
            // 
            this.label_DateTime.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label_DateTime.Location = new System.Drawing.Point(0, 0);
            this.label_DateTime.Name = "label_DateTime";
            this.label_DateTime.Size = new System.Drawing.Size(259, 21);
            this.label_DateTime.TabIndex = 0;
            this.label_DateTime.Text = "Date / Time";
            this.label_DateTime.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.label_DateTime.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.label_DateTime_MouseDoubleClick);
            this.label_DateTime.MouseDown += new System.Windows.Forms.MouseEventHandler(this.label_DateTime_MouseDown);
            this.label_DateTime.MouseMove += new System.Windows.Forms.MouseEventHandler(this.label_DateTime_MouseMove);
            this.label_DateTime.MouseUp += new System.Windows.Forms.MouseEventHandler(this.label_DateTime_MouseUp);
            // 
            // panel_Var
            // 
            this.panel_Var.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel_Var.Controls.Add(this.label_Ver);
            this.panel_Var.Location = new System.Drawing.Point(1656, 37);
            this.panel_Var.Name = "panel_Var";
            this.panel_Var.Size = new System.Drawing.Size(261, 23);
            this.panel_Var.TabIndex = 5;
            // 
            // label_Ver
            // 
            this.label_Ver.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label_Ver.Location = new System.Drawing.Point(0, 0);
            this.label_Ver.Name = "label_Ver";
            this.label_Ver.Size = new System.Drawing.Size(259, 21);
            this.label_Ver.TabIndex = 0;
            this.label_Ver.Text = "V1.0";
            this.label_Ver.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panel_Cam
            // 
            this.panel_Cam.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel_Cam.Location = new System.Drawing.Point(1588, 3);
            this.panel_Cam.Name = "panel_Cam";
            this.panel_Cam.Size = new System.Drawing.Size(12, 11);
            this.panel_Cam.TabIndex = 6;
            // 
            // label_Cam
            // 
            this.label_Cam.AutoSize = true;
            this.label_Cam.Font = new System.Drawing.Font("Arial", 6.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_Cam.Location = new System.Drawing.Point(1603, 4);
            this.label_Cam.Name = "label_Cam";
            this.label_Cam.Size = new System.Drawing.Size(25, 11);
            this.label_Cam.TabIndex = 7;
            this.label_Cam.Text = "Cam";
            // 
            // label_Light
            // 
            this.label_Light.AutoSize = true;
            this.label_Light.Font = new System.Drawing.Font("Arial", 6.75F, System.Drawing.FontStyle.Bold);
            this.label_Light.Location = new System.Drawing.Point(1603, 18);
            this.label_Light.Name = "label_Light";
            this.label_Light.Size = new System.Drawing.Size(29, 11);
            this.label_Light.TabIndex = 9;
            this.label_Light.Text = "Light";
            // 
            // panel_Light
            // 
            this.panel_Light.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel_Light.Location = new System.Drawing.Point(1588, 18);
            this.panel_Light.Name = "panel_Light";
            this.panel_Light.Size = new System.Drawing.Size(12, 11);
            this.panel_Light.TabIndex = 8;
            // 
            // label_AxisZ
            // 
            this.label_AxisZ.AutoSize = true;
            this.label_AxisZ.Font = new System.Drawing.Font("Arial", 6.75F, System.Drawing.FontStyle.Bold);
            this.label_AxisZ.Location = new System.Drawing.Point(1603, 33);
            this.label_AxisZ.Name = "label_AxisZ";
            this.label_AxisZ.Size = new System.Drawing.Size(32, 11);
            this.label_AxisZ.TabIndex = 11;
            this.label_AxisZ.Text = "Z Axis";
            // 
            // panel_AxisZ
            // 
            this.panel_AxisZ.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel_AxisZ.Location = new System.Drawing.Point(1588, 33);
            this.panel_AxisZ.Name = "panel_AxisZ";
            this.panel_AxisZ.Size = new System.Drawing.Size(12, 11);
            this.panel_AxisZ.TabIndex = 10;
            // 
            // label_PLC
            // 
            this.label_PLC.AutoSize = true;
            this.label_PLC.Font = new System.Drawing.Font("Arial", 6.75F, System.Drawing.FontStyle.Bold);
            this.label_PLC.Location = new System.Drawing.Point(1603, 48);
            this.label_PLC.Name = "label_PLC";
            this.label_PLC.Size = new System.Drawing.Size(24, 11);
            this.label_PLC.TabIndex = 13;
            this.label_PLC.Text = "PLC";
            // 
            // panel_PLC
            // 
            this.panel_PLC.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel_PLC.Location = new System.Drawing.Point(1588, 48);
            this.panel_PLC.Name = "panel_PLC";
            this.panel_PLC.Size = new System.Drawing.Size(12, 11);
            this.panel_PLC.TabIndex = 12;
            // 
            // btn_Auto_Manul
            // 
            this.btn_Auto_Manul.BackColor = System.Drawing.SystemColors.Control;
            this.btn_Auto_Manul.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btn_Auto_Manul.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_Auto_Manul.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_Auto_Manul.Location = new System.Drawing.Point(1435, 5);
            this.btn_Auto_Manul.Name = "btn_Auto_Manul";
            this.btn_Auto_Manul.Size = new System.Drawing.Size(147, 50);
            this.btn_Auto_Manul.TabIndex = 1145;
            this.btn_Auto_Manul.Text = "Manaul";
            this.btn_Auto_Manul.UseVisualStyleBackColor = false;
            this.btn_Auto_Manul.Click += new System.EventHandler(this.btn_Auto_Manul_Click);
            // 
            // btnAlarmClear
            // 
            this.btnAlarmClear.BackColor = System.Drawing.SystemColors.Control;
            this.btnAlarmClear.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnAlarmClear.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAlarmClear.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAlarmClear.Location = new System.Drawing.Point(1191, 13);
            this.btnAlarmClear.Name = "btnAlarmClear";
            this.btnAlarmClear.Size = new System.Drawing.Size(117, 37);
            this.btnAlarmClear.TabIndex = 1146;
            this.btnAlarmClear.Text = "Alarm Clear";
            this.btnAlarmClear.UseVisualStyleBackColor = false;
            this.btnAlarmClear.Click += new System.EventHandler(this.btnAlarmClear_Click);
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.labelAlarm);
            this.panel1.Location = new System.Drawing.Point(1059, 9);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(127, 44);
            this.panel1.TabIndex = 1147;
            // 
            // labelAlarm
            // 
            this.labelAlarm.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelAlarm.Font = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.labelAlarm.Location = new System.Drawing.Point(0, 0);
            this.labelAlarm.Name = "labelAlarm";
            this.labelAlarm.Size = new System.Drawing.Size(125, 42);
            this.labelAlarm.TabIndex = 0;
            this.labelAlarm.Text = "Alarm";
            this.labelAlarm.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // FormTitle
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1920, 62);
            this.ControlBox = false;
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.btnAlarmClear);
            this.Controls.Add(this.btn_Auto_Manul);
            this.Controls.Add(this.label_Title);
            this.Controls.Add(this.label_PLC);
            this.Controls.Add(this.panel_PLC);
            this.Controls.Add(this.label_AxisZ);
            this.Controls.Add(this.label_Light);
            this.Controls.Add(this.panel_AxisZ);
            this.Controls.Add(this.panel_Light);
            this.Controls.Add(this.label_Cam);
            this.Controls.Add(this.panel_Cam);
            this.Controls.Add(this.panel_Var);
            this.Controls.Add(this.panel_DateTime);
            this.Controls.Add(this.panel_RecipeName);
            this.Controls.Add(this.picture_Log);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FormTitle";
            this.Text = "FormTitle";
            ((System.ComponentModel.ISupportInitialize)(this.picture_Log)).EndInit();
            this.panel_RecipeName.ResumeLayout(false);
            this.panel_DateTime.ResumeLayout(false);
            this.panel_Var.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox picture_Log;
        private System.Windows.Forms.Label label_Title;
        private System.Windows.Forms.Panel panel_RecipeName;
        private System.Windows.Forms.Label label_Mode;
        private System.Windows.Forms.Panel panel_DateTime;
        private System.Windows.Forms.Label label_DateTime;
        private System.Windows.Forms.Panel panel_Var;
        private System.Windows.Forms.Label label_Ver;
        private System.Windows.Forms.Panel panel_Cam;
        private System.Windows.Forms.Label label_Cam;
        private System.Windows.Forms.Label label_Light;
        private System.Windows.Forms.Panel panel_Light;
        private System.Windows.Forms.Label label_AxisZ;
        private System.Windows.Forms.Panel panel_AxisZ;
        private System.Windows.Forms.Label label_PLC;
        private System.Windows.Forms.Panel panel_PLC;
        private System.Windows.Forms.Button btn_Auto_Manul;
        private System.Windows.Forms.Button btnAlarmClear;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label labelAlarm;
    }
}