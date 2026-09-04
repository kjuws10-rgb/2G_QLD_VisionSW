using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HalconDotNet;

namespace Vision_Align
{

    public partial class FormDisplay : Form
    {

        FormHalconDisplay HalconDisplay;

        Timer m_timer = new Timer();
        int m_nCamNo = -1;
        public FormDisplay()
        {
            InitializeComponent();
        }

        public void Initializ(int nCamNo)
        {
            FormDisposition();
            SetStyle();

            m_nCamNo = nCamNo;
            SetTitle(m_nCamNo);


            m_timer.Interval = 100;
            m_timer.Tick += new EventHandler(OnTimer);
            m_timer.Start();

            Global.formHDisplay[(int)HWindowType.MAIN_1 + m_nCamNo] = new FormHalconDisplay(m_nCamNo, true);
            Global.formHDisplay[(int)HWindowType.MAIN_1 + m_nCamNo].TopLevel = false;

            panel_Display.Controls.Clear();
            panel_Display.Controls.Add(Global.formHDisplay[(int)HWindowType.MAIN_1 + m_nCamNo]);

            Global.formHDisplay[(int)HWindowType.MAIN_1 + m_nCamNo].Show();
            Global.formHDisplay[(int)HWindowType.MAIN_1 + m_nCamNo].Dock = DockStyle.Fill;
        }

        #region Style
        private void FormDisposition()
        {
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            ControlBox = MinimizeBox = MaximizeBox = ShowInTaskbar = false;
        }

        private void SetStyle()
        {

        }
        #endregion

        void OnTimer(object obj, EventArgs ea)
        {
            // Status
            label_Status.Text = Global.dicClsCam[(CamInfo)m_nCamNo].IsOpen() ? "Connected" : "DisConnected";
            
            // Mode
            if(Global.bAutoMode)
            {
                label_Mode.Text = Global.PreConfig_Param.bAlways_Live ? "Auto Live" : "Auto Grab";
            }
            else
            {
                label_Mode.Text = Global.inforCam.bLive ? "Live" : "Stop";
            }
            label_Mode.BackColor = label_Mode.Text.Contains("Live") ? Color.Lime : Color.FromArgb(45,45,48);
            if (label_Mode.BackColor == Color.Lime)
                label_Mode.ForeColor = Color.Black;
            else
                label_Mode.ForeColor = Color.White;
            
            // Result(Target) 
            label_Target_X.Text = string.Format("{0:F2}", Global.inforResult.dTarget_X[m_nCamNo]);
            label_Target_Y    .Text = string.Format("{0:F2}", Global.inforResult.dTarget_Y      [m_nCamNo]);
            label_Target_Scale.Text = string.Format("{0:F1}", Global.inforResult.dTarget_Scale  [m_nCamNo] * 100.0);
            label_Target_Score.Text = string.Format("{0:F1}", Global.inforResult.dTarget_Score  [m_nCamNo] * 100.0);
            label_Target_Match.Text = string.Format("{0}"   , Global.inforResult.strTarget_Match[m_nCamNo]);

            label_Target_Score.BackColor = Global.PreConfig_Param.dLimitGlass <= Global.inforResult.dTarget_Score[m_nCamNo] ? Color.Lime : Color.FromArgb(45, 45, 48);
            if (label_Target_Score.BackColor == Color.Lime)
                label_Target_Score.ForeColor = Color.Black;
            else
                label_Target_Score.ForeColor = Color.White;

            // Result(Mark)
            label_Mark_X    .Text = string.Format("{0:F2}", Global.inforResult.dMark_X      [m_nCamNo]);
            label_Mark_Y    .Text = string.Format("{0:F2}", Global.inforResult.dMark_Y      [m_nCamNo]);
            label_Mark_Scale.Text = string.Format("{0:F1}", Global.inforResult.dMark_Scale  [m_nCamNo] * 100.0);
            label_Mark_Score.Text = string.Format("{0:F1}", Global.inforResult.dMark_Score  [m_nCamNo] * 100.0);
            label_Mark_Match.Text = string.Format("{0}"   , Global.inforResult.strMark_Match[m_nCamNo]);

            label_Mark_Score.BackColor = Global.PreConfig_Param.dLimitMark <= Global.inforResult.dMark_Score[m_nCamNo] ? Color.Lime : Color.FromArgb(45, 45, 48);
            if (label_Mark_Score.BackColor == Color.Lime)
                label_Mark_Score.ForeColor = Color.Black;
            else
                label_Mark_Score.ForeColor = Color.White;

            // Information
            label_Info_Threshold.Text = string.Format("{0:F2}", Global.inforResult.dGray[m_nCamNo]);
            label_Info_Foucs    .Text = string.Format("{0:F2}", Global.inforResult.dFocus[m_nCamNo]);
            label_Info_Time     .Text = string.Format("{0:F2}ms", Global.inforResult.dProcessTime[m_nCamNo]);
        }

        public void ShowViewr()
        {
            this.Show();
        }

        public void HideViewer()
        {
            this.Hide();
        }

        private void SetTitle(int nCamNo)
        {
            label_Title.Text = string.Format("Pre Align Cam#" + (nCamNo + 1).ToString());
        }

    }
}
