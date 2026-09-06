using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using uEye.Types;

namespace Vision_Align
{
    public partial class FormSetCam : Form
    {
        Timer m_timer = new Timer();

        Point m_pointMouseDown;
        int m_nCamNo = -1;


        public FormSetCam()
        {
            InitializeComponent();
        }

        #region Title Move
        private void label_Title_MouseDown(object sender, MouseEventArgs e)
        {
            m_pointMouseDown = new Point(e.X, e.Y);
        }

        private void label_Title_MouseMove(object sender, MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
                Location = new Point(Left - (m_pointMouseDown.X - e.X), Top - (m_pointMouseDown.Y - e.Y));
        }
        #endregion

        public void Initializ()
        {
            FormDisposition();
            SetStyle();

            cb_Mode.Items.AddRange(Enum.GetNames( typeof(CamSet)));
            cb_Mode.Items.RemoveAt(cb_Mode.Items.Count - 1); // CamSet Enum의 MAX 항목 제거

            m_timer.Interval = 500;
            m_timer.Tick += new EventHandler(OnTimer);
            
        }

        void OnTimer(object obj, EventArgs ea)
        {
            bool bTemp = true;
            Global.dicClsMotion[(CamInfo)m_nCamNo].UpdateStatus();

            label_Servo.BackColor = Global.inforMotion.bServo[m_nCamNo] ? Color.Lime : Color.White;
            label_Home .BackColor = Global.inforMotion.bHome[m_nCamNo] ? Color.Lime : Color.White;
            label_Move .BackColor = Global.inforMotion.bBasy[m_nCamNo] ? Color.Lime : Color.White;

            label_Stop   .BackColor = !Global.inforMotion.bBasy[m_nCamNo] ? Color.Red : Color.White;

            label_Limit_M.BackColor = Global.inforMotion.bLimitM[m_nCamNo] ? Color.Red : Color.White;
            label_Limit_P.BackColor = Global.inforMotion.bLimitP[m_nCamNo] ? Color.Red : Color.White;
            label_Alram  .BackColor = Global.inforMotion.bAlram[m_nCamNo] ? Color.Red : Color.White;

            label_Pos.Text = Global.dicClsMotion[(CamInfo)m_nCamNo].m_dCmdPos.ToString("F3");

            if (Global.bAutoMode)
                HideViewer();
        }


        public void ShowViewer(int nCamNo)
        {
            this.Location = new Point(1240, 60);

            this.Show();
            m_nCamNo = nCamNo;
            SetTitle(m_nCamNo);

            cb_Mode.SelectedIndex = 0;
            IsLoad(true, cb_Mode.SelectedIndex);

            m_timer.Start();

            Global.dicClsCam[(CamInfo)m_nCamNo].GetRangeExposure(out double dMin, out double dMax);
            nud_Exposure.Minimum = Convert.ToDecimal(dMin);
            nud_Exposure.Maximum = Convert.ToDecimal(dMax);

            label_AxisNo.Text = m_nCamNo.ToString();
        }

        public void HideViewer()
        {
            m_timer.Stop();
            this.Hide();
        }

        public void SetCamParam()
        {
            double dExposure = Global.CamSet_Param.dicExposure[(CamInfo)m_nCamNo][cb_Mode.SelectedIndex];
            int    nGain     = Global.CamSet_Param.dicGain    [(CamInfo)m_nCamNo][cb_Mode.SelectedIndex];
            double dGamma    = Global.CamSet_Param.dicGamma   [(CamInfo)m_nCamNo][cb_Mode.SelectedIndex];
            bool   bFlipX    = Global.CamSet_Param.dicFlipX   [(CamInfo)m_nCamNo][cb_Mode.SelectedIndex];
            bool   bFlipY    = Global.CamSet_Param.dicFlipY   [(CamInfo)m_nCamNo][cb_Mode.SelectedIndex];
            bool   bRotate   = Global.CamSet_Param.dicRotation[(CamInfo)m_nCamNo][cb_Mode.SelectedIndex];

            Global.dicClsCam[(CamInfo)m_nCamNo].SetParam(dExposure, nGain, dGamma, bFlipX, bFlipY, bRotate);
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
            //this.BackColor = Color.White;

            this.TopMost = true;

            // Cam Param 
            label_Title_Info.BackColor = Color.Black;

            // Light Param
            label_Title_Light.BackColor = Color.Black;

            trackBar_Ring.TickFrequency = trackBar_Spot.TickFrequency = 1;

            // Axis
            label_Title_Axis.BackColor = Color.FromArgb(65, 0, 65);

            // Mot status
            label_Title_Mot_Status.BackColor = Color.FromArgb(65, 0, 65);

            // Mot Ctrl
            label_Title_Mot_Ctrl.BackColor = Color.FromArgb(65, 0, 65);

            // Jog
            label_Title_Jog.BackColor = Color.FromArgb(65, 0, 65);
        }

        private void SetTitle(int nCamNo)
        {
            if (nCamNo == -1) return;

            label_Title.Text = string.Format("PRE_CAMERA CAM" + (nCamNo + 1).ToString());
        }

        #endregion

        #region Motion Ctrl Botton
        private void btn_Servo_On_Click(object sender, EventArgs e)
        {
            Global.dicClsMotion[(CamInfo)m_nCamNo].ServoOnOff(true);
        }

        private void btn_Servo_Off_Click(object sender, EventArgs e)
        {
            Global.dicClsMotion[(CamInfo)m_nCamNo].ServoOnOff(false);
        }

        private void btn_Home_Click(object sender, EventArgs e)
        {
            Global.dicClsMotion[(CamInfo)m_nCamNo].Home();
        }

        private void btn_Abs_Click(object sender, EventArgs e)
        {
            double dPos = Convert.ToDouble( nud_Abs.Value);
            double dSpeed = Convert.ToDouble( nud_Speed.Value);
            double dAcc = dSpeed * 3;
            double dDec = dSpeed * 3;
            
            Global.dicClsMotion[(CamInfo)m_nCamNo].Move(dPos, dSpeed, dAcc, dDec);
        }

        private void btn_Inc_Click(object sender, EventArgs e)
        {
            double dPos = Global.dicClsMotion[(CamInfo)m_nCamNo].m_dActPos + Convert.ToDouble(nud_Inc.Value);
            double dSpeed = Convert.ToDouble(nud_Speed.Value);
            double dAcc = dSpeed * 3;
            double dDec = dSpeed * 3;

            Global.dicClsMotion[(CamInfo)m_nCamNo].Move(dPos, dSpeed, dAcc, dDec);
        }

        private void btn_Stop_Click(object sender, EventArgs e)
        {
            Global.dicClsMotion[(CamInfo)m_nCamNo].SlowStop();
        }

        private void btn_Jog_P_MouseDown(object sender, MouseEventArgs e)
        {
            // Plus 가 아래
            Global.dicClsMotion[(CamInfo)m_nCamNo].JogMove(0);
        }

        private void btn_Jog_P_MouseUp(object sender, MouseEventArgs e)
        {
            Global.dicClsMotion[(CamInfo)m_nCamNo].SlowStop();
        }

        private void btn_Jog_M_MouseDown(object sender, MouseEventArgs e)
        {
            Global.dicClsMotion[(CamInfo)m_nCamNo].JogMove(1);
        }

        private void btn_Jog_M_MouseUp(object sender, MouseEventArgs e)
        {
            Global.dicClsMotion[(CamInfo)m_nCamNo].SlowStop();
        }

        #endregion

        #region Light 
        private void trackBar_Spot_Scroll(object sender, EventArgs e)
        {
            nud_Spot.Value = trackBar_Spot.Value;
            int nCh = m_nCamNo == 0 ? 2 : 0;
            int nValue = Convert.ToInt32(nud_Spot.Value);
            Global.clsLight.LightValue(nCh, nValue);
        }

        private void trackBar_Ring_Scroll(object sender, EventArgs e)
        {
            nud_Ring.Value = trackBar_Ring.Value;
            int nCh = m_nCamNo == 0 ? 3 : 1;
            int nValue = Convert.ToInt32(nud_Ring.Value);
            Global.clsLight.LightValue(nCh, nValue);
        }

        private void nud_Spot_ValueChanged(object sender, EventArgs e)
        {
            trackBar_Spot.Value = Convert.ToInt32(nud_Spot.Value);
            int nCh = m_nCamNo == 0 ? 2 : 0;
            int nValue = Convert.ToInt32(nud_Spot.Value);
            Global.clsLight.LightValue(nCh, nValue);
        }

        private void nud_Ring_ValueChanged(object sender, EventArgs e)
        {
            trackBar_Ring.Value = Convert.ToInt32(nud_Ring.Value);
            int nCh = m_nCamNo == 0 ? 3 : 1;
            int nValue = Convert.ToInt32(nud_Ring.Value);
            Global.clsLight.LightValue(nCh, nValue);
        }

        private void cb_Spot_CheckedChanged(object sender, EventArgs e)
        {
            int nCh = m_nCamNo == 0 ? 2 : 0;
            Global.clsLight.LightOnOff(cb_Spot.Checked, nCh); 
        }

        private void cb_Ring_CheckedChanged(object sender, EventArgs e)
        {
            int nCh = m_nCamNo == 0 ? 3 : 1;
            Global.clsLight.LightOnOff(cb_Ring.Checked, nCh);
        }
        #endregion

        #region CAMERA
        private void cb_Flip_Row_CheckedChanged(object sender, EventArgs e)
        {
            Global.dicClsCam[(CamInfo)m_nCamNo].MirrorRow(cb_Flip_Row.Checked);
        }

        private void cb_Flip_Col_CheckedChanged(object sender, EventArgs e)
        {
            Global.dicClsCam[(CamInfo)m_nCamNo].MirrorCol(cb_Flip_Col.Checked);
        }

        private void cb_Rotate_CheckedChanged(object sender, EventArgs e)
        {
            Global.dicClsCam[(CamInfo)m_nCamNo].MirrorRow(cb_Rotate.Checked);
            Global.dicClsCam[(CamInfo)m_nCamNo].MirrorCol(cb_Rotate.Checked);
        }

        private void nud_Exposure_ValueChanged(object sender, EventArgs e)
        {
            double dExposure = Convert.ToDouble(nud_Exposure.Value);
            Global.dicClsCam[(CamInfo)m_nCamNo].SetExposure(dExposure);
        }

        private void nud_Gain_ValueChanged(object sender, EventArgs e)
        {
            int nGain = Convert.ToInt32(nud_Gain.Value);
            Global.dicClsCam[(CamInfo)m_nCamNo].SetGain(nGain);
        }

        private void nud_Gamma_ValueChanged(object sender, EventArgs e)
        {
            double dGamma = Convert.ToDouble(nud_Gamma.Value);
            Global.dicClsCam[(CamInfo)m_nCamNo].SetGamma(dGamma);
        }
        #endregion

        private void IsLoad(bool IsLoad, int nModeIndex)
        {
            try {
                if (IsLoad)
                {
                    //Global.CamSet_Param = Global.IsLoad_CamSet_List(IsLoad, Global.CamSet_Param);
                    
                    label_Exposure.Text = (nud_Exposure.Value = (decimal)Global.CamSet_Param.dicExposure[(CamInfo)m_nCamNo][nModeIndex]).ToString();
                    label_Gamma   .Text = (nud_Gamma   .Value = (decimal)Global.CamSet_Param.dicGamma   [(CamInfo)m_nCamNo][nModeIndex]).ToString();
                    label_Gain    .Text = (nud_Gain    .Value = (decimal)Global.CamSet_Param.dicGain    [(CamInfo)m_nCamNo][nModeIndex]).ToString();

                    cb_Rotate  .Checked = Global.CamSet_Param.dicRotation[(CamInfo)m_nCamNo][nModeIndex];
                    cb_Flip_Row.Checked = Global.CamSet_Param.dicFlipY   [(CamInfo)m_nCamNo][nModeIndex];
                    cb_Flip_Col.Checked = Global.CamSet_Param.dicFlipX   [(CamInfo)m_nCamNo][nModeIndex];

                    label_Spot.Text = (nud_Spot.Value = trackBar_Spot.Value = Global.CamSet_Param.dicLightSpot[(CamInfo)m_nCamNo][nModeIndex]).ToString();
                    label_Ring.Text = (nud_Ring.Value = trackBar_Ring.Value = Global.CamSet_Param.dicLightRing[(CamInfo)m_nCamNo][nModeIndex]).ToString();

                    label_Set_Z_Pos.Text = (nud_Set_Z_Pos.Value = (decimal)Global.CamSet_Param.dicZ_Pos[(CamInfo)m_nCamNo][nModeIndex]).ToString();
                    
                    nud_Speed.Value = (decimal)Global.CamSet_Param.dicZ_Speed[(CamInfo)m_nCamNo][nModeIndex];
                }
                else
                {
                    Global.CamSet_Param.dicExposure[(CamInfo)m_nCamNo][nModeIndex] = Convert.ToDouble(nud_Exposure.Value);
                    Global.CamSet_Param.dicGamma   [(CamInfo)m_nCamNo][nModeIndex] = Convert.ToDouble(nud_Gamma.Value);
                    Global.CamSet_Param.dicGain    [(CamInfo)m_nCamNo][nModeIndex] = Convert.ToInt32(nud_Gain.Value);

                    Global.CamSet_Param.dicRotation[(CamInfo)m_nCamNo][nModeIndex] = cb_Rotate.Checked;
                    Global.CamSet_Param.dicFlipY   [(CamInfo)m_nCamNo][nModeIndex] = cb_Flip_Row.Checked;
                    Global.CamSet_Param.dicFlipX   [(CamInfo)m_nCamNo][nModeIndex] = cb_Flip_Col.Checked;

                    Global.CamSet_Param.dicLightSpot[(CamInfo)m_nCamNo][nModeIndex] = Convert.ToInt32(nud_Spot.Value);
                    Global.CamSet_Param.dicLightRing[(CamInfo)m_nCamNo][nModeIndex] = Convert.ToInt32(nud_Ring.Value);

                    Global.CamSet_Param.dicZ_Pos[(CamInfo)m_nCamNo][nModeIndex] = Convert.ToDouble(nud_Set_Z_Pos.Value);

                    Global.CamSet_Param.dicZ_Speed[(CamInfo)m_nCamNo][nModeIndex] = Convert.ToDouble(nud_Speed.Value);

                    Global.CamSet_Param = Global.IsLoad_List(IsLoad, Global.CamSet_Param);
                }
            } catch (Exception e)
            {

            }


            SetCamParam();
        }

        private void btn_Close_Click(object sender, EventArgs e)
        {
            HideViewer();
        }

        private void btn_Save_Click(object sender, EventArgs e)
        {
            IsLoad(false, cb_Mode.SelectedIndex);
        }

        private void cb_Mode_SelectedIndexChanged(object sender, EventArgs e)
        {
            IsLoad(true, cb_Mode.SelectedIndex);
        }

        private void btnSavedPosMove_Click(object sender, EventArgs e)
        {
            double dPos = Global.CamSet_Param.dicZ_Pos[(CamInfo)m_nCamNo][cb_Mode.SelectedIndex];
            double dSpeed = Global.CamSet_Param.dicZ_Speed[(CamInfo)m_nCamNo][cb_Mode.SelectedIndex];
            if (dSpeed < 1)
                dSpeed = 1000;
            double dAcc = dSpeed * 3;
            double dDec = dSpeed * 3;

            Global.dicClsMotion[(CamInfo)m_nCamNo].Move(dPos, dSpeed, dAcc, dDec);
        }
    }
}
