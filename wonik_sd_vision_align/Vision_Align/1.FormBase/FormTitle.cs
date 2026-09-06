using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Vision_Align
{
    public partial class FormTitle : Form
    {
        Timer m_timer = new Timer();

        private int m_nFormWidth ;
        private int m_nFormHeight;

        bool bBeforCam = false, bBeforPLC = false, bBeforLight = false, bBeforAxisZ = false;
        Point m_pointMouseDown;
        public FormTitle()
        {
            InitializeComponent();
        }

        public void Initializ()
        {
            m_nFormWidth = this.Width;
            m_nFormHeight = this.Height;
            FormDisposition();
            SetStyle();
            ToolTip();
            m_timer.Interval = 500;
            m_timer.Tick += new EventHandler(OnTimer);
            m_timer.Start();

            label_Mode.Text = "MANUAL";
            label_Mode.BackColor = Color.Yellow;
            label_Mode.ForeColor = Color.Black;
            btn_Auto_Manul.Text = "AUTO";

            // 로고 이미지 로드
            string logoPath = System.IO.Path.Combine(Environment.CurrentDirectory, "Images", "logo.png");
            if (System.IO.File.Exists(logoPath))
            {
                picture_Log.Image = Image.FromFile(logoPath);
            }
        }

        void OnTimer(object obj, EventArgs ea)
        {
            string strDate = string.Format("{0:D4} / {1:D2} / {2:D2}   {3:D2} : {4:D2} : {5:D2}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

            label_DateTime.Text = strDate;

            Global.bConnectCam = false;
            Global.bConnectLight = Global.clsLight.IsOpen();
            if (Global.dicClsCam[CamInfo.CAM_1].IsOpen() && Global.dicClsCam[CamInfo.CAM_2].IsOpen()) Global.bConnectCam = true;
            
            //Global.bConnectPLC = Global.clsOmron.IsConnect;
            if (Global.dicClsMotion[CamInfo.CAM_1].IsConnect && Global.dicClsMotion[CamInfo.CAM_2].IsConnect) Global.bConnectAxisZ = true;

            labelAlarm.Text = "Alarm\r\n[" + Global.clsAlarmManager.CurrentAlarm.ToString()+"]";
            if (Global.clsAlarmManager.CurrentAlarm == AlarmCode.None)
                labelAlarm.BackColor = SystemColors.Control;
            else
                labelAlarm.BackColor = Color.Red;


            if (Global.bAutoMode)
            {
                btn_Auto_Manul.Text = "MANUAL";
                label_Mode.TextAlign = ContentAlignment.MiddleLeft;
                label_Mode.Text = "                       AUTO [ " + Global.clsAutoThread.CurrentStep.ToString()+" ]";
                label_Mode.BackColor = Color.Lime;
            }
            else
            {
                btn_Auto_Manul.Text = "AUTO";
                label_Mode.TextAlign = ContentAlignment.MiddleCenter;
                label_Mode.Text = "MANUAL";
                label_Mode.BackColor = Color.Yellow;
            }


            CheckConnect();
        }

        #region Title Move
        private void label_DateTime_MouseDown(object sender, MouseEventArgs e)
        {
            m_pointMouseDown = new Point(e.X, e.Y);
            Global.bMovePgm = true;
        }

        private void label_DateTime_MouseUp(object sender, MouseEventArgs e)
        {
            Global.bMovePgm = false;
        }

        private void label_DateTime_MouseMove(object sender, MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                Global.point = new Point(Left - (Right - m_pointMouseDown.X - Cursor.Position.X), Top - (label_DateTime.Top + m_pointMouseDown.Y - Cursor.Position.Y));
            }
        }

        private void label_DateTime_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Global.bMovePgm = true;
            Global.point = new Point(0,0);
        }
        #endregion

        #region Style
        private void FormDisposition()
        {
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            ControlBox = MinimizeBox = MaximizeBox = ShowInTaskbar = false;

            BackColor = Color.FromArgb(19, 50, 69);
        }

        private void SetStyle()
        {
            Color BtnColor = Color.FromArgb(38, 38, 38);
            Color TextColor = Color.White;
            Font font = new Font("Arial", 25, FontStyle.Bold);

            label_Title.ForeColor = TextColor;
            label_Title.BackColor = Color.FromArgb(19, 50, 69);
            label_Title.Font = new Font("Arial", 25, FontStyle.Bold); 

            label_Mode.ForeColor = Color.LimeGreen;
            label_Mode.BackColor = Color.Black;
            label_Mode.Font = new Font("Arial", 15, FontStyle.Bold);

            
            // 현재 시간 표시
            label_DateTime.ForeColor = TextColor;
            label_DateTime.Font = new Font("Arial", 15, FontStyle.Bold);

            label_AxisZ.ForeColor = label_Light.ForeColor = label_PLC.ForeColor = label_Cam.ForeColor = TextColor;
            
            // Version 
            label_Ver.ForeColor = TextColor;
            label_Ver.Font = new Font("Arial", 15, FontStyle.Bold);

            panel_AxisZ.BackColor = panel_Light.BackColor = panel_PLC.BackColor = panel_Cam.BackColor = Color.FromArgb(255, 65, 0);

            this.TopMost = true;
        }

        private void ToolTip()
        {
            ToolTip toolTip = new ToolTip();

            toolTip.AutoPopDelay = 5000;
            toolTip.InitialDelay = 100;
            toolTip.ReshowDelay = 100;
            toolTip.ShowAlways = true;
            toolTip.IsBalloon = true;

            //for(int n = 0; n < (int)ConnectModule.MAX; n++)
            //{
            //    toolTip.SetToolTip(this.m_panel_Connect[n], Global.dicToolTip[(ConnectModule)n]);
            //}
        }
        #endregion
        
        public void CheckConnect()
        {
            Color colorDisConnect = Color.FromArgb(255, 65, 0);

            if (bBeforAxisZ != Global.bConnectAxisZ) { panel_AxisZ.BackColor = Global.bConnectAxisZ ? Color.Lime : colorDisConnect; bBeforAxisZ = Global.bConnectAxisZ; }
            if (bBeforCam   != Global.bConnectCam  ) { panel_Cam  .BackColor = Global.bConnectCam   ? Color.Lime : colorDisConnect; bBeforCam   = Global.bConnectCam  ; }
            if (bBeforPLC   != Global.bConnectPLC  ) { panel_PLC  .BackColor = Global.bConnectPLC   ? Color.Lime : colorDisConnect; bBeforPLC   = Global.bConnectPLC  ; }
            if (bBeforLight != Global.bConnectLight) { panel_Light.BackColor = Global.bConnectLight ? Color.Lime : colorDisConnect; bBeforLight = Global.bConnectLight; }
        
        }

        private void btnAlarmClear_Click(object sender, EventArgs e)
        {
            if(Global.bAutoMode)
                Global.clsAlarmManager.AlarmClearReq = true;
            else
            {
                Global.clsAlarmManager.ClearAlarm();

                //PLC 출력 영역 모두 초기화
                Global.inforPLC.ClearOutput();
            }
        }

        private void btn_Auto_Manul_Click(object sender, EventArgs e)
        {
            if(Global.inforCam.bLive)
            {
                MessageBox.Show("Please turn off the Camera Live.", "Message", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Global.bAutoMode = !Global.bAutoMode;
        }

        private void btn_Minimum_Click(object sender, EventArgs e)
        {
            Global.formBase.WindowState = FormWindowState.Minimized;
        }

        private void btn_Exit_Click(object sender, EventArgs e)
        {
            // 설비가 START 일때는 강제로 알람 발생 후 종료
            if (MessageBox.Show("Do you want exit program ?", "Exit program", MessageBoxButtons.YesNo) != DialogResult.Yes) return;

            Global.logger[LogType.SYSTEM].Write("------========= Program End =========------");

            Environment.Exit(0);
        }
    }
}
