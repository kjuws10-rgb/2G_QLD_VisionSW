namespace Vision_Align
{
    partial class FormPreview
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.label_Title = new System.Windows.Forms.Label();
            this.gridView_LogDate = new System.Windows.Forms.DataGridView();
            this.col01 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.label20 = new System.Windows.Forms.Label();
            this.cb_Align_Ok = new System.Windows.Forms.CheckBox();
            this.cb_Glass_Loding_Pos = new System.Windows.Forms.CheckBox();
            this.panel_Display = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.gridView_LogDate)).BeginInit();
            this.SuspendLayout();
            // 
            // label_Title
            // 
            this.label_Title.BackColor = System.Drawing.Color.Black;
            this.label_Title.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label_Title.Font = new System.Drawing.Font("Arial", 9F);
            this.label_Title.ForeColor = System.Drawing.Color.White;
            this.label_Title.Location = new System.Drawing.Point(0, 0);
            this.label_Title.Name = "label_Title";
            this.label_Title.Size = new System.Drawing.Size(749, 18);
            this.label_Title.TabIndex = 1155;
            this.label_Title.Text = "Pre Review Cam#1";
            this.label_Title.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // gridView_LogDate
            // 
            this.gridView_LogDate.AllowUserToAddRows = false;
            this.gridView_LogDate.AllowUserToDeleteRows = false;
            this.gridView_LogDate.AllowUserToResizeRows = false;
            this.gridView_LogDate.BackgroundColor = System.Drawing.Color.Black;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
            dataGridViewCellStyle1.Font = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.gridView_LogDate.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.gridView_LogDate.ColumnHeadersHeight = 20;
            this.gridView_LogDate.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.gridView_LogDate.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.col01});
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.Black;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("굴림", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.gridView_LogDate.DefaultCellStyle = dataGridViewCellStyle3;
            this.gridView_LogDate.EnableHeadersVisualStyles = false;
            this.gridView_LogDate.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.gridView_LogDate.Location = new System.Drawing.Point(588, 19);
            this.gridView_LogDate.Name = "gridView_LogDate";
            this.gridView_LogDate.ReadOnly = true;
            this.gridView_LogDate.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.gridView_LogDate.RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
            this.gridView_LogDate.RowHeadersVisible = false;
            this.gridView_LogDate.RowHeadersWidth = 20;
            this.gridView_LogDate.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.gridView_LogDate.RowsDefaultCellStyle = dataGridViewCellStyle5;
            this.gridView_LogDate.RowTemplate.Height = 40;
            this.gridView_LogDate.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridView_LogDate.Size = new System.Drawing.Size(158, 284);
            this.gridView_LogDate.TabIndex = 1158;
            // 
            // col01
            // 
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.col01.DefaultCellStyle = dataGridViewCellStyle2;
            this.col01.HeaderText = "Data";
            this.col01.Name = "col01";
            this.col01.ReadOnly = true;
            this.col01.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.col01.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.col01.Width = 300;
            // 
            // label20
            // 
            this.label20.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.label20.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label20.Font = new System.Drawing.Font("Arial", 9F);
            this.label20.ForeColor = System.Drawing.Color.White;
            this.label20.Location = new System.Drawing.Point(588, 306);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(158, 19);
            this.label20.TabIndex = 1159;
            this.label20.Text = "Data Filter";
            this.label20.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // cb_Align_Ok
            // 
            this.cb_Align_Ok.AutoSize = true;
            this.cb_Align_Ok.Font = new System.Drawing.Font("Arial", 9F);
            this.cb_Align_Ok.Location = new System.Drawing.Point(590, 328);
            this.cb_Align_Ok.Name = "cb_Align_Ok";
            this.cb_Align_Ok.Size = new System.Drawing.Size(71, 19);
            this.cb_Align_Ok.TabIndex = 1160;
            this.cb_Align_Ok.Text = "Align Ok";
            this.cb_Align_Ok.UseVisualStyleBackColor = true;
            // 
            // cb_Glass_Loding_Pos
            // 
            this.cb_Glass_Loding_Pos.AutoSize = true;
            this.cb_Glass_Loding_Pos.Checked = true;
            this.cb_Glass_Loding_Pos.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cb_Glass_Loding_Pos.Font = new System.Drawing.Font("Arial", 9F);
            this.cb_Glass_Loding_Pos.Location = new System.Drawing.Point(590, 353);
            this.cb_Glass_Loding_Pos.Name = "cb_Glass_Loding_Pos";
            this.cb_Glass_Loding_Pos.Size = new System.Drawing.Size(132, 19);
            this.cb_Glass_Loding_Pos.TabIndex = 1161;
            this.cb_Glass_Loding_Pos.Text = "Glass Loading Pos";
            this.cb_Glass_Loding_Pos.UseVisualStyleBackColor = true;
            // 
            // panel_Display
            // 
            this.panel_Display.Location = new System.Drawing.Point(2, 19);
            this.panel_Display.Name = "panel_Display";
            this.panel_Display.Size = new System.Drawing.Size(582, 429);
            this.panel_Display.TabIndex = 1165;
            // 
            // FormPreview
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(70)))));
            this.ClientSize = new System.Drawing.Size(749, 450);
            this.Controls.Add(this.panel_Display);
            this.Controls.Add(this.cb_Glass_Loding_Pos);
            this.Controls.Add(this.cb_Align_Ok);
            this.Controls.Add(this.label20);
            this.Controls.Add(this.gridView_LogDate);
            this.Controls.Add(this.label_Title);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FormPreview";
            this.Text = "FormPreview";
            ((System.ComponentModel.ISupportInitialize)(this.gridView_LogDate)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label_Title;
        private System.Windows.Forms.DataGridView gridView_LogDate;
        private System.Windows.Forms.DataGridViewTextBoxColumn col01;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.CheckBox cb_Align_Ok;
        private System.Windows.Forms.CheckBox cb_Glass_Loding_Pos;
        private System.Windows.Forms.Panel panel_Display;
    }
}