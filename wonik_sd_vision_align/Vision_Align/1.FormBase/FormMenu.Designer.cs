namespace Vision_Align
{
    partial class FormMenu
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
            this.btn_Exit = new System.Windows.Forms.Button();
            this.btn_Minimum = new System.Windows.Forms.Button();
            this.btn_Main = new System.Windows.Forms.Button();
            this.btn_IO = new System.Windows.Forms.Button();
            this.btn_Log = new System.Windows.Forms.Button();
            this.btn_Alram = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btn_Exit
            // 
            this.btn_Exit.BackColor = System.Drawing.SystemColors.Control;
            this.btn_Exit.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btn_Exit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_Exit.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_Exit.Location = new System.Drawing.Point(1678, 10);
            this.btn_Exit.Name = "btn_Exit";
            this.btn_Exit.Size = new System.Drawing.Size(235, 53);
            this.btn_Exit.TabIndex = 0;
            this.btn_Exit.Text = "PROGRAM EXIT";
            this.btn_Exit.UseVisualStyleBackColor = false;
            this.btn_Exit.Click += new System.EventHandler(this.btn_Exit_Click);
            // 
            // btn_Minimum
            // 
            this.btn_Minimum.BackColor = System.Drawing.SystemColors.Control;
            this.btn_Minimum.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btn_Minimum.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_Minimum.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_Minimum.Location = new System.Drawing.Point(1566, 10);
            this.btn_Minimum.Name = "btn_Minimum";
            this.btn_Minimum.Size = new System.Drawing.Size(106, 53);
            this.btn_Minimum.TabIndex = 1;
            this.btn_Minimum.Text = "MINI";
            this.btn_Minimum.UseVisualStyleBackColor = false;
            this.btn_Minimum.Click += new System.EventHandler(this.btn_Minimum_Click);
            // 
            // btn_Main
            // 
            this.btn_Main.BackColor = System.Drawing.SystemColors.Control;
            this.btn_Main.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btn_Main.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_Main.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_Main.Location = new System.Drawing.Point(12, 10);
            this.btn_Main.Name = "btn_Main";
            this.btn_Main.Size = new System.Drawing.Size(152, 53);
            this.btn_Main.TabIndex = 2;
            this.btn_Main.Text = "VISION";
            this.btn_Main.UseVisualStyleBackColor = false;
            this.btn_Main.Click += new System.EventHandler(this.btn_Main_Click);
            // 
            // btn_IO
            // 
            this.btn_IO.BackColor = System.Drawing.SystemColors.Control;
            this.btn_IO.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btn_IO.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_IO.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_IO.Location = new System.Drawing.Point(171, 10);
            this.btn_IO.Name = "btn_IO";
            this.btn_IO.Size = new System.Drawing.Size(152, 53);
            this.btn_IO.TabIndex = 4;
            this.btn_IO.Text = "I/O";
            this.btn_IO.UseVisualStyleBackColor = false;
            this.btn_IO.Click += new System.EventHandler(this.btn_IO_Click);
            // 
            // btn_Log
            // 
            this.btn_Log.BackColor = System.Drawing.SystemColors.Control;
            this.btn_Log.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btn_Log.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_Log.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_Log.Location = new System.Drawing.Point(328, 10);
            this.btn_Log.Name = "btn_Log";
            this.btn_Log.Size = new System.Drawing.Size(152, 53);
            this.btn_Log.TabIndex = 5;
            this.btn_Log.Text = "LOG VIEW";
            this.btn_Log.UseVisualStyleBackColor = false;
            this.btn_Log.Click += new System.EventHandler(this.btn_Log_Click);
            // 
            // btn_Alram
            // 
            this.btn_Alram.BackColor = System.Drawing.SystemColors.Control;
            this.btn_Alram.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btn_Alram.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_Alram.Font = new System.Drawing.Font("Arial", 9F);
            this.btn_Alram.Location = new System.Drawing.Point(486, 10);
            this.btn_Alram.Name = "btn_Alram";
            this.btn_Alram.Size = new System.Drawing.Size(152, 53);
            this.btn_Alram.TabIndex = 6;
            this.btn_Alram.Text = "ALRAM";
            this.btn_Alram.UseVisualStyleBackColor = false;
            this.btn_Alram.Click += new System.EventHandler(this.btn_Alram_Click);
            // 
            // FormMenu
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(1920, 74);
            this.ControlBox = false;
            this.Controls.Add(this.btn_Alram);
            this.Controls.Add(this.btn_Log);
            this.Controls.Add(this.btn_IO);
            this.Controls.Add(this.btn_Main);
            this.Controls.Add(this.btn_Minimum);
            this.Controls.Add(this.btn_Exit);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FormMenu";
            this.Text = "FormMenu";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btn_Exit;
        private System.Windows.Forms.Button btn_Minimum;
        private System.Windows.Forms.Button btn_Main;
        private System.Windows.Forms.Button btn_IO;
        private System.Windows.Forms.Button btn_Log;
        private System.Windows.Forms.Button btn_Alram;
    }
}