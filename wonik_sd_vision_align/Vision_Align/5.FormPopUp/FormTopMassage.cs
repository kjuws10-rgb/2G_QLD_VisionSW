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
    public partial class FormTopMassage : Form
    {
        private Point m_pointMouseDown;
        private bool m_bShow = false;

        string m_strMsg = "";
        int m_nFontSize = 90;
        Color? m_color;
        bool m_bAlram = false;

        Timer m_timer = new Timer();

        public FormTopMassage()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            FormDisposition();
            SetStyle();
            m_timer.Interval = 10;
            m_timer.Tick += new EventHandler(OnTimer);
            m_timer.Start();
            this.Hide();
            //this.TopMost = true;
        }
        void OnTimer(object obj, EventArgs ea)
        {
            ShowMessage();
        }


        #region Style
        public void FormDisposition()
        {
            StartPosition = FormStartPosition.Manual;
            ControlBox = MinimizeBox = MaximizeBox = ShowInTaskbar = false;
            

            int nWidth = Screen.PrimaryScreen.Bounds.Width;
            int nHeight = Screen.PrimaryScreen.Bounds.Height;

            Location = new Point(20, nHeight / 2 - Size.Height / 2);
            this.Size = new Size(nWidth - 40, this.Size.Height);
            TopMost = true;
            this.Opacity = 0.85;
        }

        private void SetStyle()
        {
            panel_Title.Location = new Point(10, 10);
            panel_Title.Size = new Size(this.Size.Width - 20, panel_Title.Size.Height);

            panel_Title.BackColor = panel_MoveBar.BackColor = Color.LightGray;
            label_Title.ForeColor = Color.White;
            label_Title.Font = new Font("Arial", 90, FontStyle.Bold);
            label_Title.Text = "TEST";

            btn_Ok.Location = new Point(this.Size.Width / 2 - btn_Ok.Size.Width - 10, btn_Ok.Location.Y);
            btn_Ok.Font = new Font("Arial", 32, FontStyle.Bold);

            btn_Reset.Location = new Point(this.Size.Width / 2 +10, btn_Ok.Location.Y);
            btn_Reset.Font = new Font("Arial", 32, FontStyle.Bold);
        }
        #endregion

        public void ShowMessage(string strMsg, int nFontSize = 90, Color? color = null, bool bAlram = false)
        {
            m_strMsg = strMsg;
            m_nFontSize = nFontSize;
            m_color = color;
            m_bAlram = bAlram;
            m_bShow = true;
            //this.TopMost = true;
        }

        private void ShowMessage()
        {
            if (!m_bShow) return;

            SetText(m_strMsg, m_nFontSize, m_color);

            this.Show();
            m_bShow = false;
        }

        public void SetText(string strMsg, int nFontSize = 90, Color? color = null)
        {
            label_Title.Text = strMsg;
            label_Title.ForeColor = color ?? Color.White;
            label_Title.Font = new Font("Arial", nFontSize, FontStyle.Bold);
        }

        public void HideMessage()
        {
            try
            {
                this.Hide();
            } catch(Exception e)
            {
                e.ToString();
            }
        }

        private void btn_Ok_Click(object sender, EventArgs e)
        {
            HideMessage();
        }

        private void panel_MoveBar_MouseMove(object sender, MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
                Location = new Point(Left - (m_pointMouseDown.X - e.X), Top - (m_pointMouseDown.Y - e.Y));
        }

        private void panel_MoveBar_MouseDown(object sender, MouseEventArgs e)
        {
            m_pointMouseDown = new Point(e.X, e.Y);
        }

        private void btn_Reset_Click(object sender, EventArgs e)
        {
            
        }
    }
}
