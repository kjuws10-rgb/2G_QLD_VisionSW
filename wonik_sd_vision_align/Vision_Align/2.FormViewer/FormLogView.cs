using HalconDotNet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using uEye.Defines.IO;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Vision_Align
{
    public partial class FormLogView : Form
    {
        public FormLogView()
        {
            InitializeComponent();
        }

        public void Initializ(Color? color = null)
        {
            FormDisposition(color);
            SetStyle();



            Global.formHDisplay[(int)HWindowType.LOG_1] = new FormHalconDisplay(0); 
            Global.formHDisplay[(int)HWindowType.LOG_1].TopLevel = false;

            panel_Display1.Controls.Clear();
            panel_Display1.Controls.Add(Global.formHDisplay[(int)HWindowType.LOG_1]);

            Global.formHDisplay[(int)HWindowType.LOG_1].Show();
            Global.formHDisplay[(int)HWindowType.LOG_1].Dock = DockStyle.Fill;

            Global.formHDisplay[(int)HWindowType.LOG_2] = new FormHalconDisplay(1);
            Global.formHDisplay[(int)HWindowType.LOG_2].TopLevel = false;

            panel_Display2.Controls.Clear();
            panel_Display2.Controls.Add(Global.formHDisplay[(int)HWindowType.LOG_2]);

            Global.formHDisplay[(int)HWindowType.LOG_2].Show();
            Global.formHDisplay[(int)HWindowType.LOG_2].Dock = DockStyle.Fill;
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
            //btn_Log_Folder.Font = font;
        }
        #endregion

        public void ShowViewr()
        {
            GetFolder(Global.strRsltPath);

            Show();
        }

        public void HideViewer()
        {
            Hide();
        }

        private void GetFolder(string strPath)
        {
            try
            {
                string strExtension = "";
                string[] files = Directory.GetFiles(strPath, "*AlignDataResult.csv", SearchOption.AllDirectories);

                gridView_FileData.SelectionChanged -= new System.EventHandler(this.gridView_FileData_SelectionChanged);
                
                gridView_FileData.Rows.Clear();

                gridView_FileData.DefaultCellStyle.Font = new Font("Arial", 9);

                //if (matching_Type == Matching_Type.GLASS_SHAPE) strExtension = ".shm";
                //else strExtension = ".ncm";

                for (int i = 0; i < files.Length; i++)
                {
                    if (!files[i].Contains(strExtension)) continue;
                    FileInfo info = new FileInfo(files[i]);

                    gridView_FileData.RowCount++;
                    gridView_FileData.Rows[gridView_FileData.RowCount - 1].Cells[0].Value = info.Name;
                    gridView_FileData.Rows[gridView_FileData.RowCount - 1].Cells[1].Value = info.FullName;
                    gridView_FileData.Rows[gridView_FileData.RowCount - 1].Cells[2].Value = info.LastWriteTime;
                    gridView_FileData.CurrentCell = null;

                    gridView_FileData.Rows[gridView_FileData.Rows.Count - 1].Height = 30;
                }

                gridView_FileData.SelectionChanged += new System.EventHandler(this.gridView_FileData_SelectionChanged);

                gridView_FileData.Rows[gridView_FileData.Rows.Count - 1].Selected = true;
            }
            catch { }
        }


        private void gridView_CsvData_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                HTuple hWindow1 = Global.formHDisplay[(int)HWindowType.LOG_1].HWindow;
                HTuple hWindow2 = Global.formHDisplay[(int)HWindowType.LOG_2].HWindow;
                HOperatorSet.ClearWindow(hWindow1);
                HOperatorSet.ClearWindow(hWindow2);

                if (gridView_CsvData.SelectedRows.Count == 0)
                    return;

                DataGridViewRow selectedRow = gridView_CsvData.SelectedRows[0];

                // Image Show
                string cam1Path = selectedRow.Cells[32].Value.ToString().Replace(" ", "");
                string cam2Path = selectedRow.Cells[47].Value.ToString().Replace(" ", "");

                string rootPath = Path.Combine(Global.strRsltPath, "Image", cam1Path.Split('_')[3]);

                HImage hImage1 = new HImage();
                HImage hImage2 = new HImage();

                hImage1.ReadImage(rootPath + "\\" + cam1Path);
                hImage2.ReadImage(rootPath + "\\" + cam2Path);

                HOperatorSet.DispObj(hImage1, hWindow1);
                HOperatorSet.DispObj(hImage2, hWindow2);


                //인식 좌표 표현
                double Cam1glassRow;
                double Cam1glassCol;
                double Cam1maskRow;
                double Cam1maskCol;
                double Cam2glassRow;
                double Cam2glassCol;
                double Cam2maskRow;
                double Cam2maskCol;

                if (!double.TryParse(selectedRow.Cells[20].Value.ToString(), out Cam1glassRow)) Cam1glassRow = -1;
                if (!double.TryParse(selectedRow.Cells[21].Value.ToString(), out Cam1glassCol)) Cam1glassCol = -1;
                if (!double.TryParse(selectedRow.Cells[26].Value.ToString(), out Cam1maskRow)) Cam1maskRow = -1;
                if (!double.TryParse(selectedRow.Cells[27].Value.ToString(), out Cam1maskCol)) Cam1maskCol = -1;
                if (!double.TryParse(selectedRow.Cells[35].Value.ToString(), out Cam2glassRow)) Cam2glassRow = -1;
                if (!double.TryParse(selectedRow.Cells[36].Value.ToString(), out Cam2glassCol)) Cam2glassCol = -1;
                if (!double.TryParse(selectedRow.Cells[41].Value.ToString(), out Cam2maskRow)) Cam2maskRow = -1;
                if (!double.TryParse(selectedRow.Cells[42].Value.ToString(), out Cam2maskCol)) Cam2maskCol = -1;

                if (Cam1glassRow != -1 && Cam1glassCol != -1)
                    ClsAlgorithm.OverlayCross(hWindow1, Cam1glassCol, Cam1glassRow, 0, "green");
                if (Cam1maskRow != -1 && Cam1maskCol != -1)
                    ClsAlgorithm.OverlayCross(hWindow1, Cam1maskCol, Cam1maskRow, 0, "cyan");
                if (Cam2glassRow != -1 && Cam2glassCol != -1)
                    ClsAlgorithm.OverlayCross(hWindow2, Cam2glassCol, Cam2glassRow,  0, "green");
                if (Cam2maskRow != -1 && Cam2maskCol != -1)
                    ClsAlgorithm.OverlayCross(hWindow2, Cam2maskCol, Cam2maskRow,  0, "cyan");


            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString(), "Failed", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void gridView_FileData_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                if (gridView_FileData.SelectedRows.Count == 0)
                    return;

                string strTemp = gridView_FileData.SelectedRows[0].Cells[1].Value.ToString();

                string strPath = strTemp;

                DataTable dt = new DataTable();

                bool isFirstLine = true;

                gridView_CsvData.RowTemplate.Height = 30;
                gridView_CsvData.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
                gridView_CsvData.DefaultCellStyle.Font = new Font("Arial", 9);
                gridView_CsvData.MultiSelect = false;

                foreach (string line in File.ReadLines(strPath))
                {
                    Console.WriteLine(line);

                    // 한 줄씩 처리하는 공간
                    string[] cols = line.Split(',');
                    if (isFirstLine)
                    {
                        for (int i = 0; i < cols.Length; i++)
                        {
                            dt.Columns.Add(cols[i]);
                        }
                        isFirstLine = false;
                    }
                    else
                    {
                        dt.Rows.Add(cols);
                    }
                }
                gridView_CsvData.SelectionChanged -= new System.EventHandler(this.gridView_CsvData_SelectionChanged);
                gridView_CsvData.DataSource = dt;
                gridView_CsvData.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;


                gridView_CsvData.SelectionChanged += new System.EventHandler(this.gridView_CsvData_SelectionChanged);
                gridView_CsvData.Rows[gridView_CsvData.Rows.Count - 1].Selected = true;
                //List<string> listJudge = Global.clsCSV.ReadCsvColumn(strPath, "Judge");

                //Global.clsAlgorithm[0].StageCal(1.8, 0.023, 4.1, out double dx1, out double dx2, out double dx3, out double dx4);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Failed", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

        }
    }
}
