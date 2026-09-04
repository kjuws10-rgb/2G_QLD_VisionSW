using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HalconDotNet;

namespace Vision_Align
{
    public partial class FormHalconDisplay : Form
    {
        int m_nCamNo = 0;
        bool m_bContext = false;

        HObject ho_Image;
        HTuple hv_WindowHandle;
        HTuple hv_Width, hv_Height;

        private ContextMenuStrip contextMenu;

        
        public HTuple HWindow { get { return hv_WindowHandle; } }

        public FormHalconDisplay(int nCamNo, bool bContextMenu = false)
        {
            InitializeComponent();
            InitializeContextMenu();

            m_nCamNo = nCamNo;
            m_bContext = bContextMenu;

            hv_WindowHandle = hControl_Display.HalconWindow;


            // 이벤트 핸들 연결
            this.MouseWheel += hControl_Display.HSmartWindowControl_MouseWheel;
            hControl_Display.HMouseDown += HWindowControl_MouseDown;

            ClsAlgorithm.FitImage(hv_WindowHandle, 2456, 2054);
        }

        private void HWindowControl_MouseDown(object sender, HMouseEventArgs e)
        {
            if(e.Button == MouseButtons.Right && !Global.bAutoMode)
            {
                if(m_bContext)
                    contextMenu.Show(Cursor.Position.X, Cursor.Position.Y);
                
            }
        }

        private void InitializeContextMenu()
        {
            contextMenu = new ContextMenuStrip();

            // [Create Model]
            var createModel = new ToolStripMenuItem("Create Model");

            // ─────────────── MASK
            var mask = new ToolStripMenuItem("Mask");

            var maskGray = new ToolStripMenuItem("Gray");
            maskGray.Click += (s, e) => Global.formModel.ShowViewer(m_nCamNo, Matching_Type.MARK_GRAY);

            var maskShape = new ToolStripMenuItem("Templete");
            maskShape.Click += (s, e) => Global.formModel.ShowViewer(m_nCamNo, Matching_Type.MASK_SHAPE);

            mask.DropDownItems.Add(maskGray);
            mask.DropDownItems.Add(maskShape);

            // ─────────────── GLASS
            var glass = new ToolStripMenuItem("Glass");

            var glassShape = new ToolStripMenuItem("Shape");
            glassShape.Click += (s, e) => Global.formModel.ShowViewer(m_nCamNo, Matching_Type.GLASS_SHAPE);

            glass.DropDownItems.Add(glassShape);

            // 하위 모델 추가
            createModel.DropDownItems.Add(mask);
            createModel.DropDownItems.Add(glass);

            // [Camera Equip]
            var cameraEquip = new ToolStripMenuItem("Camera Equip");
            cameraEquip.Click += (s, e) => Global.formSetCam.ShowViewer(m_nCamNo); 

            // 메뉴 구성
            contextMenu.Items.Add(createModel);
            contextMenu.Items.Add(cameraEquip);
        }
    }
}
