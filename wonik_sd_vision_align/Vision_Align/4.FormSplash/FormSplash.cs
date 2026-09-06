using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Vision_Align
{
    public partial class FormSplash : Form
    {
        private int m_nActiveDot = 0;
        private const int DOT_COUNT = 12;

        public FormSplash()
        {
            InitializeComponent();

            // Panel 더블 버퍼링 활성화 (깜빡임 방지)
            typeof(Panel).InvokeMember("DoubleBuffered",
                BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                null, panelSpinner, new object[] { true });
        }

        private void FormSplash_Load(object sender, EventArgs e)
        {
            // Load logo
            try
            {
                string strExePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string strLogoPath = Path.Combine(strExePath, "Images", "logo.png");
                if (File.Exists(strLogoPath))
                {
                    pictureBoxLogo.Image = Image.FromFile(strLogoPath);
                }
            }
            catch { }

            // Set version label
            Version ver = Assembly.GetExecutingAssembly().GetName().Version;
            labelVersion.Text = string.Format("v{0}.{1}.{2}.{3}", ver.Major, ver.Minor, ver.Build, ver.Revision);

            timerAnimation.Start();
        }

        private void timerAnimation_Tick(object sender, EventArgs e)
        {
            m_nActiveDot = (m_nActiveDot + 1) % DOT_COUNT;
            panelSpinner.Invalidate();
        }

        private void panelSpinner_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            float fCenterX = panelSpinner.Width / 2f;
            float fCenterY = panelSpinner.Height / 2f;
            float fRadius = Math.Min(fCenterX, fCenterY) - 8f;
            float fDotSize = 6f;

            for (int i = 0; i < DOT_COUNT; i++)
            {
                double dAngle = 2 * Math.PI * i / DOT_COUNT - Math.PI / 2;
                float fX = fCenterX + (float)(fRadius * Math.Cos(dAngle)) - fDotSize / 2f;
                float fY = fCenterY + (float)(fRadius * Math.Sin(dAngle)) - fDotSize / 2f;

                int nAlpha;
                Color color;

                // Distance from active dot (wrapping)
                int nDist = (m_nActiveDot - i + DOT_COUNT) % DOT_COUNT;

                if (nDist == 0)
                {
                    // Active dot - brightest
                    nAlpha = 255;
                    color = Color.FromArgb(nAlpha, 100, 200, 255);
                }
                else if (nDist <= 3)
                {
                    // Trailing dots - fade out
                    nAlpha = 255 - (nDist * 55);
                    color = Color.FromArgb(nAlpha, 100, 200, 255);
                }
                else
                {
                    // Inactive dots - dim
                    nAlpha = 40;
                    color = Color.FromArgb(nAlpha, 100, 200, 255);
                }

                using (SolidBrush brush = new SolidBrush(color))
                {
                    g.FillEllipse(brush, fX, fY, fDotSize, fDotSize);
                }
            }
        }

        public void SetStatus(string strText)
        {
            if (labelStatus.InvokeRequired)
            {
                labelStatus.BeginInvoke(new Action(() => labelStatus.Text = strText));
            }
            else
            {
                labelStatus.Text = strText;
            }
        }
    }
}
