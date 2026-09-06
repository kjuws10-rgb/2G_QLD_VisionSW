using HalconDotNet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using VoAlgorithm;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;


namespace Vision_Align
{
    public partial class FormPreview : Form
    {
        FormHalconDisplay HalconDisplay;
        Timer m_timer = new Timer();
        int m_nCamNo = 0;

        int m_nFileCount = 0;

        private List<AlignResult> _allData = new List<AlignResult>();
        private List<AlignResult> _filteredData = new List<AlignResult>();
        private string _currentCsvPath = "";
        private DateTime _currentCsvLastWriteTime = DateTime.MinValue;

        HImage _hEmptyImage = new HImage();


        public class AlignResult
        {
            public DateTime Time { get; set; }
            public bool AlignOk { get; set; }
            public double? GlassX { get; set; }
            public double? GlassY { get; set; }
            public double? SlipX { get; set; }
            public double? SlipY { get; set; }
        }

        public FormPreview()
        {
            InitializeComponent();
        }

        public void Initializ(int nCamNo)
        {
            FormDisposition();
            SetStyle();

            m_nCamNo = nCamNo;
            SetTitle(m_nCamNo);

            HalconDisplay = Global.formHDisplay[(int)HWindowType.PRE_1 + m_nCamNo];
            HalconDisplay = new FormHalconDisplay(m_nCamNo);
            HalconDisplay.TopLevel = false;

            panel_Display.Controls.Clear();
            panel_Display.Controls.Add(HalconDisplay);

            HalconDisplay.Show();
            HalconDisplay.Dock = DockStyle.Fill;

            m_timer.Interval = 10000;
            m_timer.Tick += new EventHandler(OnTimer);
            m_timer.Start();

            gridView_LogDate.CellClick -= GridView_LogDate_CellClick;
            gridView_LogDate.CellClick += GridView_LogDate_CellClick;
            cb_Align_Ok.CheckedChanged -= Filter_CheckedChanged;
            cb_Align_Ok.CheckedChanged += Filter_CheckedChanged;

            cb_Glass_Loding_Pos.CheckedChanged -= Filter_CheckedChanged;
            cb_Glass_Loding_Pos.CheckedChanged += Filter_CheckedChanged;




            _hEmptyImage.ReadImage("Images\\EmptyImage.bmp");

            HOperatorSet.DispObj(_hEmptyImage, HalconDisplay.HWindow);

            


            GetFolder(Global.strRsltPath);
            m_nFileCount = GetFileCount(Global.strRsltPath);
        }



        private void Filter_CheckedChanged(object sender, EventArgs e)
        {
            ApplyAndRender();
        }

        void OnTimer(object obj, EventArgs ea)
        {
            if(!Global.bAutoMode)
            {
                int nCount = GetFileCount(Global.strRsltPath);
                if (m_nFileCount != nCount)
                {
                    GetFolder(Global.strRsltPath);
                    m_nFileCount = nCount;
                }
                RefreshCurrentCsvIfChanged();

            }
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
            
        }
        #endregion


        public void ShowViewr()
        {
            this.Show();
        }

        public void HideViewer()
        {
            this.Hide();
        }

        private void SetTitle(int nCamNo)
        {
            label_Title.Text = string.Format("Pre Review Cam#" + (nCamNo + 1).ToString());
        }


        private int GetFileCount(string strPath)
        {
            string[] files = Directory.GetFiles(strPath, "*AlignDataResult.csv", SearchOption.AllDirectories);

            return files.Length;
        }

        private void GetFolder(string strPath)
        {
            try
            {
                string strExtension = "";
                string[] files = Directory.GetFiles(strPath, "*AlignDataResult.csv", SearchOption.AllDirectories);

                gridView_LogDate.Rows.Clear();

                gridView_LogDate.DefaultCellStyle.Font = new Font("Arial", 9);

                //if (matching_Type == MatchingType.GlassShape) strExtension = ".shm";
                //else strExtension = ".ncm";

                for (int i = 0; i < files.Length; i++)
                {
                    if (!files[i].Contains(strExtension)) continue;
                    FileInfo info = new FileInfo(files[i]);

                    gridView_LogDate.RowCount++;
                    gridView_LogDate.Rows[gridView_LogDate.RowCount - 1].Cells[0].Value = info.Name;
                    gridView_LogDate.Rows[gridView_LogDate.RowCount - 1].Tag = info.FullName;
                    gridView_LogDate.CurrentCell = null;

                    gridView_LogDate.Rows[gridView_LogDate.Rows.Count - 1].Height = 30;
                }
                gridView_LogDate.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            }
            catch { }
        }

        private void RefreshCurrentCsvIfChanged()
        {
            if (string.IsNullOrWhiteSpace(_currentCsvPath) || !File.Exists(_currentCsvPath))
                return;

            DateTime lastWrite;
            try
            {
                lastWrite = File.GetLastWriteTime(_currentCsvPath);
            }
            catch
            {
                return;
            }

            if (_currentCsvLastWriteTime == lastWrite)
                return;

            _currentCsvLastWriteTime = lastWrite;

            _allData = LoadAlignCsv(_currentCsvPath);
            ApplyAndRender();
        }

        private void GridView_LogDate_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            _currentCsvPath = gridView_LogDate.Rows[e.RowIndex].Tag?.ToString();
            if (string.IsNullOrWhiteSpace(_currentCsvPath) || !File.Exists(_currentCsvPath))
                return;

            _currentCsvLastWriteTime = File.GetLastWriteTime(_currentCsvPath);
            _allData = LoadAlignCsv(_currentCsvPath);
            ApplyAndRender();
        }


        private void ApplyAndRender()
        {
            if (_allData == null || _allData.Count == 0) return;

            if (!cb_Align_Ok.Checked && !cb_Glass_Loding_Pos.Checked )
            {
                _filteredData = new List<AlignResult>();
                RenderToHalcon(_filteredData);   // 내부에서 ClearWindow 하고 종료함
                return;
            }

            IEnumerable<AlignResult> q = _allData;

            if (cb_Align_Ok.Checked)
                q = q.Where(x => x.AlignOk);

            if (cb_Glass_Loding_Pos.Checked)
                q = q.Where(x => x.GlassX.HasValue || x.GlassY.HasValue);


            _filteredData = q.ToList();

            RenderToHalcon(_filteredData);
        }

        private void RenderToHalcon(List<AlignResult> data)
        {
            var hDisplay = HalconDisplay;
            if (hDisplay == null)
                return;

            HTuple hWindow = hDisplay.HWindow;

            // 항상 화면 클리어 먼저
            HOperatorSet.ClearWindow(hWindow);
            HOperatorSet.DispObj(_hEmptyImage, hWindow);

            // 데이터 없으면 여기서 종료
            if (data == null || data.Count == 0)
                return;

            HOperatorSet.SetLineWidth(hWindow, 1);

            foreach (var d in data)
            {
                // Glass Loading Pos 표시 (체크됐을 때만 그리기)
                if ((cb_Glass_Loding_Pos.Checked && !cb_Align_Ok.Checked) && d.GlassX.HasValue && d.GlassY.HasValue)
                {
                    HOperatorSet.SetColor(hWindow, "green");
                    HOperatorSet.DispCross(hWindow, d.GlassY.Value, d.GlassX.Value, 30, 0);
                }

                // Glass Loading Pos 표시 (체크됐을 때만 그리기)
                if (( cb_Align_Ok.Checked) && d.GlassX.HasValue && d.GlassY.HasValue)
                {
                    HOperatorSet.SetColor(hWindow, "yellow");
                    HOperatorSet.DispCross(hWindow, d.GlassY.Value, d.GlassX.Value, 30, 0);
                }

            }
        }
        private List<AlignResult> LoadAlignCsv(string path)
        {
            var list = new List<AlignResult>();

            if (!File.Exists(path))
                return list;

            var lines = File.ReadAllLines(path);
            if (lines.Length < 2)
                return list;

            // 1) 헤더 파싱 (Trim해서 앞 공백 문제 제거)
            var headers = lines[0].Split(',')
                                  .Select(h => h.Trim())
                                  .ToArray();

            var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < headers.Length; i++)
            {
                // 같은 헤더 중복 대비: 최초 1개만 등록
                if (!map.ContainsKey(headers[i]))
                    map[headers[i]] = i;
            }

            // 2) 사용할 컬럼명 구성
            //    FormPreview에서 nCamNo=0이면 "Pre Review Cam#1" 이므로 CSV의 Cam1 컬럼을 쓰는 게 자연스러움
            int camNoForCsv = m_nCamNo + 1;

            string colTime = "DateTime";
            string colJudge = "Judge";

            string colGlassRowPx = $"Cam{camNoForCsv} Glass Row[pixel]";
            string colGlassColPx = $"Cam{camNoForCsv} Glass Col[pixel]";

            string colErrX = "Final ErrX[um]";
            string colErrY = "Final ErrY[um]";

            // 3) 데이터 라인 파싱
            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i]))
                    continue;

                var col = lines[i].Split(',');

                if (col.Length < headers.Length)
                    continue;

                // DateTime
                DateTime time = DateTime.MinValue;
                {
                    string raw = GetString(col, map, colTime);

                    // 예: [2026/01/03 18:48:48:291]
                    // 괄호 제거 후 마지막 ":"를 "."로 바꿔서 밀리초 파싱을 쉽게 함
                    raw = raw.Trim();
                    raw = raw.TrimStart('[').TrimEnd(']');

                    if (!string.IsNullOrWhiteSpace(raw))
                    {
                        int lastColon = raw.LastIndexOf(':');
                        if (lastColon > 0 && raw.Length - lastColon - 1 <= 3)
                        {
                            // ...ss:fff 형태면 ...ss.fff 로 변환
                            raw = raw.Substring(0, lastColon) + "." + raw.Substring(lastColon + 1);
                        }

                        // 여러 포맷 대응
                        DateTime.TryParseExact(
                            raw,
                            new[]
                            {
                        "yyyy/MM/dd HH:mm:ss.fff",
                        "yyyy-MM-dd HH:mm:ss.fff",
                        "yyyy/MM/dd HH:mm:ss",
                        "yyyy-MM-dd HH:mm:ss"
                            },
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.None,
                            out time
                        );
                    }
                }

                // Align OK (Judge 컬럼이 없으면 일단 true로 둠)
                bool alignOk = true;
                {
                    string judge = GetString(col, map, colJudge);
                    if (!string.IsNullOrWhiteSpace(judge))
                        alignOk = judge.Trim().Equals("OK", StringComparison.OrdinalIgnoreCase);
                }

                // Glass Loading Pos (CamN Glass Row/Col[um])
                double? glassY = GetNullableDouble(col, map, colGlassRowPx);
                double? glassX = GetNullableDouble(col, map, colGlassColPx);
                double? slipX = GetNullableDouble(col, map, colErrX);
                double? slipY = GetNullableDouble(col, map, colErrY);

                list.Add(new AlignResult
                {
                    Time = time,
                    AlignOk = alignOk,
                    GlassX = glassX,
                    GlassY = glassY,
                    SlipX = slipX,
                    SlipY = slipY
                });
            }

            return list;
        }

        private static string GetString(string[] col, Dictionary<string, int> map, string headerName)
        {
            if (!map.TryGetValue(headerName.Trim(), out int idx))
                return "";

            if (idx < 0 || idx >= col.Length)
                return "";

            return col[idx]?.Trim() ?? "";
        }

        private static double? GetNullableDouble(string[] col, Dictionary<string, int> map, string headerName)
        {
            string s = GetString(col, map, headerName);

            // NG에서 빈칸/공백/NaN이면 "값 없음"
            if (string.IsNullOrWhiteSpace(s))
                return null;

            if (s.Equals("NaN", StringComparison.OrdinalIgnoreCase))
                return null;

            // 숫자 파싱 (로케일 영향 방지)
            if (double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out double v))
                return v;

            // 혹시 로컬 포맷일 때 한 번 더
            if (double.TryParse(s, NumberStyles.Float, CultureInfo.CurrentCulture, out v))
                return v;

            return null;
        }
    }
}
