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
    public partial class FormMain : Form
    {
        FormMain_SubInfo formInfo = new FormMain_SubInfo();
        FormMain_SubCalibration formCalibration = new FormMain_SubCalibration();
        FormMain_SubConfig formConfig = new FormMain_SubConfig();

        Timer m_timer = new Timer();

        Sub_View curr_View = Sub_View.MAIN;

        enum Sub_View
        {
            MAIN = 0,
            CALIBRATION = 1,
            CONFIG = 2,
            MAX
        }

        public FormMain()
        {
            InitializeComponent();
        }

        public void Initializ(Color? color = null)
        {
            FormDisposition(color);
            SetStyle();
            CrateSubView();

            m_timer.Interval = 500;
            m_timer.Tick += new EventHandler(OnTimer);
            m_timer.Start();
        }

        void OnTimer(object obj, EventArgs ea)
        {
            if(Global.bAutoMode && curr_View != Sub_View.MAIN && curr_View != Sub_View.CALIBRATION)
            {
                SubViewChange(Sub_View.MAIN);
            }
        }

        #region Style
        private void FormDisposition(Color? color )
        {
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            ControlBox = MinimizeBox = MaximizeBox = ShowInTaskbar = false;
        }

        private void SetStyle()
        {
            Font font = new Font("Arial", 9, FontStyle.Bold);
            btn_PreMain.Font = btn_PreCalibration.Font = btn_PreConfig.Font = font;
        }

        private void CrateSubView()
        {
            formCalibration.TopLevel = formInfo.TopLevel = formConfig.TopLevel = false;

            panel_SubMain.Controls.Clear();
            panel_SubMain.Controls.Add(formInfo);
            panel_SubMain.Controls.Add(formCalibration);
            panel_SubMain.Controls.Add(formConfig);

            formInfo.Dock = DockStyle.Fill;
            formInfo.Initializ();

            formCalibration.Dock = DockStyle.Fill;
            formCalibration.Initializ();

            formConfig.Dock = DockStyle.Fill;
            formConfig.Initializ();

            


            // Display
            panel_Display1.Controls.Clear();
            panel_Display1.Controls.Add(Global.formDisplay[0]);

            Global.formDisplay[0].Show();
            Global.formDisplay[0].Dock = DockStyle.Fill;

            panel_Display2.Controls.Clear();
            panel_Display2.Controls.Add(Global.formDisplay[1]);

            Global.formDisplay[1].Show();
            Global.formDisplay[1].Dock = DockStyle.Fill;

            // Pre Review

            panel_PreView1.Controls.Clear();
            panel_PreView1.Controls.Add(Global.formPreView[0]);

            Global.formPreView[0].Show();
            Global.formPreView[0].Dock = DockStyle.Fill;

            panel_PreView2.Controls.Clear();
            panel_PreView2.Controls.Add(Global.formPreView[1]);

            Global.formPreView[1].Show();
            Global.formPreView[1].Dock = DockStyle.Fill;

        }
        #endregion

        public void ShowViewr()
        {
            Show();
            SubViewChange(Sub_View.MAIN);
        }

        public void HideViewer()
        {
            Hide();
        }

        private void btn_PreMain_Click(object sender, EventArgs e)
        {
            SubViewChange(Sub_View.MAIN);
        }

        private void btn_PreCalibration_Click(object sender, EventArgs e)
        {
            SubViewChange(Sub_View.CALIBRATION);
        }

        private void btn_PreConfig_Click(object sender, EventArgs e)
        {
            SubViewChange(Sub_View.CONFIG);
        }

        private void SubViewChange(Sub_View view)
        {
            // Auto Mode에서는 MAIN과 CALIBRATION 뷰만 허용
            if (Global.bAutoMode && curr_View == Sub_View.MAIN && view != Sub_View.CALIBRATION) return;

            formInfo       .HideViewer();
            formCalibration.HideViewer();
            formConfig     .HideViewer();

            btn_PreMain.BackColor = Color.White;
            btn_PreConfig.BackColor = Color.White;
            btn_PreCalibration.BackColor = Color.White;
            if (view == Sub_View.MAIN)
                btn_PreMain.BackColor = Color.DeepSkyBlue;
            if (view == Sub_View.CONFIG)
                btn_PreConfig.BackColor = Color.DeepSkyBlue;
            if (view == Sub_View.CALIBRATION)
                btn_PreCalibration.BackColor = Color.DeepSkyBlue;


            switch (view)
            {
                case Sub_View.MAIN       : formInfo       .ShowViewer(); curr_View = Sub_View.MAIN       ; break;
                case Sub_View.CALIBRATION: formCalibration.ShowViewer(); curr_View = Sub_View.CALIBRATION; break;
                case Sub_View.CONFIG     : formConfig     .ShowViewer(); curr_View = Sub_View.CONFIG     ; break;
                default                  : formInfo       .ShowViewer(); curr_View = Sub_View.MAIN       ; break;
            }
        }
    }
}
