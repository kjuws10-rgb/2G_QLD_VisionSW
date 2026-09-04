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
    public partial class FormMain_SubCalibration : Form
    {
        private bool m_bPrevCalibrationMode = false;

        public FormMain_SubCalibration()
        {
            InitializeComponent();

            Global.clsAutoThread.ProcessStatusMsg += ClsAutoThread_ProcessStatusMsg;

        }

        private void ClsAutoThread_ProcessStatusMsg(object sender, string msg)
        {
            DateTime time = DateTime.Now;
            Task.Run(() =>
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(() =>
                    {
                        if (!msg.Contains("[CALIBRATION_MODE]"))
                            return;
                        if (msg.Contains("[CALIBRATION_MODE] Start"))
                            listBoxCalStatus.Items.Clear();
                        if (listBoxCalStatus.Items.Count > 200)
                        {
                            listBoxCalStatus.Items.Remove(0);
                        }
                        listBoxCalStatus.Items.Add($"[{time.ToString("yyMMdd-HHmmss.fff")}] {msg}");
                        if(listBoxCalStatus.Items.Count > 0)
                            listBoxCalStatus.SelectedIndex = listBoxCalStatus.Items.Count - 1;
                    }));
                }
            });
        }

        public void Initializ()
        {
            FormDisposition();
            LoadParamToUI();

            tmrStatus.Enabled = true;
        }

        #region Style
        private void FormDisposition()
        {
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            ControlBox = MinimizeBox = MaximizeBox = ShowInTaskbar = false;
        }


        #endregion

        #region Parameter Load/Save
        /// <summary>
        /// Config 값을 UI에 표시
        /// </summary>
        public void LoadParamToUI()
        {
            // CALIBRATION_LIST
            var calParam = Global.Calibration_Param;

            // Mark Score Limit (%)
            nud_Mask_Score_Limit.Value = (decimal)(calParam.MarkScoreLimit * 100);
            nud_AverageImageCount.Value = (decimal)(calParam.AverageImageCount);

            // Calibration Move Offset
            nud_OffsetXY.Value = (decimal)calParam.MoveOffsetXY;
            nud_OffsetTh.Value = (decimal)calParam.MoveOffsetAngle;
            nud_OffsetAngleCount.Value = (decimal)calParam.MoveOffsetAngleCount;

            // Calibration Move Offset
            nud_StartX.Value = (decimal)calParam.StartPositionX;
            nud_StartY.Value = (decimal)calParam.StartPositionY;
            nud_StartT.Value = (decimal)calParam.StartPositionAngle;

            // Camera 1 Calibration Result
            txtCam1PixelResolutionX.Text = calParam.CameraPixelResolutionX[(int)CamInfo.CAM_1].ToString("F6");
            txtCam1PixelResolutionY.Text = calParam.CameraPixelResolutionY[(int)CamInfo.CAM_1].ToString("F6");
            txtCam1Tilt.Text = calParam.CameraTilt[(int)CamInfo.CAM_1].ToString("F6");
            txtCam1OffsetX.Text = calParam.CameraCoordinateOffsetX[(int)CamInfo.CAM_1].ToString("F3");
            txtCam1OffsetY.Text = calParam.CameraCoordinateOffsetY[(int)CamInfo.CAM_1].ToString("F3");

            // Camera 2 Calibration Result
            txtCam2PixelResolutionX.Text = calParam.CameraPixelResolutionX[(int)CamInfo.CAM_2].ToString("F6");
            txtCam2PixelResolutionY.Text = calParam.CameraPixelResolutionY[(int)CamInfo.CAM_2].ToString("F6");
            txtCam2Tilt.Text = calParam.CameraTilt[(int)CamInfo.CAM_2].ToString("F6");
            txtCam2OffsetX.Text = calParam.CameraCoordinateOffsetX[(int)CamInfo.CAM_2].ToString("F3");
            txtCam2OffsetY.Text = calParam.CameraCoordinateOffsetY[(int)CamInfo.CAM_2].ToString("F3");

            // Fiducial Mark Distance (μm)
            txtPiducialMarkDistance.Text = calParam.MaskHoleDistanceC.ToString("F3");

        }

        /// <summary>
        /// UI 값을 Config에 저장
        /// </summary>
        private void SaveParamFromUI()
        {
            try
            {
                // CALIBRATION_LIST
                var calParam = Global.Calibration_Param;

                // Mark Score Limit (%) - 두 카메라 동일 값 사용
                calParam.MarkScoreLimit = (double)nud_Mask_Score_Limit.Value / 100.0;
                calParam.AverageImageCount = (int)nud_AverageImageCount.Value;

                // Calibration Move Offset
                calParam.MoveOffsetXY = (double)nud_OffsetXY.Value;
                calParam.MoveOffsetAngle = (double)nud_OffsetTh.Value;
                calParam.MoveOffsetAngleCount = (int)nud_OffsetAngleCount.Value;

                // Calibration Start Position
                calParam.StartPositionX      = (double)nud_StartX.Value;
                calParam.StartPositionY      = (double)nud_StartY.Value;
                calParam.StartPositionAngle  = (double)nud_StartT.Value;

                // UVW_Info - Motion Limit
                var uvwInfo = Global.inforUvw;

                // JSON 파일로 저장
                Global.Calibration_Param = Global.IsLoad_List(false, Global.Calibration_Param);
                Global.inforUvw = Global.IsLoad_List(false, Global.inforUvw);

                MessageBox.Show("Parameter saved successfully.", "Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save parameter.\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Btn_SetParam_Click(object sender, EventArgs e)
        {
            SaveParamFromUI();
        }
        #endregion

        public void ShowViewer()
        {
            LoadParamToUI();
            this.Show();
        }

        public void HideViewer()
        {
            this.Hide();
        }

        private void tmrStatus_Tick(object sender, EventArgs e)
        {
            btn_Auto_Cal.BackColor = Global.inforPLC.InCalibrationRequest ? Color.Lime : Color.FromArgb(45,45,48);
            if (btn_Auto_Cal.BackColor == Color.Lime)
                btn_Auto_Cal.ForeColor = Color.Black;
            else
                btn_Auto_Cal.ForeColor = Color.White;

            // Calibration Mode가 True → False로 바뀌면 UI Refresh
            if (m_bPrevCalibrationMode && !Global.inforPLC.InCalibrationRequest)
            {
                LoadParamToUI();
            }
            m_bPrevCalibrationMode = Global.inforPLC.InCalibrationRequest;

            SetUVW();
        }

        private void chbManualMode_CheckedChanged(object sender, EventArgs e)
        {
            Global.bCalibrationManualMode = chbManualMode.Checked;
            btnManualMove.Enabled = Global.bCalibrationManualMode;
        }

        private void btnManualMove_Click(object sender, EventArgs e)
        {
            Global.clsUvwStage.Move((double)numManualX.Value, (double)numManualY.Value, (double)numManualT.Value);
        }

        private void SetUVW()
        {
            double currentX, currentY, currentAngle;
            ClsAlgorithm.StageCalInverse(Global.inforPLC.InStageCurrentU, Global.inforPLC.InStageCurrentW, Global.inforPLC.InStageCurrentV, out currentX, out currentY, out currentAngle);
            label_OffsetX.Text = string.Format(currentX.ToString("F3"));
            label_OffsetY.Text = string.Format(currentY.ToString("F3"));
            label_OffsetT.Text = string.Format(currentAngle.ToString("F6"));
        }
    }
}
