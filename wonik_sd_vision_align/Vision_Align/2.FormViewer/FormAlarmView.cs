using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Vision_Align
{
    public partial class FormAlarmView : Form
    {
        Timer m_timerAlarm = new Timer();
     
        public FormAlarmView()
        {
            InitializeComponent();
        }

        public void Initializ()
        {
            FormDisposition();
            dtpDate.Value = DateTime.Now;

            m_timerAlarm.Interval = 500;
            m_timerAlarm.Tick += new EventHandler(OnTimerAlarm);
        }

        #region Style
        private void FormDisposition()
        {
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            ControlBox = MinimizeBox = MaximizeBox = ShowInTaskbar = false;
        }
        #endregion

        public void ShowViewr()
        {
            LoadAlarmHistory(dtpDate.Value);
            m_timerAlarm.Start();
            Show();
        }

        public void HideViewer()
        {
            m_timerAlarm.Stop();
            Hide();
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            LoadAlarmHistory(dtpDate.Value);
        }

        private void LoadAlarmHistory(DateTime date)
        {
            gridAlarmHistory.Rows.Clear();

            List<AlarmRecord> records = Global.clsAlarmManager.GetAlarmHistory(date);

            foreach (var record in records)
            {
                int rowIndex = gridAlarmHistory.Rows.Add();
                DataGridViewRow row = gridAlarmHistory.Rows[rowIndex];

                row.Cells[0].Value = record.Time.ToString("yyyy-MM-dd HH:mm:ss.fff");
                row.Cells[1].Value = record.Type;
                row.Cells[2].Value = string.Format("[{0}] {1}", (int)record.Code, record.Code);
                row.Cells[3].Value = record.Description;
                row.Cells[4].Value = record.Context;

                if (record.Type == "SET")
                {
                    row.DefaultCellStyle.ForeColor = Color.Red;
                }
                else if (record.Type == "CLEAR")
                {
                    row.DefaultCellStyle.ForeColor = Color.Lime;
                }
            }

            if (gridAlarmHistory.Rows.Count > 0)
            {
                gridAlarmHistory.FirstDisplayedScrollingRowIndex = gridAlarmHistory.Rows.Count - 1;
            }
        }

        private void OnTimerAlarm(object sender, EventArgs e)
        {
            AlarmCode code = Global.clsAlarmManager.CurrentAlarm;
            string desc = ClsAlarmManager.GetDescription(code);

            labelAlarmCode.Text = $"[{(int)code}] {code}";
            labelAlarmDesc.Text = desc;

            if (code == AlarmCode.None)
            {
                labelAlarmCode.ForeColor = Color.Lime;
                labelAlarmDesc.ForeColor = Color.Lime;
            }
            else
            {
                labelAlarmCode.ForeColor = Color.Red;
                labelAlarmDesc.ForeColor = Color.Red;
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            Global.clsAlarmManager.ClearAlarm();
        }
    }
}
