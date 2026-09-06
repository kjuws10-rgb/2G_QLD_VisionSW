using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using VoAlgorithm;

namespace Vision_Align
{
    public partial class FormAlgorithmTest : Form
    {
        private VisionAlgorithm m_Algorithm = new VisionAlgorithm();

        // FitCircle canvas data
        private List<PointF> m_FitCirclePoints = new List<PointF>();
        private PointF? m_FitCircleCenter = null;
        private double m_FitCircleRadius = 0;

        // 9-Point canvas data
        private List<PointF> m_9PointPoints = new List<PointF>();

        public FormAlgorithmTest()
        {
            InitializeComponent();
        }

        private void Log(string message)
        {
            txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\r\n");
            txtLog.SelectionStart = txtLog.Text.Length;
            txtLog.ScrollToCaret();
        }

        #region GetRealPos
        private void btnGetRealPos_Click(object sender, EventArgs e)
        {
            try
            {
                int camWidth = int.Parse(txtCamWidth.Text);
                int camHeight = int.Parse(txtCamHeight.Text);
                double calOffsetX = double.Parse(txtCalOffsetX.Text);
                double calOffsetY = double.Parse(txtCalOffsetY.Text);
                double calTilt = double.Parse(txtCalTilt.Text);
                double pixelResX = double.Parse(txtPixelResX.Text);
                double pixelResY = double.Parse(txtPixelResY.Text);
                double camPosX = double.Parse(txtCamPosX.Text);
                double camPosY = double.Parse(txtCamPosY.Text);

                bool result = m_Algorithm.GetRealPos(
                    camWidth, camHeight,
                    calOffsetX, calOffsetY, calTilt,
                    pixelResX, pixelResY,
                    camPosX, camPosY,
                    out double realPosX, out double realPosY);

                if (result)
                {
                    txtRealPosX.Text = realPosX.ToString("F6");
                    txtRealPosY.Text = realPosY.ToString("F6");
                    Log($"GetRealPos: ({camPosX}, {camPosY}) -> ({realPosX:F6}, {realPosY:F6})");
                }
                else
                {
                    Log("GetRealPos: Failed");
                }
            }
            catch (Exception ex)
            {
                Log($"GetRealPos Error: {ex.Message}");
            }
        }
        #endregion

        #region StageCal
        private void btnStageCal_Click(object sender, EventArgs e)
        {
            try
            {
                double x = double.Parse(txtStageX.Text);
                double y = double.Parse(txtStageY.Text);
                double angle = double.Parse(txtStageAngle.Text);

                double[] pinDeg = ParseDoubleArray(txtPinDeg.Text);
                double[] stageDeg = ParseDoubleArray(txtStageDeg.Text);
                double radius = double.Parse(txtRadius.Text);

                if (pinDeg.Length != 4 || stageDeg.Length != 4)
                {
                    Log("StageCal: PinDeg and StageDeg must have 4 values each");
                    return;
                }

                m_Algorithm.StageBasicInfo(pinDeg, stageDeg, radius);
                VisionAlgorithm.StageCal(x, y, angle, out double x1, out double x2, out double y1, out double y2);

                txtX1.Text = x1.ToString("F6");
                txtX2.Text = x2.ToString("F6");
                txtY1.Text = y1.ToString("F6");
                txtY2.Text = y2.ToString("F6");

                Log($"StageCal: (X={x}, Y={y}, Angle={angle}) -> (X1={x1:F6}, X2={x2:F6}, Y1={y1:F6}, Y2={y2:F6})");
            }
            catch (Exception ex)
            {
                Log($"StageCal Error: {ex.Message}");
            }
        }

        private void btnStageCalInverse_Click(object sender, EventArgs e)
        {
            try
            {
                double x1 = double.Parse(txtX1.Text);
                double x2 = double.Parse(txtX2.Text);
                double y1 = double.Parse(txtY1.Text);

                double[] pinDeg = ParseDoubleArray(txtPinDeg.Text);
                double[] stageDeg = ParseDoubleArray(txtStageDeg.Text);
                double radius = double.Parse(txtRadius.Text);

                if (pinDeg.Length != 4 || stageDeg.Length != 4)
                {
                    Log("StageCalInverse: PinDeg and StageDeg must have 4 values each");
                    return;
                }

                m_Algorithm.StageBasicInfo(pinDeg, stageDeg, radius);
                bool result = VisionAlgorithm.StageCalInverse(x1, x2, y1, out double x, out double y, out double angle);

                if (result)
                {
                    txtStageX.Text = x.ToString("F6");
                    txtStageY.Text = y.ToString("F6");
                    txtStageAngle.Text = angle.ToString("F6");

                    Log($"StageCalInverse: (X1={x1}, X2={x2}, Y1={y1}) -> (X={x:F6}, Y={y:F6}, Angle={angle:F6})");
                }
                else
                {
                    Log("StageCalInverse: Failed");
                }
            }
            catch (Exception ex)
            {
                Log($"StageCalInverse Error: {ex.Message}");
            }
        }

        private double[] ParseDoubleArray(string text)
        {
            string[] parts = text.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            double[] result = new double[parts.Length];
            for (int i = 0; i < parts.Length; i++)
            {
                result[i] = double.Parse(parts[i].Trim(), CultureInfo.InvariantCulture);
            }
            return result;
        }
        #endregion

        #region FitCircle / CalAngle
        private void btnFitCircle_Click(object sender, EventArgs e)
        {
            try
            {
                List<PointF> points = ParsePoints(txtPoints.Text);

                if (points.Count < 3)
                {
                    Log("FitCircle: At least 3 points required");
                    return;
                }

                VisionAlgorithm.FitCircle(points, out PointF center, out double radius);

                txtCenterX.Text = center.X.ToString("F4");
                txtCenterY.Text = center.Y.ToString("F4");
                txtCircleRadius.Text = radius.ToString("F4");

                // Update canvas visualization
                m_FitCircleCenter = center;
                m_FitCircleRadius = radius;
                m_FitCirclePoints = new List<PointF>(points);
                picFitCircle.Invalidate();

                Log($"FitCircle: {points.Count} points -> Center=({center.X:F4}, {center.Y:F4}), Radius={radius:F4}");
            }
            catch (Exception ex)
            {
                Log($"FitCircle Error: {ex.Message}");
            }
        }

        private void btnCalAngle_Click(object sender, EventArgs e)
        {
            try
            {
                List<PointF> points = ParsePoints(txtPoints.Text);

                if (points.Count < 2)
                {
                    Log("CalAngle: At least 2 points required (P1, P2)");
                    return;
                }

                PointF p1 = points[0];
                PointF p2 = points[1];

                double angle = m_Algorithm.CalculateAngle(p1, p2);

                txtAngleResult.Text = angle.ToString("F4");

                Log($"CalAngle: P1=({p1.X}, {p1.Y}), P2=({p2.X}, {p2.Y}) -> Angle={angle:F4} deg");
            }
            catch (Exception ex)
            {
                Log($"CalAngle Error: {ex.Message}");
            }
        }

        private List<PointF> ParsePoints(string text)
        {
            List<PointF> points = new List<PointF>();
            string[] lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in lines)
            {
                string[] parts = line.Split(new[] { ',', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                {
                    float x = float.Parse(parts[0].Trim(), CultureInfo.InvariantCulture);
                    float y = float.Parse(parts[1].Trim(), CultureInfo.InvariantCulture);
                    points.Add(new PointF(x, y));
                }
            }

            return points;
        }
        #endregion

        #region 9-Point Calibration
        private void btn9PointCal_Click(object sender, EventArgs e)
        {
            try
            {
                double offsetMove = double.Parse(txtOffsetMove.Text);
                List<PointF> pointList = ParsePoints(txt9Points.Text);

                if (pointList.Count != 9)
                {
                    Log($"9PointCal: Exactly 9 points required (got {pointList.Count})");
                    return;
                }

                PointF[] points = pointList.ToArray();

                bool result = m_Algorithm.Calculate9PointCalibration(
                    offsetMove, points,
                    out double pixelResX, out double pixelResY, out double tilt);

                if (result)
                {
                    txtPixelResXOut.Text = pixelResX.ToString("F6");
                    txtPixelResYOut.Text = pixelResY.ToString("F6");
                    txtTiltOut.Text = tilt.ToString("F4");

                    Log($"9PointCal: PixelResX={pixelResX:F6}, PixelResY={pixelResY:F6}, Tilt={tilt:F4} deg");
                }
                else
                {
                    Log("9PointCal: Failed");
                }
            }
            catch (Exception ex)
            {
                Log($"9PointCal Error: {ex.Message}");
            }
        }
        #endregion

        #region FitCircle Canvas
        private void picFitCircle_MouseClick(object sender, MouseEventArgs e)
        {
            m_FitCirclePoints.Add(new PointF(e.X, e.Y));
            UpdateFitCircleTextBox();
            picFitCircle.Invalidate();
            Log($"FitCircle Canvas: Added point ({e.X}, {e.Y}), Total: {m_FitCirclePoints.Count}");
        }

        private void picFitCircle_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Draw grid
            using (Pen gridPen = new Pen(Color.FromArgb(50, 50, 55)))
            {
                for (int x = 0; x < picFitCircle.Width; x += 50)
                    g.DrawLine(gridPen, x, 0, x, picFitCircle.Height);
                for (int y = 0; y < picFitCircle.Height; y += 50)
                    g.DrawLine(gridPen, 0, y, picFitCircle.Width, y);
            }

            // Draw clicked points
            using (Brush pointBrush = new SolidBrush(Color.Yellow))
            {
                foreach (PointF pt in m_FitCirclePoints)
                {
                    g.FillEllipse(pointBrush, pt.X - 4, pt.Y - 4, 8, 8);
                }
            }

            // Draw fitted circle if available
            if (m_FitCircleCenter.HasValue && m_FitCircleRadius > 0)
            {
                using (Pen circlePen = new Pen(Color.Cyan, 2))
                {
                    float r = (float)m_FitCircleRadius;
                    g.DrawEllipse(circlePen, m_FitCircleCenter.Value.X - r, m_FitCircleCenter.Value.Y - r, r * 2, r * 2);
                }

                // Draw center point
                using (Brush centerBrush = new SolidBrush(Color.Red))
                {
                    g.FillEllipse(centerBrush, m_FitCircleCenter.Value.X - 5, m_FitCircleCenter.Value.Y - 5, 10, 10);
                }

                // Draw center crosshair
                using (Pen crossPen = new Pen(Color.Red, 1))
                {
                    g.DrawLine(crossPen, m_FitCircleCenter.Value.X - 15, m_FitCircleCenter.Value.Y, m_FitCircleCenter.Value.X + 15, m_FitCircleCenter.Value.Y);
                    g.DrawLine(crossPen, m_FitCircleCenter.Value.X, m_FitCircleCenter.Value.Y - 15, m_FitCircleCenter.Value.X, m_FitCircleCenter.Value.Y + 15);
                }
            }
        }

        private void btnFitCircleClear_Click(object sender, EventArgs e)
        {
            m_FitCirclePoints.Clear();
            m_FitCircleCenter = null;
            m_FitCircleRadius = 0;
            txtPoints.Clear();
            txtCenterX.Clear();
            txtCenterY.Clear();
            txtCircleRadius.Clear();
            txtAngleResult.Clear();
            picFitCircle.Invalidate();
            Log("FitCircle Canvas: Cleared");
        }

        private void UpdateFitCircleTextBox()
        {
            txtPoints.Clear();
            foreach (PointF pt in m_FitCirclePoints)
            {
                txtPoints.AppendText($"{pt.X}, {pt.Y}\r\n");
            }
        }
        #endregion

        #region 9-Point Canvas
        private void pic9Point_MouseClick(object sender, MouseEventArgs e)
        {
            if (m_9PointPoints.Count >= 9)
            {
                Log("9-Point Canvas: Maximum 9 points allowed. Clear to add new points.");
                return;
            }

            m_9PointPoints.Add(new PointF(e.X, e.Y));
            Update9PointTextBox();
            pic9Point.Invalidate();
            Log($"9-Point Canvas: Added point ({e.X}, {e.Y}), Total: {m_9PointPoints.Count}/9");
        }

        private void pic9Point_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Draw grid
            using (Pen gridPen = new Pen(Color.FromArgb(50, 50, 55)))
            {
                for (int x = 0; x < pic9Point.Width; x += 50)
                    g.DrawLine(gridPen, x, 0, x, pic9Point.Height);
                for (int y = 0; y < pic9Point.Height; y += 50)
                    g.DrawLine(gridPen, 0, y, pic9Point.Width, y);
            }

            // Draw 3x3 grid pattern guide
            using (Pen guidePen = new Pen(Color.FromArgb(80, 80, 90), 1) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash })
            {
                int margin = 30;
                int w = pic9Point.Width - margin * 2;
                int h = pic9Point.Height - margin * 2;
                for (int i = 0; i <= 2; i++)
                {
                    int x = margin + (w / 2) * i;
                    int y = margin + (h / 2) * i;
                    g.DrawLine(guidePen, x, margin, x, pic9Point.Height - margin);
                    g.DrawLine(guidePen, margin, y, pic9Point.Width - margin, y);
                }
            }

            // Draw clicked points with numbers
            using (Brush pointBrush = new SolidBrush(Color.Lime))
            using (Font font = new Font("Arial", 8, FontStyle.Bold))
            using (Brush textBrush = new SolidBrush(Color.White))
            {
                for (int i = 0; i < m_9PointPoints.Count; i++)
                {
                    PointF pt = m_9PointPoints[i];
                    g.FillEllipse(pointBrush, pt.X - 5, pt.Y - 5, 10, 10);
                    g.DrawString((i + 1).ToString(), font, textBrush, pt.X + 8, pt.Y - 6);
                }
            }
        }

        private void btn9PointClear_Click(object sender, EventArgs e)
        {
            m_9PointPoints.Clear();
            txt9Points.Clear();
            txtPixelResXOut.Clear();
            txtPixelResYOut.Clear();
            txtTiltOut.Clear();
            pic9Point.Invalidate();
            Log("9-Point Canvas: Cleared");
        }

        private void Update9PointTextBox()
        {
            txt9Points.Clear();
            foreach (PointF pt in m_9PointPoints)
            {
                txt9Points.AppendText($"{pt.X}, {pt.Y}\r\n");
            }
        }
        #endregion
    }
}
