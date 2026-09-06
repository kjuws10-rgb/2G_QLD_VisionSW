namespace Vision_Align
{
    partial class FormAlarmView
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.dtpDate = new System.Windows.Forms.DateTimePicker();
            this.btnSearch = new System.Windows.Forms.Button();
            this.gridAlarmHistory = new System.Windows.Forms.DataGridView();
            this.colTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCode = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDescription = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colContext = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.labelTitle = new System.Windows.Forms.Label();
            this.panelTop = new System.Windows.Forms.Panel();
            this.labelAlarmCode = new System.Windows.Forms.Label();
            this.labelAlarmDesc = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.btnClear = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.panelAlarmStatus = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.gridAlarmHistory)).BeginInit();
            this.panelTop.SuspendLayout();
            this.panelAlarmStatus.SuspendLayout();
            this.SuspendLayout();
            // 
            // dtpDate
            // 
            this.dtpDate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.dtpDate.CalendarFont = new System.Drawing.Font("Arial", 12F);
            this.dtpDate.Font = new System.Drawing.Font("Arial", 14F);
            this.dtpDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpDate.Location = new System.Drawing.Point(247, 124);
            this.dtpDate.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.dtpDate.Name = "dtpDate";
            this.dtpDate.Size = new System.Drawing.Size(176, 29);
            this.dtpDate.TabIndex = 1;
            // 
            // btnSearch
            // 
            this.btnSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSearch.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.btnSearch.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
            this.btnSearch.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSearch.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold);
            this.btnSearch.ForeColor = System.Drawing.Color.White;
            this.btnSearch.Location = new System.Drawing.Point(440, 123);
            this.btnSearch.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(105, 30);
            this.btnSearch.TabIndex = 2;
            this.btnSearch.Text = "Search";
            this.btnSearch.UseVisualStyleBackColor = false;
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            // 
            // gridAlarmHistory
            // 
            this.gridAlarmHistory.AllowUserToAddRows = false;
            this.gridAlarmHistory.AllowUserToDeleteRows = false;
            this.gridAlarmHistory.AllowUserToResizeRows = false;
            this.gridAlarmHistory.BackgroundColor = System.Drawing.Color.Black;
            this.gridAlarmHistory.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.gridAlarmHistory.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            this.gridAlarmHistory.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Arial", 9F);
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.Color.White;
            this.gridAlarmHistory.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.gridAlarmHistory.ColumnHeadersHeight = 40;
            this.gridAlarmHistory.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.gridAlarmHistory.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colTime,
            this.colType,
            this.colCode,
            this.colDescription,
            this.colContext});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.Black;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Arial", 10F);
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(80)))));
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.gridAlarmHistory.DefaultCellStyle = dataGridViewCellStyle2;
            this.gridAlarmHistory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridAlarmHistory.EnableHeadersVisualStyles = false;
            this.gridAlarmHistory.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.gridAlarmHistory.Location = new System.Drawing.Point(0, 163);
            this.gridAlarmHistory.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.gridAlarmHistory.MultiSelect = false;
            this.gridAlarmHistory.Name = "gridAlarmHistory";
            this.gridAlarmHistory.ReadOnly = true;
            this.gridAlarmHistory.RowHeadersVisible = false;
            this.gridAlarmHistory.RowTemplate.Height = 35;
            this.gridAlarmHistory.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridAlarmHistory.Size = new System.Drawing.Size(1399, 542);
            this.gridAlarmHistory.TabIndex = 3;
            // 
            // colTime
            // 
            this.colTime.HeaderText = "Time";
            this.colTime.Name = "colTime";
            this.colTime.ReadOnly = true;
            this.colTime.Width = 250;
            // 
            // colType
            // 
            this.colType.HeaderText = "Type";
            this.colType.Name = "colType";
            this.colType.ReadOnly = true;
            this.colType.Width = 120;
            // 
            // colCode
            // 
            this.colCode.HeaderText = "Alarm Code";
            this.colCode.Name = "colCode";
            this.colCode.ReadOnly = true;
            this.colCode.Width = 200;
            // 
            // colDescription
            // 
            this.colDescription.HeaderText = "Description";
            this.colDescription.Name = "colDescription";
            this.colDescription.ReadOnly = true;
            this.colDescription.Width = 450;
            // 
            // colContext
            // 
            this.colContext.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colContext.HeaderText = "Detail";
            this.colContext.Name = "colContext";
            this.colContext.ReadOnly = true;
            // 
            // labelTitle
            // 
            this.labelTitle.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelTitle.Font = new System.Drawing.Font("Arial", 16F, System.Drawing.FontStyle.Bold);
            this.labelTitle.ForeColor = System.Drawing.Color.White;
            this.labelTitle.Location = new System.Drawing.Point(12, 124);
            this.labelTitle.Name = "labelTitle";
            this.labelTitle.Size = new System.Drawing.Size(219, 27);
            this.labelTitle.TabIndex = 0;
            this.labelTitle.Text = "Alarm History";
            this.labelTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // panelTop
            // 
            this.panelTop.BackColor = System.Drawing.Color.Transparent;
            this.panelTop.Controls.Add(this.panelAlarmStatus);
            this.panelTop.Controls.Add(this.label3);
            this.panelTop.Controls.Add(this.label2);
            this.panelTop.Controls.Add(this.btnClear);
            this.panelTop.Controls.Add(this.label1);
            this.panelTop.Controls.Add(this.labelTitle);
            this.panelTop.Controls.Add(this.dtpDate);
            this.panelTop.Controls.Add(this.btnSearch);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(1399, 163);
            this.panelTop.TabIndex = 0;
            // 
            // labelAlarmCode
            // 
            this.labelAlarmCode.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.labelAlarmCode.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold);
            this.labelAlarmCode.ForeColor = System.Drawing.Color.Lime;
            this.labelAlarmCode.Location = new System.Drawing.Point(2, 2);
            this.labelAlarmCode.Name = "labelAlarmCode";
            this.labelAlarmCode.Size = new System.Drawing.Size(468, 27);
            this.labelAlarmCode.TabIndex = 0;
            this.labelAlarmCode.Text = "[0] None";
            this.labelAlarmCode.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelAlarmDesc
            // 
            this.labelAlarmDesc.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.labelAlarmDesc.Font = new System.Drawing.Font("Arial", 10F);
            this.labelAlarmDesc.ForeColor = System.Drawing.Color.Lime;
            this.labelAlarmDesc.Location = new System.Drawing.Point(2, 34);
            this.labelAlarmDesc.Name = "labelAlarmDesc";
            this.labelAlarmDesc.Size = new System.Drawing.Size(468, 27);
            this.labelAlarmDesc.TabIndex = 1;
            this.labelAlarmDesc.Text = "No Alarm";
            this.labelAlarmDesc.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.Font = new System.Drawing.Font("Arial", 16F, System.Drawing.FontStyle.Bold);
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(12, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(219, 27);
            this.label1.TabIndex = 1167;
            this.label1.Text = "Current Alarm";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnClear
            // 
            this.btnClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnClear.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.btnClear.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
            this.btnClear.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClear.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold);
            this.btnClear.ForeColor = System.Drawing.Color.White;
            this.btnClear.Location = new System.Drawing.Point(630, 44);
            this.btnClear.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(105, 60);
            this.btnClear.TabIndex = 1168;
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = false;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.Font = new System.Drawing.Font("Arial", 14F);
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(45, 44);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(95, 27);
            this.label2.TabIndex = 1169;
            this.label2.Text = "Code";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label3.Font = new System.Drawing.Font("Arial", 14F);
            this.label3.ForeColor = System.Drawing.Color.White;
            this.label3.Location = new System.Drawing.Point(45, 76);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(95, 27);
            this.label3.TabIndex = 1170;
            this.label3.Text = "Message";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // panelAlarmStatus
            // 
            this.panelAlarmStatus.Controls.Add(this.labelAlarmCode);
            this.panelAlarmStatus.Controls.Add(this.labelAlarmDesc);
            this.panelAlarmStatus.Location = new System.Drawing.Point(154, 42);
            this.panelAlarmStatus.Name = "panelAlarmStatus";
            this.panelAlarmStatus.Size = new System.Drawing.Size(470, 62);
            this.panelAlarmStatus.TabIndex = 1171;
            // 
            // FormAlarmView
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(70)))));
            this.ClientSize = new System.Drawing.Size(1399, 705);
            this.Controls.Add(this.gridAlarmHistory);
            this.Controls.Add(this.panelTop);
            this.Font = new System.Drawing.Font("Arial", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "FormAlarmView";
            this.Text = "FormAlarmView";
            ((System.ComponentModel.ISupportInitialize)(this.gridAlarmHistory)).EndInit();
            this.panelTop.ResumeLayout(false);
            this.panelAlarmStatus.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.DateTimePicker dtpDate;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.DataGridView gridAlarmHistory;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTime;
        private System.Windows.Forms.DataGridViewTextBoxColumn colType;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCode;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDescription;
        private System.Windows.Forms.DataGridViewTextBoxColumn colContext;
        private System.Windows.Forms.Label labelAlarmCode;
        private System.Windows.Forms.Label labelAlarmDesc;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Panel panelAlarmStatus;
    }
}
