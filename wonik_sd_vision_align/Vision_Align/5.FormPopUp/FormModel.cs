using HalconDotNet;
using Newtonsoft.Json.Linq;
using VoAlgorithm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using uEye;
using static HalconDotNet.HDrawingObject;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Vision_Align
{
    public partial class FormModel : Form
    {
        Point m_pointMouseDown;

        int m_nCamNo = 0;

        HTuple hv_WindowHandle;
        bool m_bSetRoi = false;

        MatchingType matching_Type;

        string m_strTypeFolder = "";
        string m_strCurrModel = "";

        HObject LoadedImg;


        Timer timer = new Timer();

        public FormModel()
        {
            InitializeComponent();
            SetStyle();
            
        }

        public void Initializ()
        {
            hv_WindowHandle = hControl_Display.HalconWindow;
            

            this.MouseWheel += hControl_Display.HSmartWindowControl_MouseWheel;
            hControl_Display.HMouseDown += HWindowControl_MouseDown;
            hControl_Display.HMouseUp   += HWindowControl_MouseUp;



            timer.Tick += new EventHandler(OnTimer);
        }

        void OnTimer(object obj, EventArgs ea)
        {
           if(Global.bAutoMode) 
                HideViewer();
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
            //nud_MG_Angle_Min.Minimum = -360; nud_MG_Angle_Min.Maximum = 0;
        }
        #endregion

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

        #region Mouse Event
        private void HWindowControl_MouseDown(object sender, HMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && m_bSetRoi)
            {
                SetRoi();
            }
            else if (e.Button == MouseButtons.Left && m_bSetRoi)
            {
                //HOperatorSet.ClearDrawingObject(hv_InnerCircleID);
                
            }
        }

        private void HWindowControl_MouseUp(object sender, HMouseEventArgs e)
        {
            
            if (e.Button == MouseButtons.Left && m_bSetRoi)
            {
                Global.clsAlgorithm[m_nCamNo].DrawMove(hv_WindowHandle, Convert.ToDouble(nud_MG_Inner_Circle.Value));
            }

        }
        #endregion

        private void DisplayImageFitToWindow(HObject image)
        {
            // 이미지 해상도
            HOperatorSet.GetImageSize(image, out HTuple imgWidth, out HTuple imgHeight);

            // 윈도우 크기
            int winWidth = hControl_Display.Width;
            int winHeight = hControl_Display.Height;

            // 이미지 비율 / 윈도우 비율 비교
            double imgRatio = (double)imgWidth / imgHeight;
            double winRatio = (double)winWidth / winHeight;

            double scale, newHeight, newWidth;

            if (imgRatio >= winRatio)
            {
                // 너비 기준 스케일
                scale = (double)imgWidth / winWidth;
            }
            else
            {
                // 높이 기준 스케일
                scale = (double)imgHeight / winHeight;
            }

            newHeight = scale * winHeight;
            newWidth = scale * winWidth;

            double row1 = -(newHeight - imgHeight) / 2.0;
            double col1 = -(newWidth - imgWidth) / 2.0;
            double row2 = row1 + newHeight - 1;
            double col2 = col1 + newWidth - 1;

            // SetPart 설정
            hControl_Display.HalconWindow.SetPart(row1, col1, row2, col2);

            // 표시
            hControl_Display.HalconWindow.ClearWindow();
            hControl_Display.HalconWindow.DispObj(image);
        }

        private void SetUi(MatchingType type)
        {
            string strTitle = "";

            switch (type)
            {
                case MatchingType.GlassShape:
                    strTitle = string.Format("[Camera#CAM{0} Create Model Manu] ▶ [GLASS][SHAPE][TEMPLETE] Matching", m_nCamNo + 1);

                    if (tabControl1.TabPages.Contains(tabPage2)) tabControl1.TabPages.Remove(tabPage2);
                    if (tabControl1.TabPages.Contains(tabPage3)) tabControl1.TabPages.Remove(tabPage3);
                    if (!tabControl1.TabPages.Contains(tabPage1)) tabControl1.TabPages.Insert(0, tabPage1);

                    break;

                case MatchingType.MaskShape:
                    strTitle = string.Format("[Camera#CAM{0} Create Model Manu] ▶ [MASK][SHAPE][TEMPLETE] Matching", m_nCamNo + 1);

                    if (tabControl1.TabPages.Contains(tabPage1)) tabControl1.TabPages.Remove(tabPage1);
                    if (tabControl1.TabPages.Contains(tabPage2)) tabControl1.TabPages.Remove(tabPage2);
                    if (!tabControl1.TabPages.Contains(tabPage3)) tabControl1.TabPages.Insert(0, tabPage3);

                    break;

                case MatchingType.MarkGray:
                    strTitle = string.Format("[Camera#CAM{0} Create Model Manu] ▶ [MASK][GRAY][ROI] Matching", m_nCamNo + 1);

                    if (tabControl1.TabPages.Contains(tabPage1)) tabControl1.TabPages.Remove(tabPage1);
                    if (tabControl1.TabPages.Contains(tabPage3)) tabControl1.TabPages.Remove(tabPage3);
                    if (!tabControl1.TabPages.Contains(tabPage2)) tabControl1.TabPages.Insert(0, tabPage2);

                    break;
            }

            tb_CurrName.Text = m_strCurrModel = Global.Sys_Param.strCurrModel[m_nCamNo, (int)type];
            m_strTypeFolder  = Global.strMatchPath[(int)type];

            IsLoad(true, tb_CurrName.Text);
            
            

            GetFolder(m_strTypeFolder);
            label_Title.Text = strTitle;
        }

        public void ShowViewer(int nCamNo, MatchingType type)
        {
            //this.TopMost = true;
            m_nCamNo = nCamNo;

            matching_Type = type;
            SetUi(matching_Type);

            timer.Interval = 100;
            timer.Start();

            this.Show();
        }

        public void HideViewer()
        {
            this.TopMost = false;

            timer.Stop();

            this.Hide();
        }

        #region Roi Model
        private void btn_SetRoi_Click(object sender, EventArgs e)
        {
            CreateRoi();
        }

        private void SetRoi()
        {
            Global.clsAlgorithm[m_nCamNo].SetRoi(hv_WindowHandle, Convert.ToDouble(nud_MG_Inner_Circle.Value), ref Global.hModelInfo[m_nCamNo, (int)matching_Type]);
            m_bSetRoi = false;
        }

        public void CreateRoi()
        {
            Global.clsAlgorithm[m_nCamNo].CreateRoi(hv_WindowHandle, Global.hModelInfo[m_nCamNo, (int)matching_Type]);
            m_bSetRoi = true;
        }


        private void btn_Grab_Click(object sender, EventArgs e)
        {
            if(!Global.dicClsCam[(CamInfo)m_nCamNo].IsOpen())
            {
                string strPath = "";
                if (m_nCamNo == 0) strPath = "D:\\1.Project\\7.IPS-Align\\Test Image\\Cam00-14-08-37";
                else               strPath = "D:\\1.Project\\7.IPS-Align\\Test Image\\Cam01-14-18-31";
                    
                Global.clsAlgorithm[m_nCamNo].SetImage(hv_WindowHandle, strPath);


                return;
            }

            byte[] temp = Global.dicClsCam[(CamInfo)m_nCamNo].GetImgBuff();
            Global.clsAlgorithm[m_nCamNo].SetImage(hv_WindowHandle, temp, Global.dicClsCam[(CamInfo)m_nCamNo].Width, Global.dicClsCam[(CamInfo)m_nCamNo].Height);

        }

        private void btn_Test_Click(object sender, EventArgs e)
        {
            if (cb_MG_SubPixel.SelectedIndex < 0)
            {
                MessageBox.Show("SubPixel option is not selected.");
                return;
            }

            ModelParam modelParam = new ModelParam();
            modelParam.NumLevels = Convert.ToInt32(nud_MG_NumLevel.Value);
            modelParam.AngleMin = Convert.ToDouble(nud_MG_Angle_Min.Value);
            modelParam.AngleMax = Convert.ToDouble(nud_MG_Angle_Max.Value);
            modelParam.MinScore = Convert.ToDouble(nud_MG_MinScore.Value);
            modelParam.MaxOverlap = Convert.ToDouble(nud_MG_MaxOverlap.Value);
            modelParam.SubPixel = cb_MG_SubPixel.Items[cb_MG_SubPixel.SelectedIndex].ToString();

            bool bRslt = Global.clsAlgorithm[m_nCamNo].FindNccModel(modelParam, Global.hModelInfo[m_nCamNo, (int)matching_Type], out ModelResult rslt);
            
            Global.clsAlgorithm[m_nCamNo].OverlayNccModel(bRslt, hv_WindowHandle, rslt.X, rslt.Y, rslt.Angle, rslt.Score, "cyan");
        }
        #endregion

        #region Mask Templete
        private void btn_Init_Mask_Click(object sender, EventArgs e)
        {

        }

        private void btn_Add_Mask_Click(object sender, EventArgs e)
        {
            if (cb_MS_BackGround.SelectedIndex < 0)
            {
                MessageBox.Show("BackGround option is not selected.");
                return;
            }

            ModelParam shape = new ModelParam();
            shape.Radius       = Convert.ToDouble(nud_MS_Radius.Value);
            shape.Background = cb_MS_BackGround.Items[cb_MS_BackGround.SelectedIndex].ToString();

            VisionAlgorithm.CreateCircleImg(hv_WindowHandle, shape, ref LoadedImg);
        }

        private void btn_Create_Mask_Click(object sender, EventArgs e)
        {
            SetModel(tb_CurrName.Text);
        }

        private void btn_AutoGenMaskTemplate_Click(object sender, EventArgs e)
        {
            try
            {
                // 현재 모델이 로드되어 있는지 확인
                if (Global.hModelInfo[m_nCamNo, (int)MatchingType.MaskShape] == null ||
                    Global.hModelInfo[m_nCamNo, (int)MatchingType.MaskShape].ModelID == null)
                {
                    MessageBox.Show("MASK_SHAPE model is not loaded.");
                    return;
                }

                // 현재 UI에서 파라미터 수집
                if (cb_MS_Metric.SelectedIndex < 0)
                {
                    MessageBox.Show("Metric option is not selected.");
                    return;
                }
                if (cb_MS_BackGround.SelectedIndex < 0)
                {
                    MessageBox.Show("BackGround option is not selected.");
                    return;
                }
                if (cb_MS_SubPixel.SelectedIndex < 0)
                {
                    MessageBox.Show("SubPixel option is not selected.");
                    return;
                }

                ModelParam modelParam = new ModelParam();
                modelParam.NumLevels   = Convert.ToInt32(nud_MS_NumLevel.Value);
                modelParam.AngleMin    = Convert.ToDouble(nud_MS_Angle_Min.Value);
                modelParam.AngleMax    = Convert.ToDouble(nud_MS_Angle_Max.Value);
                modelParam.MinScore    = Convert.ToDouble(nud_MS_MinScore.Value);
                modelParam.MaxOverlap  = Convert.ToDouble(nud_MS_MaxOverlap.Value);
                modelParam.SubPixel  = cb_MS_SubPixel.Items[cb_MS_SubPixel.SelectedIndex].ToString();
                modelParam.Metric    = cb_MS_Metric.Items[cb_MS_Metric.SelectedIndex].ToString();
                modelParam.Radius      = Convert.ToDouble(nud_MS_Radius.Value);
                modelParam.Background = cb_MS_BackGround.Items[cb_MS_BackGround.SelectedIndex].ToString();

                // Auto Gen 시에는 Scale을 최대한 넓은 범위로 설정
                modelParam.ScaleMin = 0.5;
                modelParam.ScaleMax = 2.0;

                // FindShapeModel 실행하여 Scale 획득
                bool bFound = Global.clsAlgorithm[m_nCamNo].FindShapeModel(modelParam,
                    Global.hModelInfo[m_nCamNo, (int)MatchingType.MaskShape], out ModelResult rslt);

                if (!bFound)
                {
                    MessageBox.Show("FindScaledShapeModel failed. Cannot detect current scale.");
                    return;
                }

                double dOldRadius = modelParam.Radius;
                double dScale = rslt.Scale;
                double dNewRadius = Math.Round(dOldRadius * dScale, 1);

                // Scale이 1.0에 매우 가까우면 재생성 불필요
                if (Math.Abs(dScale - 1.0) < 0.001)
                {
                    MessageBox.Show(string.Format("Current Scale = {0:F3}\nRadius adjustment is not needed.", dScale));
                    return;
                }

                // UI의 Radius 업데이트
                nud_MS_Radius.Value = (decimal)dNewRadius;
                modelParam.Radius = dNewRadius;

                // 새 Template 이미지 생성
                VisionAlgorithm.CreateCircleImg(hv_WindowHandle, modelParam, ref LoadedImg);

                // Shape Model 재학습
                VisionAlgorithm.CreateShapeMaskModel(hv_WindowHandle, modelParam, LoadedImg,
                    ref Global.hModelInfo[m_nCamNo, (int)MatchingType.MaskShape]);

                // 모델 저장
                string strPath = m_strTypeFolder + "\\" + tb_CurrName.Text;
                VisionAlgorithm.SaveModel(MatchingType.MaskShape,
                    Global.hModelInfo[m_nCamNo, (int)MatchingType.MaskShape], LoadedImg,
                    strPath + "\\" + tb_CurrName.Text);

                // Recipe JSON 저장
                IsLoadMaskS(false, tb_CurrName.Text);

                MessageBox.Show(string.Format(
                    "Auto Gen Complete.\n\nScale: {0:F3}\nOld Radius: {1:F1}\nNew Radius: {2:F1}",
                    dScale, dOldRadius, dNewRadius));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Auto Gen failed.\n" + ex.Message);
            }
        }

        #endregion

        #region Glass Templete
        private void btn_Add_Glass_Click(object sender, EventArgs e)
        {
            if (cb_GS_BackGround.SelectedIndex < 0)
            {
                MessageBox.Show("BackGround option is not selected.");
                return;
            }

            ModelParam shape = new ModelParam();
            shape.SizeX = Convert.ToDouble(nud_GS_X_Size.Value);
            shape.SizeY = Convert.ToDouble(nud_GS_Y_Size.Value);
            shape.Depth = Convert.ToDouble(nud_GS_Depth.Value);
            shape.Background = cb_GS_BackGround.Items[cb_GS_BackGround.SelectedIndex].ToString();

            VisionAlgorithm.CreateCrossImg(hv_WindowHandle, shape, ref LoadedImg);
        }
        private void btn_Create_Glass_Click(object sender, EventArgs e)
        {
            SetModel(tb_CurrName.Text);
        }
        #endregion


        private void btn_ModelSave_Click(object sender, EventArgs e)
        {
            IsLoad(false, tb_CurrName.Text);
            GetFolder(m_strTypeFolder);
        }

        private void btn_ModelLoad_Click(object sender, EventArgs e)
        {
            if (gridView_FileView.SelectedRows.Count == 0)
            {
                MessageBox.Show("No model is selected.");
                return;
            }

            string strTemp = Path.GetFileNameWithoutExtension(gridView_FileView.SelectedRows[0].Cells[0].Value.ToString());
            
            IsLoad(true, strTemp);

            Global.Sys_Param.strCurrModel[m_nCamNo, (int)matching_Type] = tb_CurrName.Text = strTemp;
            Global.Sys_Param = Global.IsLoad_List(false, Global.Sys_Param);
        }

        private void btn_CreateModel_Click(object sender, EventArgs e)
        {
            if (tb_NewName.Text == "")
            {
                MessageBox.Show("There is no model name.");
                return;
            }

            string strPath = m_strTypeFolder + "\\" + tb_NewName.Text;
            if (!Directory.Exists(strPath)) Directory.CreateDirectory(strPath);
           
            GetFolder(m_strTypeFolder);

            Global.Sys_Param.strCurrModel[m_nCamNo, (int)matching_Type] = tb_CurrName.Text = tb_NewName.Text;
            Global.Sys_Param = Global.IsLoad_List(false, Global.Sys_Param);
        }

        private void btn_Close_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void SetModel(string strModel)
        {
            string strPath = m_strTypeFolder + "\\" + strModel;
            ModelParam modelParam = new ModelParam();

            switch (matching_Type)
            {
                case MatchingType.MarkGray:
                    if (cb_GrayMetric.SelectedIndex < 0) { MessageBox.Show("Metric option is not selected."); return; }
                    modelParam.NumLevels = Convert.ToInt32(nud_MG_NumLevel.Value);
                    modelParam.AngleMin = Convert.ToDouble(nud_MG_Angle_Min.Value);
                    modelParam.AngleMax = Convert.ToDouble(nud_MG_Angle_Max.Value);
                    modelParam.Metric = cb_GrayMetric.Items[cb_GrayMetric.SelectedIndex].ToString();

                    Global.clsAlgorithm[m_nCamNo].CreateNccModel(hv_WindowHandle, modelParam, ref Global.hModelInfo[m_nCamNo, (int)matching_Type]);

                    break;

                case MatchingType.MaskShape:
                    if (cb_MS_Metric.SelectedIndex < 0) { MessageBox.Show("Metric option is not selected."); return; }
                    modelParam.NumLevels = Convert.ToInt32(nud_MS_NumLevel.Value);
                    modelParam.AngleMin  = Convert.ToDouble(nud_MS_Angle_Min.Value);
                    modelParam.AngleMax  = Convert.ToDouble(nud_MS_Angle_Max.Value);
                    modelParam.Metric  = cb_MS_Metric.Items[cb_MS_Metric.SelectedIndex].ToString();
                    modelParam.Radius    = Convert.ToDouble(nud_MS_Radius.Value);
                    VisionAlgorithm.CreateShapeMaskModel(hv_WindowHandle, modelParam, LoadedImg, ref Global.hModelInfo[m_nCamNo, (int)matching_Type]);

                    //Global.clsAlgorithm[m_nCamNo].SaveModel(matching_Type, Global.hModelInfo[m_nCamNo, (int)matching_Type], strPath + "\\" + strModel);
                    break;

                case MatchingType.GlassShape:
                    if (cb_GS_Metric.SelectedIndex < 0) { MessageBox.Show("Metric option is not selected."); return; }
                    modelParam.NumLevels = Convert.ToInt32(nud_GS_NumLevel.Value);
                    modelParam.AngleMin = Convert.ToDouble(nud_GS_Angle_Min.Value);
                    modelParam.AngleMax = Convert.ToDouble(nud_GS_Angle_Max.Value);
                    modelParam.Metric = cb_GS_Metric.Items[cb_GS_Metric.SelectedIndex].ToString();
                    modelParam.SizeX = Convert.ToDouble(nud_GS_X_Size.Value);
                    modelParam.SizeY = Convert.ToDouble(nud_GS_Y_Size.Value);

                    VisionAlgorithm.CreateShapeGlassModel(hv_WindowHandle, modelParam, LoadedImg, ref Global.hModelInfo[m_nCamNo, (int)matching_Type]);
                    //Global.clsAlgorithm[m_nCamNo].SaveModel(matching_Type, Global.hModelInfo[m_nCamNo, (int)matching_Type], strPath + "\\" + strModel);

                    //Global.clsHalconMgr.SaveModel(matching_Type, Global.hModelDatas[m_nCamNo, (int)matching_Type], strPath + "\\" + strModel);
                    //Global.clsHalconMgr.SaveImage(strPath + "\\" + strModel);

                    break;
            }

            VisionAlgorithm.SaveModel(matching_Type, Global.hModelInfo[m_nCamNo, (int)matching_Type], LoadedImg, strPath + "\\" + strModel);
        }

        private void GetFolder(string strPath)
        {
            try
            {
                string strExtension = "";
                string[] files = Directory.GetFiles(strPath, "*.*", SearchOption.AllDirectories);

                gridView_FileView.Rows.Clear();

                gridView_FileView.DefaultCellStyle.Font = new Font("Arial", 9);

                if (matching_Type == MatchingType.GlassShape || matching_Type == MatchingType.MaskShape) strExtension = ".shm"; 
                else                                            strExtension = ".ncm";

                for (int i = 0; i < files.Length; i++)
                {
                    if (!files[i].Contains(strExtension)) continue;
                    FileInfo info = new FileInfo(files[i]);

                    gridView_FileView.RowCount++;
                    gridView_FileView.Rows[gridView_FileView.RowCount - 1].Cells[0].Value = info.Name;
                    gridView_FileView.Rows[gridView_FileView.RowCount - 1].Cells[1].Value = info.FullName;
                    gridView_FileView.Rows[gridView_FileView.RowCount - 1].Cells[2].Value = info.LastWriteTime;
                    gridView_FileView.CurrentCell = null;

                    gridView_FileView.Rows[gridView_FileView.Rows.Count - 1].Height = 30;
                }
            }
            catch { }
        }


        private void btn_AutoCreateModel_Click(object sender, EventArgs e)
        {
            SetModel(tb_CurrName.Text);
        }

        #region Recipe Load / Save
        private void IsLoad(bool IsLoad, string strModel)
        {
            IsLoadModel(IsLoad, strModel);
            IsLoadParam(IsLoad, strModel);
        }

        private void IsLoadModel(bool IsLoad, string strModel)
        {
            string strPath = string.Format("{0}\\{1}\\{2}", m_strTypeFolder, strModel, strModel);
            if(IsLoad) Global.clsAlgorithm[m_nCamNo].LoadModel(matching_Type, strPath, out Global.hModelInfo[m_nCamNo, (int)matching_Type]);

            switch (matching_Type)
            {
                case MatchingType.MarkGray  :  break;
                case MatchingType.MaskShape :  break;
                case MatchingType.GlassShape:  break;
            }
        }


        private void IsLoadParam(bool IsLoad, string strModel)
        {
            switch (matching_Type)
            {
                case MatchingType.MarkGray  : IsLoadMaskG (IsLoad, strModel); break;
                case MatchingType.MaskShape : IsLoadMaskS (IsLoad, strModel); break;
                case MatchingType.GlassShape: IsLoadGlassS(IsLoad, strModel); break;
            }
            string strPath = string.Format("{0}\\{1}\\{2}", m_strTypeFolder, strModel, strModel);
            if(IsLoad) SetImage(hv_WindowHandle, strPath);
        }

        public void SetImage(HTuple hWindow, string strPath)
        {
            try
            {
                HOperatorSet.ClearWindow(hWindow);
                if (System.IO.File.Exists(strPath + ".bmp"))
                {
                    HOperatorSet.ReadImage(out LoadedImg, strPath);


                    HTuple width, height;
                    HOperatorSet.GetImageSize(LoadedImg, out width, out height);

                    VisionAlgorithm.FitImage(hWindow, width, height);

                    HOperatorSet.DispObj(LoadedImg, hWindow);
                }
            }
            catch { }
        }

        private void IsLoadMaskG(bool IsLoad, string strModel)
        {
            try
            {
                string strPath = string.Format("{0}\\{1}\\{2}", m_strTypeFolder, strModel, strModel);

                if (IsLoad)
                {
                    Global.recipeMask_Ncc[m_nCamNo] = Global.IsLoad_List(IsLoad, Global.recipeMask_Ncc[m_nCamNo], strPath);

                    nud_MG_Angle_Min   .Value = (decimal)Global.recipeMask_Ncc[m_nCamNo].dAngleMin    ;
                    nud_MG_Angle_Max   .Value = (decimal)Global.recipeMask_Ncc[m_nCamNo].dAngleMax    ;
                    nud_MG_MaxOverlap  .Value = (decimal)Global.recipeMask_Ncc[m_nCamNo].dMaxOverlap  ;
                    nud_MG_MinScore    .Value = (decimal)Global.recipeMask_Ncc[m_nCamNo].dMinScore    ;
                    nud_MG_Inner_Circle.Value = (decimal)Global.recipeMask_Ncc[m_nCamNo].dInner_Circle;
                    nud_MG_MulOption   .Value = (decimal)Global.recipeMask_Ncc[m_nCamNo].dMultiOption ;
                    nud_MG_NumLevel    .Value = (decimal)Global.recipeMask_Ncc[m_nCamNo].nNumLevels   ;

                    cb_GrayMetric .SelectedIndex = cb_GrayMetric .FindString(Global.recipeMask_Ncc[m_nCamNo].strMetric  );
                    cb_MG_SubPixel.SelectedIndex = cb_MG_SubPixel.FindString(Global.recipeMask_Ncc[m_nCamNo].strSubPixel);

                    cb_MG_MulOption.Checked = Global.recipeMask_Ncc[m_nCamNo].bMultiOption;
                }
                else
                {
                    Global.recipeMask_Ncc[m_nCamNo].dAngleMin     = Convert.ToDouble(nud_MG_Angle_Min   .Value);
                    Global.recipeMask_Ncc[m_nCamNo].dAngleMax     = Convert.ToDouble(nud_MG_Angle_Max   .Value);
                    Global.recipeMask_Ncc[m_nCamNo].dMaxOverlap   = Convert.ToDouble(nud_MG_MaxOverlap  .Value);
                    Global.recipeMask_Ncc[m_nCamNo].dMinScore     = Convert.ToDouble(nud_MG_MinScore    .Value);
                    Global.recipeMask_Ncc[m_nCamNo].dInner_Circle = Convert.ToDouble(nud_MG_Inner_Circle.Value);
                    Global.recipeMask_Ncc[m_nCamNo].dMultiOption  = Convert.ToDouble(nud_MG_MulOption   .Value);
                    Global.recipeMask_Ncc[m_nCamNo].nNumLevels    = Convert.ToInt32 (nud_MG_NumLevel    .Value);

                    if (cb_GrayMetric.SelectedIndex >= 0)
                        Global.recipeMask_Ncc[m_nCamNo].strMetric   = cb_GrayMetric .Items[cb_GrayMetric .SelectedIndex].ToString();
                    if (cb_MG_SubPixel.SelectedIndex >= 0)
                        Global.recipeMask_Ncc[m_nCamNo].strSubPixel = cb_MG_SubPixel.Items[cb_MG_SubPixel.SelectedIndex].ToString();

                    Global.recipeMask_Ncc[m_nCamNo].bMultiOption = cb_MG_MulOption.Checked;

                    Global.recipeMask_Ncc[m_nCamNo] = Global.IsLoad_List(IsLoad, Global.recipeMask_Ncc[m_nCamNo], strPath);
                }
            }
            catch { }
        }

        private void IsLoadMaskS(bool IsLoad, string strModel)
        {
            try
            {
                string strPath = string.Format("{0}\\{1}\\{2}", m_strTypeFolder, strModel, strModel);

                if (IsLoad)
                {
                    Global.recipeMask_Shm[m_nCamNo] = Global.IsLoad_List(IsLoad, Global.recipeMask_Shm[m_nCamNo], strPath);

                    nud_MS_Radius    .Value = (decimal)Global.recipeMask_Shm[m_nCamNo].dRadius     ;
                    nud_MS_Angle_Min .Value = (decimal)Global.recipeMask_Shm[m_nCamNo].dAngleMin   ;
                    nud_MS_Angle_Max .Value = (decimal)Global.recipeMask_Shm[m_nCamNo].dAngleMax   ;
                    nud_MS_MaxOverlap.Value = (decimal)Global.recipeMask_Shm[m_nCamNo].dMaxOverlap ;
                    nud_MS_MinScore  .Value = (decimal)Global.recipeMask_Shm[m_nCamNo].dMinScore   ;
                    nud_MS_NumLevel  .Value = (decimal)Global.recipeMask_Shm[m_nCamNo].nNumLevels  ;
                    nud_MS_MulOption .Value = (decimal)Global.recipeMask_Shm[m_nCamNo].dMultiOption;
                    nud_MS_Scale_Min .Value = (decimal)Global.recipeMask_Shm[m_nCamNo].dScaleMin   ;
                    nud_MS_Scale_Max .Value = (decimal)Global.recipeMask_Shm[m_nCamNo].dScaleMax   ;

                    cb_MS_BackGround.SelectedIndex = cb_MS_BackGround.FindString(Global.recipeMask_Shm[m_nCamNo].strBackGround);
                    cb_MS_Metric    .SelectedIndex = cb_MS_Metric    .FindString(Global.recipeMask_Shm[m_nCamNo].strMetric    );
                    cb_MS_SubPixel  .SelectedIndex = cb_MS_SubPixel  .FindString(Global.recipeMask_Shm[m_nCamNo].strSubPixel  );

                    cb_MS_MulOption.Checked = Global.recipeMask_Shm[m_nCamNo].bMultiOption;
                }
                else
                {
                    Global.recipeMask_Shm[m_nCamNo].dRadius      = Convert.ToDouble(nud_MS_Radius    .Value);
                    Global.recipeMask_Shm[m_nCamNo].dAngleMin    = Convert.ToDouble(nud_MS_Angle_Min .Value);
                    Global.recipeMask_Shm[m_nCamNo].dAngleMax    = Convert.ToDouble(nud_MS_Angle_Max .Value);
                    Global.recipeMask_Shm[m_nCamNo].dMaxOverlap  = Convert.ToDouble(nud_MS_MaxOverlap.Value);
                    Global.recipeMask_Shm[m_nCamNo].dMinScore    = Convert.ToDouble(nud_MS_MinScore  .Value);
                    Global.recipeMask_Shm[m_nCamNo].nNumLevels   = Convert.ToInt32 (nud_MS_NumLevel  .Value);
                    Global.recipeMask_Shm[m_nCamNo].dMultiOption = Convert.ToDouble(nud_MS_MulOption .Value);
                    Global.recipeMask_Shm[m_nCamNo].dScaleMin    = Convert.ToDouble(nud_MS_Scale_Min .Value);
                    Global.recipeMask_Shm[m_nCamNo].dScaleMax    = Convert.ToDouble(nud_MS_Scale_Max .Value);

                    if (cb_MS_BackGround.SelectedIndex >= 0)
                        Global.recipeMask_Shm[m_nCamNo].strBackGround = cb_MS_BackGround.Items[cb_MS_BackGround.SelectedIndex].ToString();
                    if (cb_MS_Metric.SelectedIndex >= 0)
                        Global.recipeMask_Shm[m_nCamNo].strMetric     = cb_MS_Metric    .Items[cb_MS_Metric    .SelectedIndex].ToString();
                    if (cb_MS_SubPixel.SelectedIndex >= 0)
                        Global.recipeMask_Shm[m_nCamNo].strSubPixel   = cb_MS_SubPixel  .Items[cb_MS_SubPixel  .SelectedIndex].ToString();

                    Global.recipeMask_Shm[m_nCamNo].bMultiOption = cb_MS_MulOption.Checked;

                    Global.recipeMask_Shm[m_nCamNo] = Global.IsLoad_List(IsLoad, Global.recipeMask_Shm[m_nCamNo], strPath);
                }
            }
            catch{ }
        }

        private void IsLoadGlassS(bool IsLoad, string strModel)
        {
            try
            {
                string strPath = string.Format("{0}\\{1}\\{2}", m_strTypeFolder, strModel, strModel);

                if (IsLoad)
                {
                    Global.recipeGlass_Shm[m_nCamNo] = Global.IsLoad_List(IsLoad, Global.recipeGlass_Shm[m_nCamNo], strPath);
                    
                    nud_GS_X_Size     .Value = (decimal)Global.recipeGlass_Shm[m_nCamNo].dXSize      ;
                    nud_GS_Y_Size     .Value = (decimal)Global.recipeGlass_Shm[m_nCamNo].dYSize      ;
                    nud_GS_Depth      .Value = (decimal)Global.recipeGlass_Shm[m_nCamNo].dDepth      ;
                    nud_GS_Angle_Min  .Value = (decimal)Global.recipeGlass_Shm[m_nCamNo].dAngleMin   ;
                    nud_GS_Angle_Max  .Value = (decimal)Global.recipeGlass_Shm[m_nCamNo].dAngleMax   ;
                    nud_GS_MaxOverlap .Value = (decimal)Global.recipeGlass_Shm[m_nCamNo].dMaxOverlap ;
                    nud_GS_MinScore   .Value = (decimal)Global.recipeGlass_Shm[m_nCamNo].dMinScore   ;
                    nud_GS_NumLevel   .Value = (decimal)Global.recipeGlass_Shm[m_nCamNo].nNumLevels  ;
                    nud_GS_Scale_Min  .Value = (decimal)Global.recipeGlass_Shm[m_nCamNo].dScaleMin   ;
                    nud_GS_Scale_Max  .Value = (decimal)Global.recipeGlass_Shm[m_nCamNo].dScaleMax   ;
                    nud_GS_Contrast   .Value = (decimal)Global.recipeGlass_Shm[m_nCamNo].dContrast   ;
                    nud_GS_ContrastMin.Value = (decimal)Global.recipeGlass_Shm[m_nCamNo].dContrastMin;
                    nud_GS_Greedness  .Value = (decimal)Global.recipeGlass_Shm[m_nCamNo].dGreedNess  ;

                    cb_GS_Contrast   .Checked = Global.recipeGlass_Shm[m_nCamNo].bAutoContrast   ;
                    cb_GS_ContrastMin.Checked = Global.recipeGlass_Shm[m_nCamNo].bAutoContrastMin;

                    cb_GS_BackGround.SelectedIndex = cb_GS_BackGround.FindString(Global.recipeGlass_Shm[m_nCamNo].strBackGround);
                    cb_GS_Metric    .SelectedIndex = cb_GS_Metric    .FindString(Global.recipeGlass_Shm[m_nCamNo].strMetric    );
                    cb_GS_SubPixel  .SelectedIndex = cb_GS_SubPixel  .FindString(Global.recipeGlass_Shm[m_nCamNo].strSubPixel  );
                    cb_GS_Dir       .SelectedIndex = Global.recipeGlass_Shm[m_nCamNo].bDirLeft ? 0 : 1;
                }
                else
                {
                    Global.recipeGlass_Shm[m_nCamNo].dXSize       = Convert.ToDouble(nud_GS_X_Size     .Value);
                    Global.recipeGlass_Shm[m_nCamNo].dYSize       = Convert.ToDouble(nud_GS_Y_Size     .Value);
                    Global.recipeGlass_Shm[m_nCamNo].dDepth       = Convert.ToDouble(nud_GS_Depth      .Value);
                    Global.recipeGlass_Shm[m_nCamNo].dAngleMin    = Convert.ToDouble(nud_GS_Angle_Min  .Value);
                    Global.recipeGlass_Shm[m_nCamNo].dAngleMax    = Convert.ToDouble(nud_GS_Angle_Max  .Value);
                    Global.recipeGlass_Shm[m_nCamNo].dMaxOverlap  = Convert.ToDouble(nud_GS_MaxOverlap .Value);
                    Global.recipeGlass_Shm[m_nCamNo].dMinScore    = Convert.ToDouble(nud_GS_MinScore   .Value);
                    Global.recipeGlass_Shm[m_nCamNo].nNumLevels   = Convert.ToInt32 (nud_GS_NumLevel   .Value);
                    Global.recipeGlass_Shm[m_nCamNo].dScaleMin    = Convert.ToDouble(nud_GS_Scale_Min  .Value);
                    Global.recipeGlass_Shm[m_nCamNo].dScaleMax    = Convert.ToDouble(nud_GS_Scale_Max  .Value);
                    Global.recipeGlass_Shm[m_nCamNo].dContrast    = Convert.ToDouble(nud_GS_Contrast   .Value);
                    Global.recipeGlass_Shm[m_nCamNo].dContrastMin = Convert.ToDouble(nud_GS_ContrastMin.Value);
                    Global.recipeGlass_Shm[m_nCamNo].dGreedNess   = Convert.ToDouble(nud_GS_Greedness  .Value);

                    Global.recipeGlass_Shm[m_nCamNo].bAutoContrast    = cb_GS_Contrast   .Checked ;
                    Global.recipeGlass_Shm[m_nCamNo].bAutoContrastMin = cb_GS_ContrastMin.Checked ;

                    if (cb_GS_BackGround.SelectedIndex >= 0)
                        Global.recipeGlass_Shm[m_nCamNo].strBackGround = cb_GS_BackGround.Items[cb_GS_BackGround.SelectedIndex].ToString();
                    if (cb_GS_Metric.SelectedIndex >= 0)
                        Global.recipeGlass_Shm[m_nCamNo].strMetric     = cb_GS_Metric    .Items[cb_GS_Metric    .SelectedIndex].ToString();
                    if (cb_GS_SubPixel.SelectedIndex >= 0)
                        Global.recipeGlass_Shm[m_nCamNo].strSubPixel   = cb_GS_SubPixel  .Items[cb_GS_SubPixel  .SelectedIndex].ToString();

                    Global.recipeGlass_Shm[m_nCamNo].bDirLeft = cb_GS_Dir.SelectedIndex == 0 ? true : false;

                    Global.recipeGlass_Shm[m_nCamNo] = Global.IsLoad_List(IsLoad, Global.recipeGlass_Shm[m_nCamNo], strPath);
                }
            }
            catch (Exception e)
            {

            }
        }


        private void IsLoad(bool IsLoad, string strPath, string strModel)
        {
            //switch (matching_Type)
            //{
            //    case MatchingType.MarkGray:
            //        IsLoadMaskG(IsLoad, strPath);
            //        break;
            //
            //    case MatchingType.MARK_SHAPE:
            //        IsLoadMaskS(IsLoad, strPath);
            //        break;
            //
            //    case MatchingType.GlassShape:
            //        IsLoadGlassS(IsLoad, strPath);
            //        break;
            //}
            //
            //Global.Sys_Param.strCurrModel[(int)matching_Type] = strModel;
            //Global.Sys_Param = Global.IsLoad_List(false, Global.Sys_Param);
            //
            //tb_CurrName.Text = strModel;
        }

        #endregion
    }
}
