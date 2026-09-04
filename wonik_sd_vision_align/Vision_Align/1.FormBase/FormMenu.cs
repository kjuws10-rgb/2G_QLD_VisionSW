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
    public partial class FormMenu : Form
    {
        Timer m_timer = new Timer();

        private int m_nFormWidth ;
        private int m_nFormHeight;

        public FormMenu()
        {
            InitializeComponent();
        }

        public void Initializ()
        {
            m_nFormWidth  = this.Width ;
            m_nFormHeight = this.Height;
            FormDisposition();
            SetStyle();

            m_timer.Interval = 500;
            m_timer.Tick += new EventHandler(OnTimer);
            m_timer.Start();
        }

        void OnTimer(object obj, EventArgs ea)
        {

        }

        #region Style
        public void FormDisposition()
        {
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            ControlBox = MinimizeBox = MaximizeBox = ShowInTaskbar = false;

            BackColor = Color.FromArgb(19, 50, 69);

            //int btnMarzin = 10;
            //int btnLocationY = (m_nFormHeight - btn_Exit.Height) / 2;

            //btn_Exit.Location = new Point(m_nFormWidth - btn_Exit.Width - btnMarzin, btnLocationY);
            //btn_Minimum.Location = new Point(btn_Exit.Location.X - btn_Minimum.Width - btnMarzin, btnLocationY);
        }

        private void SetStyle()
        {
            Color BtnColor = Color.FromArgb(38, 38, 38); 
            Color TextColor = Color.Black;
            Font font = new Font("Arial", 14, FontStyle.Bold);

            //btn_Minimum.BackColor = btn_Exit.BackColor = BtnColor;
            btn_Minimum.ForeColor = btn_Exit.ForeColor = TextColor;
            btn_Minimum.Font = btn_About.Font = btn_Exit.Font = font;

            //btn_Alram.BackColor = btn_Main.BackColor = btn_Manual.BackColor = BtnColor;
            //btn_Comm.BackColor = btn_IO.BackColor = btn_Recipe.BackColor = BtnColor;

            btn_Alram.ForeColor = btn_Main.ForeColor =TextColor;
            btn_Log.ForeColor = btn_IO.ForeColor = TextColor;

            btn_Main.Font = font;
            btn_IO.Font = font;
            btn_Log.Font  = new Font("Arial", 11, FontStyle.Bold);
            btn_Alram.Font = new Font("Arial", 11, FontStyle.Bold);

            btn_Minimum.ForeColor = Color.White;
            btn_Minimum.BackColor = Color.FromArgb(120, 60, 80);

            btn_About.ForeColor = btn_Exit.ForeColor = Color.White;
            btn_About.BackColor = btn_Exit.BackColor = Color.FromArgb(20, 50, 100);
        }
        #endregion

        #region Btn Func
        private void btn_About_Click(object sender, EventArgs e)
        {

        }

        private void btn_Exit_Click(object sender, EventArgs e)
        {
            // 설비가 START 일때는 강제로 알람 발생 후 종료
            if (MessageBox.Show("Do you want exit program ?", "Exit program", MessageBoxButtons.YesNo) != DialogResult.Yes) return;

            Global.formBase.RequestShutdown();
        }

        private void btn_Minimum_Click(object sender, EventArgs e)
        {
            Global.formBase.WindowState = FormWindowState.Minimized;
        }

        private void btn_Main_Click(object sender, EventArgs e)
        {
            Global.m_nChangViewer = Viewer.Main;
        }

        private void btn_IO_Click(object sender, EventArgs e)
        {
            Global.m_nChangViewer = Viewer.IO;
        }

        private void btn_Log_Click(object sender, EventArgs e)
        {
            Global.m_nChangViewer = Viewer.Log;
        }

        private void btn_Alram_Click(object sender, EventArgs e)
        {
            Global.m_nChangViewer = Viewer.Alram;
        }

        #endregion

        private void btnAlgorithmTest_Click(object sender, EventArgs e)
        {
            FormAlgorithmTest formAlgorithmTest = new FormAlgorithmTest();
            formAlgorithmTest.ShowDialog();
        }
    }
}
