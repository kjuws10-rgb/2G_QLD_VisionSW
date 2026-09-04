using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Vision_Align._2.FormViewer
{
    public partial class FormIO : Form
    {
        Timer m_Timer = new Timer();

        public FormIO()
        {
            InitializeComponent();
            Global.clsOmron.ChangedIoMsg += ClsOmron_ChangedIoMsg;
        }

        private void ClsOmron_ChangedIoMsg(object sender, string msg)
        {
            DateTime time = DateTime.Now;
            Action updateUi = () =>
            {
                if (!checkBox_AiStage.Checked && msg.Contains("[AI"))
                    return;
                string message = $"[{time.ToString("yyMMdd-HHmmss.fff")}] {msg}";

                if (checkBox_AliveSignal.Checked || !msg.Contains(DI_STAGE_BIT.ALIVE.ToString()))
                {
                    if (listBoxPlcIoLog.Items.Count > 200)
                        listBoxPlcIoLog.Items.Remove(0);
                    listBoxPlcIoLog.Items.Add(message);
                    listBoxPlcIoLog.SelectedIndex = listBoxPlcIoLog.Items.Count - 1;
                }

                Global.logger[LogType.PLC].Write(msg);
            };

            try
            {
                if (IsDisposed || Disposing || !IsHandleCreated)
                    return;

                if (InvokeRequired)
                    BeginInvoke(updateUi);
                else
                    updateUi();
            }
            catch (Exception ex)
            {
                CrashDiagnostics.ReportRecoverableException("PLC I/O UI update", ex);
            }
        }

        public void Initializ(Color? color = null)
        {
            FormDisposition(color);
            SetStyle();

            m_Timer.Interval = 100;
            m_Timer.Tick += new EventHandler(OnTimer);
            m_Timer.Start();
        }

        #region Style
        private void FormDisposition(Color? color)
        {
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            ControlBox = MinimizeBox = MaximizeBox = ShowInTaskbar = false;
        }

        private void SetStyle()
        {
            Font font = new Font("Arial", 9, FontStyle.Bold);
        }
        #endregion

        void OnTimer(object obj, EventArgs ea)
        {
            // DI_Stage (Input from PLC) - 초록색 계열
            panel_DI_Alive.BackColor = Global.clsOmron.DI_Bit[(int)DI_STAGE_BIT.ALIVE] ? Color.Lime : Color.DarkGreen;
            panel_DI_Ready.BackColor = Global.clsOmron.DI_Bit[(int)DI_STAGE_BIT.READY] ? Color.Lime : Color.DarkGreen;
            panel_DI_Alram.BackColor = Global.clsOmron.DI_Bit[(int)DI_STAGE_BIT.ALRAM] ? Color.Lime : Color.DarkGreen;
            panel_DI_Stage_Enable.BackColor = Global.clsOmron.DI_Bit[(int)DI_STAGE_BIT.STAGE_ENABLE] ? Color.Lime : Color.DarkGreen;

            panel_PreAlign_Req.BackColor = Global.clsOmron.DI_Bit[(int)DI_STAGE_BIT.PREALIGN_REQ] ? Color.Lime : Color.DarkGreen;
            panel_Contact_Req.BackColor = Global.clsOmron.DI_Bit[(int)DI_STAGE_BIT.CONTACT_REQ] ? Color.Lime : Color.DarkGreen;
            panel_ModelChange_Req.BackColor = Global.clsOmron.DI_Bit[(int)DI_STAGE_BIT.MASK_MODEL_REQ] ? Color.Lime : Color.DarkGreen;
            panel_Calibration_Req .BackColor = Global.clsOmron.DI_Bit[(int)DI_STAGE_BIT.CALIBRATION_REQ] ? Color.Lime : Color.DarkGreen;

            panel_UVW_Reply.BackColor = Global.clsOmron.DI_Bit[(int)DI_STAGE_BIT.UVW_MOVE_REPLY] ? Color.Lime : Color.DarkGreen;
            panel_UVW_OK.BackColor = Global.clsOmron.DI_Bit[(int)DI_STAGE_BIT.UVW_MOVE_OK] ? Color.Lime : Color.DarkGreen;
            panel_UVW_NG.BackColor = Global.clsOmron.DI_Bit[(int)DI_STAGE_BIT.UVW_MOVE_NG] ? Color.Lime : Color.DarkGreen;

            // DO_Vision (Output to PLC) - 빨간색 계열
            panel_DO_Alive.BackColor = Global.clsOmron.DO_Bit[(int)DO_VISION_BIT.ALIVE] ? Color.Red : Color.DarkRed;
            panel_DO_Ready.BackColor = Global.clsOmron.DO_Bit[(int)DO_VISION_BIT.READY] ? Color.Red : Color.DarkRed;
            panel_DO_Alram.BackColor = Global.clsOmron.DO_Bit[(int)DO_VISION_BIT.ALARM] ? Color.Red : Color.DarkRed;
            panel_DO_Busy.BackColor = Global.clsOmron.DO_Bit[(int)DO_VISION_BIT.BUSY] ? Color.Red : Color.DarkRed;
            panel_DO_Cancel.BackColor = Global.clsOmron.DO_Bit[(int)DO_VISION_BIT.CANCEL] ? Color.Red : Color.DarkRed;

            panel_PreAlign_Reply.BackColor = Global.clsOmron.DO_Bit[(int)DO_VISION_BIT.PREALIGN_REPLY] ? Color.Red : Color.DarkRed;
            panel_PreAlign_OK.BackColor = Global.clsOmron.DO_Bit[(int)DO_VISION_BIT.PREALIGN_OK] ? Color.Red : Color.DarkRed;
            panel_PreAlign_NG.BackColor = Global.clsOmron.DO_Bit[(int)DO_VISION_BIT.PREALIGN_NG] ? Color.Red : Color.DarkRed;

            panel_Contact_Reply.BackColor = Global.clsOmron.DO_Bit[(int)DO_VISION_BIT.CONTACT_REPLY] ? Color.Red : Color.DarkRed;
            panel_Contact_OK.BackColor = Global.clsOmron.DO_Bit[(int)DO_VISION_BIT.CONTACT_OK] ? Color.Red : Color.DarkRed;
            panel_Contact_NG.BackColor = Global.clsOmron.DO_Bit[(int)DO_VISION_BIT.CONTACT_NG] ? Color.Red : Color.DarkRed;

            panel_UVW_Req.BackColor = Global.clsOmron.DO_Bit[(int)DO_VISION_BIT.UVW_MOVE_REQ] ? Color.Red : Color.DarkRed;

            panel_Calibration_Rpy.BackColor = Global.clsOmron.DI_Bit[(int)DO_VISION_BIT.CALIBRATION_REPLY] ? Color.Red : Color.DarkRed;
            panel_Calibration_Stop.BackColor = Global.clsOmron.DI_Bit[(int)DO_VISION_BIT.CALIBRATION_STOP] ? Color.Red : Color.DarkRed;

            panel_Model_Reply.BackColor = Global.clsOmron.DO_Bit[(int)DO_VISION_BIT.MODEL_REPLY] ? Color.Red : Color.DarkRed;
            panel_Model_OK.BackColor = Global.clsOmron.DO_Bit[(int)DO_VISION_BIT.MODEL_OK] ? Color.Red : Color.DarkRed;
            panel_Model_NG.BackColor = Global.clsOmron.DO_Bit[(int)DO_VISION_BIT.MODEL_NG] ? Color.Red : Color.DarkRed;

            // DI_Stage (Word from PLC)
            lbl_DI_RetryCount_Value.Text = Global.clsOmron.DI_Word[(int)DI_STAGE_WORD.PRE_ALIGN_RETRY_COUNT].ToString();
            lbl_DI_CurrentRetryCount_Value.Text = Global.clsOmron.DI_Word[(int)DI_STAGE_WORD.PRE_ALIGN_RETRY_CURRENT_COUNT].ToString();

            // DO_Vision (Word to PLC)
            lbl_DO_Alarm_Value.Text = Global.clsOmron.DO_Word[(int)DO_VISION_WORD.ALARM_CODE].ToString();

            // AI_Stage (Current Position from PLC)
            lbl_AI_U_Value.Text = Global.inforPLC.InStageCurrentU.ToString("F2");
            lbl_AI_V_Value.Text = Global.inforPLC.InStageCurrentV.ToString("F2");
            lbl_AI_W_Value.Text = Global.inforPLC.InStageCurrentW.ToString("F2");

            // AO_Vision (Target Position to PLC)
            lbl_AO_U_Value.Text = Global.inforPLC.OutStageTargetU.ToString("F2");
            lbl_AO_V_Value.Text = Global.inforPLC.OutStageTargetV.ToString("F2");
            lbl_AO_W_Value.Text = Global.inforPLC.OutStageTargetW.ToString("F2");

            // AO_Vision - PreAlign Data
            lbl_AO_PreAlign_Time_Value.Text = Global.clsOmron.WordToFloat( AO_VISION.PRE_ALIGN_TIME).ToString("F2");
            lbl_AO_PreAlign_ErrX_Value.Text = Global.clsOmron.WordToFloat(AO_VISION.PRE_ALIGN_ERROR_X).ToString("F2");
            lbl_AO_PreAlign_ErrY_Value.Text = Global.clsOmron.WordToFloat(AO_VISION.PRE_ALIGN_ERROR_Y).ToString("F2");
            lbl_AO_PreAlign_ErrT_Value.Text = Global.clsOmron.WordToFloat(AO_VISION.PRE_ALIGN_ERROR_T).ToString("F2");

            // AO_Vision - ContactAlign Data
            lbl_AO_Contact_Time_Value.Text = Global.clsOmron.WordToFloat(AO_VISION.CONTACT_ALIGN_TIME).ToString("F2");
            lbl_AO_Contact_ErrX_Value.Text = Global.clsOmron.WordToFloat(AO_VISION.CONTACT_ALIGN_ERROR_X).ToString("F2");
            lbl_AO_Contact_ErrY_Value.Text = Global.clsOmron.WordToFloat(AO_VISION.CONTACT_ALIGN_ERROR_Y).ToString("F2");
            lbl_AO_Contact_ErrT_Value.Text = Global.clsOmron.WordToFloat(AO_VISION.CONTACT_ALIGN_ERROR_T).ToString("F2");
        }

        public void ShowViewr()
        {
            Show();
        }

        public void HideViewer()
        {
            Hide();
        }
    }
}
