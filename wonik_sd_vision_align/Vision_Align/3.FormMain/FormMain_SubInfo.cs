using HalconDotNet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using uEye.Types;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
//using uEye;

namespace Vision_Align
{
    public partial class FormMain_SubInfo : Form
    {
        Timer m_timer = new Timer();
        Timer m_timerLog = new Timer();

        Timer m_timerLive = new Timer();

        public FormMain_SubInfo()
        {
            InitializeComponent();

            Global.clsAutoThread.ProcessStatusMsg += ClsAutoThread_ProcessStatusMsg;

            //첫 이미지 노이즈 제거용
            Global.dicClsCam[CamInfo.CAM_1].Grab();
            Global.dicClsCam[CamInfo.CAM_2].Grab();
            Global.dicClsCam[CamInfo.CAM_1].Grab();
            Global.dicClsCam[CamInfo.CAM_2].Grab();
        }

        private void ClsAutoThread_ProcessStatusMsg(object sender, string msg)
        {
            DateTime time = DateTime.Now;
            Task.Run(() =>
            {
                if(InvokeRequired)
                {
                    Invoke(new Action(() =>
                    {
                        if(listBoxProcessStatus.Items.Count > 200)
                        {
                            listBoxProcessStatus.Items.Remove(0);
                        }
                        string message = $"[{time.ToString("yyMMdd-HHmmss.fff")}] {msg}";
                        listBoxProcessStatus.Items.Add(message);
                        listBoxProcessStatus.SelectedIndex = listBoxProcessStatus.Items.Count - 1;
                        Global.logger[LogType.SYSTEM].Write(msg);
                    }));
                }
            });
        }


        public void Initializ()
        {
            FormDisposition();

            m_timer.Interval = 500;
            m_timer.Tick += new EventHandler(OnTimer);

            m_timerLog.Interval = 10;
            m_timerLog.Tick += new EventHandler(OnTimerLog);

            m_timerLive.Interval = 1;
            m_timerLive.Tick += new EventHandler(OnTimerLive);
        }

        #region Timer
        void OnTimerLive(object obj, EventArgs ea)
        {
            try
            {
                Global.dicClsCam[CamInfo.CAM_1].Grab();
                Global.dicClsCam[CamInfo.CAM_2].Grab();

                Global.inforResult.dFocus[(int)CamInfo.CAM_1] = Global.clsAlgorithm[(int)CamInfo.CAM_1].MeasureSharpness();
                Global.inforResult.dFocus[(int)CamInfo.CAM_2] = Global.clsAlgorithm[(int)CamInfo.CAM_2].MeasureSharpness();
            }
            catch { }
        }

        void OnTimer(object obj, EventArgs ea)
        {
            SetValue();
        }

        void OnTimerLog(object obj, EventArgs ea)
        {
            this.m_timerLog.Enabled = false;

            if (Global.queueProcessLog.Count > 1000)
                Global.queueProcessLog.Clear();

            while (Global.queueProcessLog.Count != 0)
            {
                AddMessageLog(listBoxProcessStatus, Global.queueProcessLog.Dequeue());
            }
            this.m_timerLog.Enabled = true;
        }

        private void TimerStart()
        {
            m_timer.Start();
            m_timerLog.Start();
        }

        private void TimerStop()
        {
            m_timer.Stop();
            m_timerLog.Stop();
        }
        #endregion

        public void ShowViewer()
        {
            TimerStart();
            this.Show();
        }

        public void HideViewer()
        {
            TimerStop();
            this.Hide();
        }

        #region Style
        private void FormDisposition()
        {
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            ControlBox = MinimizeBox = MaximizeBox = ShowInTaskbar = false;
        }

        #endregion

        #region LogBox Message
        private void AddMessageLog(object listBox, object state)
        {
            ListBox LogBox = (ListBox)listBox;

            if (LogBox.Items == null) return;


            string strMessage = (string)state;

            if (strMessage == null) return;
            LogBox.Items.Insert(0, strMessage);
            if (LogBox.Items.Count > 500)
                LogBox.Items.RemoveAt(500);
        }
        #endregion

        public void SetValue()
        {
            SetInfoRslt();
            SetRsltDetails();
            SetUVW();
        }

        private void SetInfoRslt()
        {
            if (Global.inforResult.strJudg == "OK")
                label_Judgement.BackColor = Color.Lime;
            else if (Global.inforResult.strJudg == "-")
                label_Judgement.BackColor = Color.FromArgb(45,45,48);
            else
                label_Judgement.BackColor = Color.Red;

            if (label_Judgement.BackColor != Color.FromArgb(45, 45, 48))
                label_Judgement.ForeColor = Color.Black;
            else
                label_Judgement.ForeColor = Color.White;

            label_Judgement.Text = string.Format(Global.inforResult.strJudg);
            label_Err_X    .Text = string.Format(Global.inforResult.resultErrorX.ToString("F3"));
            label_Err_Y    .Text = string.Format(Global.inforResult.resultErrorY.ToString("F3"));
            label_Err_T    .Text = string.Format(Global.inforResult.resultErrorT.ToString("F6"));
            label_Retry    .Text = string.Format(Global.inforPLC.InPmcCurrentRetry + " / " + Global.inforPLC.InPmcRetry);

            double dTemp = Global.inforResult.dProcessTime[0] + Global.inforResult.dProcessTime[1];
            label_Process_Time.Text = string.Format(dTemp.ToString());
        }

        private void SetRsltDetails()
        {
            labelContactSlipX.Text = Global.inforResult.ContactErrorX.ToString("F3");
            labelContactSlipY.Text = Global.inforResult.ContactErrorY.ToString("F3");
            labelContactSlipT.Text = Global.inforResult.ContactErrorT.ToString("F3");
        }

        private void SetUVW()
        {
            double currentX, currentY, currentAngle;
            ClsAlgorithm.StageCalInverse(Global.inforPLC.InStageCurrentU, Global.inforPLC.InStageCurrentW, Global.inforPLC.InStageCurrentV, out currentX, out currentY, out currentAngle);
            label_OffsetX.Text = string.Format(currentX.ToString("F3"));
            label_OffsetY.Text = string.Format(currentY.ToString("F3"));
            label_OffsetT.Text = string.Format(currentAngle.ToString("F6"));

            label_UVW_X1.Text = string.Format(Global.inforPLC.InStageCurrentU.ToString("F3"));
            label_UVW_X2.Text = string.Format(Global.inforPLC.InStageCurrentW.ToString("F3"));
            label_UVW_Y1.Text = string.Format(Global.inforPLC.InStageCurrentV.ToString("F3"));
        }

        bool m_bStop = false;
        private void btn_Live_Click(object sender, EventArgs e)
        {
            if (Global.bAutoMode)
            {
                MessageBox.Show("Please turn off the Auto Mode", "Message", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!m_bStop)
            {
                m_timerLive.Start();

                btn_Live.Text = "Stop";
                Global.inforCam.bLive = m_bStop = true;
            }
            else
            {
                m_timerLive.Stop();

                btn_Live.Text = "Live";
                Global.inforCam.bLive = m_bStop = false;
            }
        }

        private void btn_Snap_Click(object sender, EventArgs e)
        {
            if (Global.bAutoMode)
            {
                MessageBox.Show("Please turn off the Auto Mode", "Message", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Global.dicClsCam[CamInfo.CAM_1].Grab();
            Global.dicClsCam[CamInfo.CAM_2].Grab();

            Global.inforResult.dFocus[(int)CamInfo.CAM_1] = Global.clsAlgorithm[(int)CamInfo.CAM_1].MeasureSharpness();
            Global.inforResult.dFocus[(int)CamInfo.CAM_2] = Global.clsAlgorithm[(int)CamInfo.CAM_2].MeasureSharpness();
        }

        private void btn_Judgement_Click(object sender, EventArgs e)
        {
            if (Global.bAutoMode) return;

            double x, y, t;
            Global.clsVision.GetUvwTargetPos(out x, out y, out t);

            DateTime ts = DateTime.Now;
            Global.clsFolderThread.RequestSave(ts, "MANUAL_JUDGE");
        }


        private void btn_Capture_Click(object sender, EventArgs e)
        {
            Global.clsFolderThread.SaveBothCamsBmp(DateTime.Now, "Capture");
            MessageBox.Show("Image saved successfully.", "Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
