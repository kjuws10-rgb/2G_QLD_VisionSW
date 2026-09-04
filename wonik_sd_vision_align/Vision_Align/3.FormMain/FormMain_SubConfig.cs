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
    public partial class FormMain_SubConfig : Form
    {
        public FormMain_SubConfig()
        {
            InitializeComponent();
        }

        public void Initializ()
        {
            FormDisposition();
            SetStyle();
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
            for(int n = 0; n < 10; n++)
            {
                cb_Comport.Items.Add("COM" + (n + 1).ToString());
            }

            cb_Comport.SelectedIndex = 0;
            cb_BaudRate.SelectedIndex = 0;
        }
        #endregion

        public void ShowViewer()
        {
            IsLoad(true);
            this.Show();
        }

        public void HideViewer()
        {
            this.Hide();
        }

        private void IsLoad(bool IsLoad)
        {
            if(IsLoad)
            {
                cb_Comport.SelectedIndex = Global.Serial_Param.nPort ;
                for(int n = 0; n < cb_BaudRate.Items.Count; n++)
                {
                    if (Global.Serial_Param.nbaud.ToString() == cb_BaudRate.Items[n].ToString())
                    {
                        cb_BaudRate.SelectedIndex = n;
                        break;
                    }
                }

                nud_Score_Limit_Mark_0 .Value = (decimal)(Global.PreConfig_Param.dLimitMark * 100.0);
                nud_Score_Limit_Glass_0.Value = (decimal)(Global.PreConfig_Param.dLimitGlass * 100.0);
                nud_Pre_XY         .Value = (decimal)Global.PreConfig_Param.dPreAlignXY   ;
                nud_Pre_T          .Value = (decimal)Global.PreConfig_Param.dPreAlignTh;
                nud_Contact_XY.Value = (decimal)Global.PreConfig_Param.dContactAlignXY;
                nud_Contact_T.Value = (decimal)Global.PreConfig_Param.dContactAlignTh;

                nud_Pos_Limit_X1.Value = (decimal)Global.PreConfig_Param.dUVW_LimitPosX1; nud_Pos_Limit_Y1.Value = (decimal)Global.PreConfig_Param.dUVW_LimitPosY1;
                nud_Pos_Limit_X2.Value = (decimal)Global.PreConfig_Param.dUVW_LimitPosX2; nud_Pos_Limit_Y2.Value = (decimal)Global.PreConfig_Param.dUVW_LimitPosY2;
                nud_Neg_Limit_X1.Value = (decimal)Global.PreConfig_Param.dUVW_LimitNegX1; nud_Neg_Limit_Y1.Value = (decimal)Global.PreConfig_Param.dUVW_LimitNegY1;
                nud_Neg_Limit_X2.Value = (decimal)Global.PreConfig_Param.dUVW_LimitNegX2; nud_Neg_Limit_Y2.Value = (decimal)Global.PreConfig_Param.dUVW_LimitNegY2;

                nud_Offset_X1.Value = (decimal)Global.PreConfig_Param.dUVW_OffsetX1; nud_Offset_Y1.Value = (decimal)Global.PreConfig_Param.dUVW_OffsetY1;
                nud_Offset_X2.Value = (decimal)Global.PreConfig_Param.dUVW_OffsetX2; nud_Offset_Y2.Value = (decimal)Global.PreConfig_Param.dUVW_OffsetY2;

                nud_AverageImageCount.Value = (decimal)Global.PreConfig_Param.iPreAlignMaskAverageImageCount;

                cb_Always_Live.Checked = Global.PreConfig_Param.bAlways_Live;
            } 
            else
            {
                Global.Serial_Param.nPort = cb_Comport.SelectedIndex;
                Global.Serial_Param.nbaud = Convert.ToInt32( cb_BaudRate.Items[cb_BaudRate.SelectedIndex].ToString());

                Global.Serial_Param = Global.IsLoad_List(IsLoad, Global.Serial_Param);

                Global.PreConfig_Param.dLimitMark  = Convert.ToDouble(nud_Score_Limit_Mark_0 .Value) / 100.0;   
                Global.PreConfig_Param.dLimitGlass = Convert.ToDouble(nud_Score_Limit_Glass_0.Value) / 100.0;
                Global.PreConfig_Param.dPreAlignXY = Convert.ToDouble(nud_Pre_XY.Value);
                Global.PreConfig_Param.dPreAlignTh = Convert.ToDouble(nud_Pre_T.Value);
                Global.PreConfig_Param.dContactAlignXY = Convert.ToDouble(nud_Contact_XY.Value);
                Global.PreConfig_Param.dContactAlignTh = Convert.ToDouble(nud_Contact_T.Value);

                Global.PreConfig_Param.dUVW_LimitPosX1 = Convert.ToDouble(nud_Pos_Limit_X1.Value); Global.PreConfig_Param.dUVW_LimitPosY1 = Convert.ToDouble(nud_Pos_Limit_Y1.Value);
                Global.PreConfig_Param.dUVW_LimitPosX2 = Convert.ToDouble(nud_Pos_Limit_X2.Value); Global.PreConfig_Param.dUVW_LimitPosY2 = Convert.ToDouble(nud_Pos_Limit_Y2.Value);
                Global.PreConfig_Param.dUVW_LimitNegX1 = Convert.ToDouble(nud_Neg_Limit_X1.Value); Global.PreConfig_Param.dUVW_LimitNegY1 = Convert.ToDouble(nud_Neg_Limit_Y1.Value);
                Global.PreConfig_Param.dUVW_LimitNegX2 = Convert.ToDouble(nud_Neg_Limit_X2.Value); Global.PreConfig_Param.dUVW_LimitNegY2 = Convert.ToDouble(nud_Neg_Limit_Y2.Value);

                Global.PreConfig_Param.dUVW_OffsetX1 = Convert.ToDouble(nud_Offset_X1.Value); Global.PreConfig_Param.dUVW_OffsetY1 = Convert.ToDouble(nud_Offset_Y1.Value);
                Global.PreConfig_Param.dUVW_OffsetX2 = Convert.ToDouble(nud_Offset_X2.Value); Global.PreConfig_Param.dUVW_OffsetY2 = Convert.ToDouble(nud_Offset_Y2.Value);

                Global.PreConfig_Param.iPreAlignMaskAverageImageCount = Convert.ToInt32(nud_AverageImageCount.Value);

                Global.PreConfig_Param.bAlways_Live = cb_Always_Live.Checked;

                Global.PreConfig_Param = Global.IsLoad_List(IsLoad, Global.PreConfig_Param);
            }
        }

        private void btn_Save_Click(object sender, EventArgs e)
        {
            IsLoad(false);
        }
    }
}
