using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using VoAlgorithm;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Vision_Align._2.FormViewer;

namespace Vision_Align
{
    public partial class FormBase : Form
    {
        
        public FormBase()
        {
            InitializeComponent();

            Initializ();
        }

        public void Initializ()
        {
            Global.formBase = this;

            SplashManager.UpdateStatus("Creating Logger...");
            #region Logger 생성
            Global.logger = new Dictionary<LogType, ClsLogger>();
            for (LogType n = 0; n < LogType.MAX; n++)
            {
                string strTemp = ((LogType)n).ToString();
#pragma warning disable CS0612 // 형식 또는 멤버는 사용되지 않습니다.
                Global.logger.Add(n, new ClsLogger(strTemp, string.Format(@"LOG\" + strTemp + "\\${{date:format=yyyyMMdd}}.log")));
#pragma warning restore CS0612 // 형식 또는 멤버는 사용되지 않습니다.
            }
            #endregion

            SplashManager.UpdateStatus("Initializing HALCON...");
            VisionAlgorithm.Initialize();

            SplashManager.UpdateStatus("Connecting Motion Library...");
            ClsMotion.OpenLib();

            SetStyle();


            SplashManager.UpdateStatus("Loading Configuration...");
            Global.IsLoadConfig(true);

            CrateFolder();

            SplashManager.UpdateStatus("Connecting Light Controller...");
            Global.clsLight.Open(Global.Serial_Param.nPort, Global.Serial_Param.nbaud);
            SplashManager.UpdateStatus("Connecting PLC...");
            Global.clsOmron.Open("192.168.240.80");

            Global.clsAlgorithm[0].SetTriangleDirectionMark(100, 100);
            Global.clsAlgorithm[1].SetTriangleDirectionMark(100, 100);
            Global.clsAlgorithm[0].SetSquareDirectionMark(100, 100);
            Global.clsAlgorithm[1].SetSquareDirectionMark(100, 100);

            SplashManager.UpdateStatus("Creating Background Threads...");
            Global.clsAutoThread = new ClsAutoThread();
            Global.clsFolderThread = new ClsFolderThread();
            Global.clsOmronThread = new ClsOmronThread();

            SplashManager.UpdateStatus("Initializing Camera and Motor...");
            CreateDictionary();

            SplashManager.UpdateStatus("Creating Forms...");
            CreateForm();


            Global.logger[(int)LogType.SYSTEM].Write("========= Program Start =========");


            SplashManager.Close();

            timer_ViewerChange.Interval = 100;
            timer_ViewerChange.Start();
        }

        #region Style
        int m_nMenuHight = 0;
        int m_nTitleHeight = 0;
        private void SetStyle()
        {
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            ControlBox = MinimizeBox = MaximizeBox = false;

            int nWidth = Screen.PrimaryScreen.Bounds.Width;
            int nHeight = Screen.PrimaryScreen.Bounds.Height;
            Location = new Point(0, 0);
            Size = new Size(nWidth, nHeight);

            panelMenu.Visible = Global.bUseFormMenu;
            if (Global.bUseFormMenu)
            {
                // 하단 메뉴 위치 및 사이즈 설정
                m_nMenuHight = (int)((double)nHeight * 0.93);
                panelMenu.Location = new Point(0, m_nMenuHight);
                panelMenu.Size = new Size(nWidth, nHeight - m_nMenuHight);
                m_nMenuHight = panelMenu.Size.Height;
            }

            panelTitle.Visible = Global.bUseFormTitle;
            if (Global.bUseFormTitle)
            {
                // 상단 Title 위치 및 사이즈 설정
                m_nTitleHeight = (int)((double)nHeight * 0.06);
                panelTitle.Location = new Point(0, 0);
                panelTitle.Size = new Size(nWidth, m_nTitleHeight);
            }

            // 중앙 화면 위치 및 사이즈 설정
            panelView.Location = new Point(0, m_nTitleHeight);
            panelView.Size = new Size(nWidth, nHeight - (m_nTitleHeight + m_nMenuHight));

            //더블 버퍼 처리
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
            this.UpdateStyles();
        }
        #endregion

        private void timer_ViewerChange_Tick(object sender, EventArgs e)
        {
            ViewChange();

            if (Global.bMovePgm)
            {
                Location = Global.point;
            }
        }

        public void ShowViewr()
        {
            Show();
        }

        public void HideViewer()
        {
            Hide();
        }

        private void CrateFolder()
        {
            //Recipe
            if (!System.IO.Directory.Exists(Global.strRecipePath))
                System.IO.Directory.CreateDirectory(Global.strRecipePath);

            for (int n = 0; n < (int)MatchingType.Max; n++)
            {
                if (!System.IO.Directory.Exists(Global.strMatchPath[n]))
                    System.IO.Directory.CreateDirectory(Global.strMatchPath[n]);
            }

            //Config
            if (!System.IO.Directory.Exists(Global.strConfigPath))
                System.IO.Directory.CreateDirectory(Global.strConfigPath);

            if (!System.IO.Directory.Exists(Global.strRsltPath))
                System.IO.Directory.CreateDirectory(Global.strRsltPath);
           
        }

        public void CreateForm()
        {
            #region Create Base
            
            if (Global.bUseFormTitle)
            {
                Global.formTitle = new FormTitle();

                panelTitle.Controls.Clear();
                Global.formTitle.TopLevel = false;
                panelTitle.Controls.Add(Global.formTitle);

                Global.formTitle.Show();
                Global.formTitle.Dock = DockStyle.Fill;

                Global.formTitle.Initializ();
            }

            
            if (Global.bUseFormMenu)
            {
                Global.formMenu = new FormMenu();

                panelMenu.Controls.Clear();
                Global.formMenu.TopLevel = false;
                panelMenu.Controls.Add(Global.formMenu);

                Global.formMenu.Show();
                Global.formMenu.Dock = DockStyle.Fill;

                Global.formMenu.Initializ();

            }
            #endregion

            for (int n = 0; n < Global.MAX_CAM; n++)
            {
                Global.formDisplay[n] = new FormDisplay();
                Global.formDisplay[n].TopLevel = false;
                Global.formDisplay[n].Initializ(n);

                Global.formPreView[n] = new FormPreview();
                Global.formPreView[n].TopLevel = false;
                Global.formPreView[n].Initializ(n);
            }

            // 폼 생성
            Global.formMain = new FormMain();
            Global.formLog = new FormLogView();
            Global.formIO = new FormIO();
            Global.formSetCam = new FormSetCam();
            Global.formModel = new FormModel();
            Global.formAlarmView = new FormAlarmView();


            // 폼 도크
            Global.formMain.Dock = DockStyle.Fill;
            Global.formLog.Dock = DockStyle.Fill;
            Global.formIO.Dock = DockStyle.Fill;
            Global.formAlarmView.Dock = DockStyle.Fill;

            // 폼 초기화
            Global.formMain.Initializ();
            Global.formLog.Initializ();
            Global.formSetCam.Initializ();
            Global.formModel.Initializ();
            Global.formIO.Initializ();
            Global.formAlarmView.Initializ();
        }

        public void ViewChange()
        {
            if (Global.m_nCurrViewer == Global.m_nChangViewer) return;

            Global.formMain.HideViewer(); Global.formMain.TopLevel = false;
            Global.formLog.HideViewer(); Global.formLog.TopLevel = false;
            Global.formIO.HideViewer(); Global.formIO.TopLevel = false;
            Global.formAlarmView.HideViewer(); Global.formAlarmView.TopLevel = false;

            panelView.Controls.Clear();
            switch (Global.m_nChangViewer)
            {
                case Viewer.Main  : panelView.Controls.Add(Global.formMain  ); Global.formMain  .ShowViewr(); Global.m_nCurrViewer = Viewer.Main  ; break;
                case Viewer.Alram : panelView.Controls.Add(Global.formAlarmView); Global.formAlarmView.ShowViewr(); Global.m_nCurrViewer = Viewer.Alram; break;
                case Viewer.IO    : panelView.Controls.Add(Global.formIO); Global.formIO.ShowViewr(); Global.m_nCurrViewer = Viewer.IO    ; break;
                case Viewer.Log   : panelView.Controls.Add(Global.formLog); Global.formLog.ShowViewr();Global.m_nCurrViewer = Viewer.Log; break;

                default: Global.formMain.Show(); break;
            }

            Global.formMenu.UpdateSelectedButton(Global.m_nCurrViewer);
        }

        private void CreateDictionary()
        {

            for (CamInfo n = 0; n < CamInfo.MAX; n++)
            {
                Global.dicClsMotion.Add(n, new ClsMotion(n));
            }
            Global.dicClsMotion[CamInfo.CAM_1].Home();
            Global.dicClsMotion[CamInfo.CAM_2].Home();

            for (CamInfo n = 0; n < CamInfo.MAX; n++ )
            {
                Global.dicClsCam.Add(n, new ClsCamera(n, Global.inforCam.strCamSerial[(int)n]));

                double dExposure = Global.CamSet_Param.dicExposure[n][0];
                int    nGain     = Global.CamSet_Param.dicGain    [n][0];
                double dGamma    = Global.CamSet_Param.dicGamma   [n][0];
                bool   bFlipX    = Global.CamSet_Param.dicFlipX   [n][0];
                bool   bFlipY    = Global.CamSet_Param.dicFlipY   [n][0];
                bool   bRotate   = Global.CamSet_Param.dicRotation[n][0];

                Global.dicClsCam[n].SetParam(dExposure, nGain, dGamma, bFlipX, bFlipY, bRotate);
            }


        }



    }
}
