using HalconDotNet;
using peak.ipl;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using uEye.Types;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrayNotify;

namespace Vision_Align
{
    public class HModelInfo
    {
        public HTuple ModelID;
        public HObject Region;

        public double dRow = 0;
        public double dCol = 0;
    }

    public class ModelRslt
    {
        public double dX = 0;
        public double dY = 0;
        public double dAngle = 0;
        public double dScore = 0;
        public double dScale = 0;
    }

    public class ModelParam
    {
        public int nNumLevels = 0;
        public double dAngleMin = 0;
        public double dAngleMax = 0;
        public double dMaxOverlap = 0;
        public double dMinScore = 0;
        public string strMetric = "";
        public string strSubPixel = "";

        public double dRadius = 0;
        public string strBackGround = "";

        public double dSizeX = 0;
        public double dSizeY = 0;
        public double dDepth = 0;

        public double dGreedNess = 0;

        public double dContrast = 0;
        public double dContrastMin = 0;
        public bool bContrast = false;
        public bool bContrastMin = false;
    }


    public class ClsAlgorithm //LJH
    {
        HObject OriImg;
        public HObject ProcessImg;
        private readonly object _imageSync = new object();
        HTuple m_hDrawID;

        HTuple m_SquareModel, m_triangleModel;

        static double[] m_dPinDeg    = new double[4];
        static double[] m_dStageDeg = new double[4];
        static double m_dRadius  = 0;

        public ClsAlgorithm()
        {

        }

        public static void init()
        {
            HOperatorSet.SetSystem("border_shape_models", "true");
        }

        /// <summary>
        /// Camera 정보와 Calibration 정보를 기반으로 Image의 특정 좌표의 Hardware 좌표를 구한다.
        /// </summary>
        /// <param name="cameraWidth">Camera 가로 Pixel</param>
        /// <param name="cameraHeight">Camera 세로 Pixel</param>
        /// <param name="calOffsetX">Hardware 좌표계의 Camera Center X의 위치</param>
        /// <param name="calOffsetY">Hardware 좌표계의 Camera Center Y의 위치</param>
        /// <param name="calTilt">Camera가 틀어진 각도 (degree)</param>
        /// <param name="pixelResolutionX">X축 Pixel 해상도 (mm/pixel)</param>
        /// <param name="pixelResolutionY">Y축 Pixel 해상도 (mm/pixel)</param>
        /// <param name="cameraPosX">입력 Camera X Pixel 위치</param>
        /// <param name="cameraPosY">입력 Camera Y Pixel 위치</param>
        /// <param name="realPosX">구해진 실제 X 좌표</param>
        /// <param name="realPosY">구해진 실제 Y 좌표</param>
        /// <returns>성공 여부</returns>
        public bool GetRealPos(int cameraWidth, int cameraHeight,
                               double calOffsetX, double calOffsetY, double calTilt,
                               double pixelResolutionX, double pixelResolutionY,
                               double cameraPosX, double cameraPosY,
                               out double realPosX, out double realPosY)
        {
            realPosX = 0;
            realPosY = 0;

            try
            {
                // 1. Camera Center 기준 Pixel Offset 계산
                double deltaPixelX = cameraPosX - (cameraWidth / 2.0);
                double deltaPixelY = cameraPosY - (cameraHeight / 2.0);

                // 2. Pixel → Real 좌표 변환 (mm/pixel 적용)
                double deltaRealX = deltaPixelX * pixelResolutionX;
                double deltaRealY = deltaPixelY * pixelResolutionY;

                // 3. Camera Tilt 보정 (회전 변환)
                double tiltRad = calTilt * Math.PI / 180.0;
                double cos = Math.Cos(tiltRad);
                double sin = Math.Sin(tiltRad);

                double rotatedX = deltaRealX * cos - deltaRealY * sin;
                double rotatedY = deltaRealX * sin + deltaRealY * cos;

                // 4. Hardware 좌표계의 Camera Center Offset 적용
                realPosX = calOffsetX + rotatedX;
                realPosY = calOffsetY + rotatedY;

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 9 Point Calibration으로 PixelResolution과 Tilt를 계산한다.
        /// </summary>
        /// <param name="offsetMove">모터 이동 간격 (mm)</param>
        /// <param name="rawPoints">9개의 Camera Pixel 좌표 (3x3 격자 순서)</param>
        /// <param name="pixelResolutionX">계산된 X축 Pixel 해상도 (mm/pixel)</param>
        /// <param name="pixelResolutionY">계산된 Y축 Pixel 해상도 (mm/pixel)</param>
        /// <param name="tilt">계산된 Camera Tilt 각도 (degree)</param>
        /// <returns>성공 여부</returns>
        public bool Calculate9PointCalibration(double offsetMove, PointF[] rawPoints,
                                                out double pixelResolutionX, out double pixelResolutionY, out double tilt)
        {
            pixelResolutionX = 0;
            pixelResolutionY = 0;
            tilt = 0;

            try
            {
                if (rawPoints == null || rawPoints.Length != 9)
                    return false;

                // 9점 배열 인덱스:
                // 0(−,−)  1(0,−)  2(+,−)
                // 3(−,0)  4(0,0)  5(+,0)
                // 6(−,+)  7(0,+)  8(+,+)

                // X축 방향 픽셀 변화량 계산 (가로 방향 이동)
                // Row 0: Point0 → Point1 → Point2
                // Row 1: Point3 → Point4 → Point5
                // Row 2: Point6 → Point7 → Point8
                double sumDeltaPixelX = 0;
                sumDeltaPixelX += Math.Abs(rawPoints[2].X - rawPoints[0].X);  // Row 0
                sumDeltaPixelX += Math.Abs(rawPoints[5].X - rawPoints[3].X);  // Row 1
                sumDeltaPixelX += Math.Abs(rawPoints[8].X - rawPoints[6].X);  // Row 2
                double avgDeltaPixelX = sumDeltaPixelX / 3.0;

                // Y축 방향 픽셀 변화량 계산 (세로 방향 이동)
                // Col 0: Point0 → Point3 → Point6
                // Col 1: Point1 → Point4 → Point7
                // Col 2: Point2 → Point5 → Point8
                double sumDeltaPixelY = 0;
                sumDeltaPixelY += Math.Abs(rawPoints[6].Y - rawPoints[0].Y);  // Col 0
                sumDeltaPixelY += Math.Abs(rawPoints[7].Y - rawPoints[1].Y);  // Col 1
                sumDeltaPixelY += Math.Abs(rawPoints[8].Y - rawPoints[2].Y);  // Col 2
                double avgDeltaPixelY = sumDeltaPixelY / 3.0;

                // 실제 이동 거리 (2 * offsetMove: 양 끝점 간 거리)
                double actualMoveDistance = 2.0 * offsetMove;

                // Pixel Resolution 계산 (mm/pixel)
                if (avgDeltaPixelX > 0)
                    pixelResolutionX = actualMoveDistance / avgDeltaPixelX;
                if (avgDeltaPixelY > 0)
                    pixelResolutionY = actualMoveDistance / avgDeltaPixelY;

                // Tilt 계산: X축 이동 시 Y 픽셀 변화량으로 기울기 측정
                // 중앙 행(Point3 → Point5) 기준으로 계산
                double deltaX_pixel = rawPoints[5].X - rawPoints[3].X;
                double deltaY_pixel = rawPoints[5].Y - rawPoints[3].Y;

                // atan2로 실제 이동 방향과 이상적인 방향(0도)의 차이 계산
                if (Math.Abs(deltaX_pixel) > 0.001)
                {
                    tilt = Math.Atan2(deltaY_pixel, deltaX_pixel) * 180.0 / Math.PI;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void FitImage(HTuple window, int HwindowWidth, int nHwindowHeight)
        {
            // 이미지 해상도

            //HOperatorSet.GetImageSize(OriImg, out HTuple imgWidth, out HTuple imgHeight);
            //HTuple imgWidth = new HTuple(2456);
            //HTuple imgHeight = new HTuple(2054); 
            HTuple imgWidth = new HTuple(HwindowWidth);
            HTuple imgHeight = new HTuple(nHwindowHeight);

            // 윈도우 크기
            int winWidth = HwindowWidth;
            int winHeight = nHwindowHeight;

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
            HOperatorSet.SetPart(window, row1, col1, row2, col2);

            // 표시
            HOperatorSet.ClearWindow(window);
            //HOperatorSet.DispObj(OriImg, window);
        }

        public double MeanGrayValue()
        {
            lock (_imageSync)
            {
                return CalculateMeanGrayValue(OriImg);
            }
        }

        private static double CalculateMeanGrayValue(HObject image)
        {
            if (image == null)
                return 0;

            HTuple width = null;
            HTuple height = null;
            HTuple mean = null;
            HTuple deviation = null;
            HObject fullRegion = null;

            try
            {
                HOperatorSet.GetImageSize(image, out width, out height);
                HOperatorSet.GenRectangle1(out fullRegion, 0, 0, height - 1, width - 1);
                HOperatorSet.Intensity(fullRegion, image, out mean, out deviation);
                return mean.D;
            }
            finally
            {
                width?.Dispose();
                height?.Dispose();
                mean?.Dispose();
                deviation?.Dispose();
                fullRegion?.Dispose();
            }
        }

        static public double[,] MMULT(double[,] A, double[,] B)
        {
            int aRows = A.GetLength(0);
            int aCols = A.GetLength(1);
            int bRows = B.GetLength(0);
            int bCols = B.GetLength(1);

            if (aCols != bRows)
                return new double[aRows, bCols];

            double[,] result = new double[aRows, bCols];

            for (int i = 0; i < aRows; i++)
            {
                for (int j = 0; j < bCols; j++)
                {
                    double sum = 0.0;
                    for (int k = 0; k < aCols; k++)
                    {
                        sum += A[i, k] * B[k, j];
                    }
                    result[i, j] = sum;
                }
            }

            return result;
        }

        public PointF RotatePoint(PointF point, PointF center, double angleDeg)
        {
            double angleRad = angleDeg * Math.PI / 180.0;

            // 중심 기준으로 평행이동
            double dx = point.X - center.X;
            double dy = point.Y - center.Y;

            double cos = Math.Cos(angleRad);
            double sin = Math.Sin(angleRad);

            // 회전 변환
            double rx = dx * cos - dy * sin;
            double ry = dx * sin + dy * cos;

            // 다시 원래 좌표계로 이동
            double newX = center.X + rx;
            double newY = center.Y + ry;

            return new PointF((float)newX, (float)newY);
        }

        /// <summary>
        /// 다점 원 피팅 함수
        /// - 입력: 여러 개의 2D 포인트
        /// - 출력: 원의 중심(center), 반지름(radius)
        /// 
        /// Step 1: 선형 최소자승(대수적 피팅)으로 초기값 계산
        /// Step 2: Gauss-Newton 비선형 최소자승으로 정밀 보정
        /// </summary>
        public static void FitCircle(
            List<PointF> points,
            out PointF center,
            out double radius,
            int refineIters = 15)
        {
            if (points == null)
                throw new ArgumentNullException(nameof(points));
            if (points.Count < 3)
                throw new ArgumentException("최소 3개 이상의 점이 필요합니다.", nameof(points));

            // ==============================
            // 0단계: 수치 안정성을 위한 평균 중심 이동
            // ==============================
            double meanX = 0, meanY = 0;
            for (int i = 0; i < points.Count; i++)
            {
                meanX += points[i].X;
                meanY += points[i].Y;
            }
            meanX /= points.Count;
            meanY /= points.Count;

            // ==============================
            // 1단계: 선형 최소자승 원 피팅 (Kåsa 방식)
            //
            // 원 방정식:
            //   x² + y² + a·x + b·y + c = 0
            //
            // 이를 선형식으로 변환:
            //   [x  y  1] [a b c]^T = -(x² + y²)
            // ==============================
            double Sxx = 0, Sxy = 0, Sx1 = 0;
            double Syy = 0, Sy1 = 0;
            double S11 = 0;

            double Tx = 0, Ty = 0, T1 = 0;

            for (int i = 0; i < points.Count; i++)
            {
                // 평균 중심 기준 좌표
                double x = points[i].X - meanX;
                double y = points[i].Y - meanY;
                double z = x * x + y * y;

                // A^T * A 항
                Sxx += x * x;
                Sxy += x * y;
                Sx1 += x;

                Syy += y * y;
                Sy1 += y;
                S11 += 1.0;

                // A^T * b 항
                Tx += x * (-z);
                Ty += y * (-z);
                T1 += (-z);
            }

            // 대칭 3x3 선형 방정식 풀이
            if (!SolveSymmetric3x3(
                    Sxx, Sxy, Sx1,
                    Syy, Sy1,
                    S11,
                    Tx, Ty, T1,
                    out double a, out double b, out double c))
            {
                throw new InvalidOperationException(
                    "원 피팅 실패: 점들이 거의 직선에 가깝습니다.");
            }

            // 중심 및 반지름 계산 (중심 이동 좌표계 기준)
            double cx = -a / 2.0;
            double cy = -b / 2.0;
            double rSq = cx * cx + cy * cy - c;

            if (rSq <= 0)
                throw new InvalidOperationException("초기 원 반지름 계산 실패");

            double r = Math.Sqrt(rSq);

            // 원래 좌표계로 복원
            cx += meanX;
            cy += meanY;

            // ==============================
            // 2단계: Gauss-Newton 비선형 최소자승 보정
            //
            // 목적 함수:
            //   Σ ( |Pi - C| - r )²
            //
            // 실제 점과 원 사이의 기하학적 거리 오차 최소화
            // ==============================
            for (int iter = 0; iter < refineIters; iter++)
            {
                // JᵀJ 행렬 요소
                double J00 = 0, J01 = 0, J02 = 0;
                double J11 = 0, J12 = 0;
                double J22 = 0;

                // Jᵀe 벡터
                double g0 = 0, g1 = 0, g2 = 0;

                const double eps = 1e-12;

                for (int i = 0; i < points.Count; i++)
                {
                    double dx = points[i].X - cx;
                    double dy = points[i].Y - cy;
                    double di = Math.Sqrt(dx * dx + dy * dy);

                    // 중심과 거의 같은 점은 스킵 (수치 불안정 방지)
                    if (di < eps) continue;

                    // 오차 (점-중심 거리 - 반지름)
                    double ei = di - r;

                    // 야코비안 (편미분)
                    double dEdCx = -dx / di;
                    double dEdCy = -dy / di;
                    double dEdR = -1.0;

                    // JᵀJ 누적
                    J00 += dEdCx * dEdCx;
                    J01 += dEdCx * dEdCy;
                    J02 += dEdCx * dEdR;

                    J11 += dEdCy * dEdCy;
                    J12 += dEdCy * dEdR;

                    J22 += dEdR * dEdR;

                    // Jᵀe 누적
                    g0 += dEdCx * ei;
                    g1 += dEdCy * ei;
                    g2 += dEdR * ei;
                }

                // 보정량(delta) 계산
                if (!SolveSymmetric3x3(
                        J00, J01, J02,
                        J11, J12,
                        J22,
                        -g0, -g1, -g2,
                        out double dcx, out double dcy, out double dr))
                {
                    // 더 이상 보정 불가
                    break;
                }

                // 파라미터 업데이트
                cx += dcx;
                cy += dcy;
                r += dr;

                // 수렴 판정
                if (Math.Abs(dcx) + Math.Abs(dcy) + Math.Abs(dr) < 1e-9)
                    break;
            }

            if (r <= 0)
                throw new InvalidOperationException("최종 반지름이 유효하지 않습니다.");

            center = new PointF((float)cx, (float)cy);
            radius = r;
        }

        /// <summary>
        /// 대칭 3x3 선형 방정식 풀이
        /// [a00 a01 a02]
        /// [a01 a11 a12] * x = b
        /// [a02 a12 a22]
        /// </summary>
        private static bool SolveSymmetric3x3(
            double a00, double a01, double a02,
            double a11, double a12,
            double a22,
            double b0, double b1, double b2,
            out double x0, out double x1, out double x2)
        {
            // 가우시안 소거법 + 부분 피벗팅
            double[,] A = new double[3, 4]
            {
            { a00, a01, a02, b0 },
            { a01, a11, a12, b1 },
            { a02, a12, a22, b2 }
            };

            x0 = x1 = x2 = 0;

            for (int col = 0; col < 3; col++)
            {
                int pivot = col;
                double max = Math.Abs(A[col, col]);

                for (int row = col + 1; row < 3; row++)
                {
                    double v = Math.Abs(A[row, col]);
                    if (v > max) { max = v; pivot = row; }
                }

                if (max < 1e-18) return false;

                // 피벗 교환
                if (pivot != col)
                {
                    for (int k = col; k < 4; k++)
                    {
                        double tmp = A[col, k];
                        A[col, k] = A[pivot, k];
                        A[pivot, k] = tmp;
                    }
                }

                // 정규화
                double diag = A[col, col];
                for (int k = col; k < 4; k++) A[col, k] /= diag;

                // 다른 행 제거
                for (int row = 0; row < 3; row++)
                {
                    if (row == col) continue;
                    double factor = A[row, col];
                    for (int k = col; k < 4; k++)
                        A[row, k] -= factor * A[col, k];
                }
            }

            x0 = A[0, 3];
            x1 = A[1, 3];
            x2 = A[2, 3];
            return true;
        }

        public bool TryGetCircleCenter(PointF p1, PointF p2, PointF p3, out PointF center, out double radius)
        {
            center = new PointF(0, 0);
            radius = 0;

            float x1 = p1.X, y1 = p1.Y;
            float x2 = p2.X, y2 = p2.Y;
            float x3 = p3.X, y3 = p3.Y;

            double a = x1 * (y2 - y3)
                     + x2 * (y3 - y1)
                     + x3 * (y1 - y2);

            double D = 2.0 * a;

            if (Math.Abs(D) < 1e-9)
            {
                // 세 점이 거의 일직선 → 원 정의 불가
                center = new PointF(float.NaN, float.NaN);
                radius = double.NaN;
                return false;
            }

            double x1sq_y1sq = x1 * x1 + y1 * y1;
            double x2sq_y2sq = x2 * x2 + y2 * y2;
            double x3sq_y3sq = x3 * x3 + y3 * y3;

            double cx =
                (x1sq_y1sq * (y2 - y3) +
                 x2sq_y2sq * (y3 - y1) +
                 x3sq_y3sq * (y1 - y2)) / D;

            double cy =
                (x1sq_y1sq * (x3 - x2) +
                 x2sq_y2sq * (x1 - x3) +
                 x3sq_y3sq * (x2 - x1)) / D;

            center = new PointF((float)cx, (float)cy);

            radius = Math.Sqrt((cx - x1) * (cx - x1) +
                               (cy - y1) * (cy - y1));

            return true;
        }

        public double CalAngle(PointF p1, PointF p2)
        {
            return Math.Atan2(p2.Y - p1.Y, p2.X - p1.X) * 180.0 / Math.PI;
        }


        public void StageBasicInfo(double[] dPinDeg, double[] dStageDeg, double dRadius)
        {
            m_dPinDeg = dPinDeg;
            m_dStageDeg = dStageDeg;
            m_dRadius = dRadius;
        }

        static public void StageCal(double dX, double dY, double dAngle, out double dX1, out double dX2, out double dY1, out double dY2)
        {

            if (m_dPinDeg.Length != 4 || m_dStageDeg.Length != 4)
            {
                dX1 = dX2 = dY1 = dY2 = 0;
                return;
            }

            //////////////////////////////////////////////

            double rad = dAngle * Math.PI / 180.0;

            double[,] R = { { Math.Cos(rad), -Math.Sin(rad) },
                            { Math.Sin(rad),  Math.Cos(rad) } };

            // RefPin 좌표 (2×4)
            double[,] refPin = new double[2, 4];
            for (int i = 0; i < 4; i++)
            {
                double t = m_dPinDeg[i] * Math.PI / 180.0;
                refPin[0, i] = m_dRadius * Math.Cos(t);
                refPin[1, i] = m_dRadius * Math.Sin(t);
            }

            // Stage 방향 벡터 (2×4)
            double[,] stageDir = new double[2, 4];
            for (int i = 0; i < 4; i++)
            {
                double t = m_dStageDeg[i] * Math.PI / 180.0;
                stageDir[0, i] = Math.Cos(t);
                stageDir[1, i] = Math.Sin(t);
            }

            // RefPin 회전 적용 (2×4)
            double[,] rotated = MMULT(R, refPin);

            // 회전 보정 + XY 이동 적용 (2×4)
            double[,] moved = new double[2, 4];
            for (int i = 0; i < 4; i++)
            {
                moved[0, i] = rotated[0, i] + dX - refPin[0, i];
                moved[1, i] = rotated[1, i] + dY - refPin[1, i];
            }

            // 스테이지 축 방향 투영 → 최종 Required Command
            dX1 = stageDir[0, 0] * moved[0, 0] + stageDir[1, 0] * moved[1, 0];
            dX2 = stageDir[0, 1] * moved[0, 1] + stageDir[1, 1] * moved[1, 1];
            dY1 = stageDir[0, 2] * moved[0, 2] + stageDir[1, 2] * moved[1, 2];
            dY2 = stageDir[0, 3] * moved[0, 3] + stageDir[1, 3] * moved[1, 3];
        }

        /// <summary>
        /// StageCal의 역함수. UVW Stage 이동량(dX1, dX2, dY1)으로부터 X, Y, Angle을 역산함.
        /// </summary>
        /// <param name="dX1">UVW Stage X1 이동량</param>
        /// <param name="dX2">UVW Stage X2 이동량</param>
        /// <param name="dY1">UVW Stage Y1 이동량</param>
        /// <param name="dX">역산된 X 좌표</param>
        /// <param name="dY">역산된 Y 좌표</param>
        /// <param name="dAngle">역산된 각도 (degree)</param>
        /// <returns>성공 여부</returns>
        static public bool StageCalInverse(double dX1, double dX2, double dY1, out double dX, out double dY, out double dAngle)
        {
            dX = dY = dAngle = 0;

            if (m_dPinDeg.Length != 4 || m_dStageDeg.Length != 4)
                return false;

            // RefPin 좌표 (2×4)
            double[,] refPin = new double[2, 4];
            for (int i = 0; i < 4; i++)
            {
                double t = m_dPinDeg[i] * Math.PI / 180.0;
                refPin[0, i] = m_dRadius * Math.Cos(t);
                refPin[1, i] = m_dRadius * Math.Sin(t);
            }

            // Stage 방향 벡터 (2×4)
            double[,] stageDir = new double[2, 4];
            for (int i = 0; i < 4; i++)
            {
                double t = m_dStageDeg[i] * Math.PI / 180.0;
                stageDir[0, i] = Math.Cos(t);
                stageDir[1, i] = Math.Sin(t);
            }

            // 선형 근사 행렬 A 구성 (3x3)
            // dXi = stageDir[0,i]*dX + stageDir[1,i]*dY + θ*(stageDir[1,i]*refPin[0,i] - stageDir[0,i]*refPin[1,i])
            double[,] A = new double[3, 3];

            // Row 0: dX1 (index 0)
            A[0, 0] = stageDir[0, 0];
            A[0, 1] = stageDir[1, 0];
            A[0, 2] = stageDir[1, 0] * refPin[0, 0] - stageDir[0, 0] * refPin[1, 0];

            // Row 1: dX2 (index 1)
            A[1, 0] = stageDir[0, 1];
            A[1, 1] = stageDir[1, 1];
            A[1, 2] = stageDir[1, 1] * refPin[0, 1] - stageDir[0, 1] * refPin[1, 1];

            // Row 2: dY1 (index 2)
            A[2, 0] = stageDir[0, 2];
            A[2, 1] = stageDir[1, 2];
            A[2, 2] = stageDir[1, 2] * refPin[0, 2] - stageDir[0, 2] * refPin[1, 2];

            // 3x3 역행렬 계산
            double det = A[0, 0] * (A[1, 1] * A[2, 2] - A[1, 2] * A[2, 1])
                       - A[0, 1] * (A[1, 0] * A[2, 2] - A[1, 2] * A[2, 0])
                       + A[0, 2] * (A[1, 0] * A[2, 1] - A[1, 1] * A[2, 0]);

            if (Math.Abs(det) < 1e-12)
                return false;

            double[,] invA = new double[3, 3];
            invA[0, 0] = (A[1, 1] * A[2, 2] - A[1, 2] * A[2, 1]) / det;
            invA[0, 1] = (A[0, 2] * A[2, 1] - A[0, 1] * A[2, 2]) / det;
            invA[0, 2] = (A[0, 1] * A[1, 2] - A[0, 2] * A[1, 1]) / det;
            invA[1, 0] = (A[1, 2] * A[2, 0] - A[1, 0] * A[2, 2]) / det;
            invA[1, 1] = (A[0, 0] * A[2, 2] - A[0, 2] * A[2, 0]) / det;
            invA[1, 2] = (A[0, 2] * A[1, 0] - A[0, 0] * A[1, 2]) / det;
            invA[2, 0] = (A[1, 0] * A[2, 1] - A[1, 1] * A[2, 0]) / det;
            invA[2, 1] = (A[0, 1] * A[2, 0] - A[0, 0] * A[2, 1]) / det;
            invA[2, 2] = (A[0, 0] * A[1, 1] - A[0, 1] * A[1, 0]) / det;

            // [dX, dY, θ(rad)] = invA * [dX1, dX2, dY1]
            dX     = invA[0, 0] * dX1 + invA[0, 1] * dX2 + invA[0, 2] * dY1;
            dY     = invA[1, 0] * dX1 + invA[1, 1] * dX2 + invA[1, 2] * dY1;
            double dTheta = invA[2, 0] * dX1 + invA[2, 1] * dX2 + invA[2, 2] * dY1;

            // 라디안 → 도(degree) 변환
            dAngle = dTheta * 180.0 / Math.PI;

            return true;
        }

        public double SetImage(HTuple hWindow, byte[] img, int nWidth, int nHeight)
        {
            long requiredLength = (long)nWidth * nHeight;
            if (img == null || nWidth <= 0 || nHeight <= 0 || img.LongLength < requiredLength)
                throw new ArgumentException("버퍼 크기가 이미지 크기보다 작습니다.");

            HObject newOriginalImage = null;
            HObject newProcessImage = null;
            GCHandle handle = default(GCHandle);
            try
            {
                handle = GCHandle.Alloc(img, GCHandleType.Pinned);
                IntPtr ptr = handle.AddrOfPinnedObject();
                HOperatorSet.GenImage1(out newOriginalImage, "byte", nWidth, nHeight, ptr);
                HOperatorSet.CopyImage(newOriginalImage, out newProcessImage);

                lock (_imageSync)
                {
                    // Keep the previous frame intact until the new frame is fully valid.
                    HOperatorSet.DispObj(newOriginalImage, hWindow);

                    HObject oldOriginalImage = OriImg;
                    HObject oldProcessImage = ProcessImg;

                    OriImg = newOriginalImage;
                    ProcessImg = newProcessImage;
                    newOriginalImage = null;
                    newProcessImage = null;

                    oldOriginalImage?.Dispose();
                    oldProcessImage?.Dispose();

                    return CalculateMeanGrayValue(OriImg);
                }
            }
            finally
            {
                if (handle.IsAllocated)
                    handle.Free();

                newOriginalImage?.Dispose();
                newProcessImage?.Dispose();
            }
        }

        public void SetImage(HTuple hWindow, string strPath)
        {
            try
            {
                HOperatorSet.ClearWindow(hWindow);
                if (System.IO.File.Exists(strPath + ".bmp"))
                {
                    HOperatorSet.ReadImage(out OriImg, strPath);
                    

                    HOperatorSet.CopyImage(OriImg, out ProcessImg);

                    HTuple width, height;
                    HOperatorSet.GetImageSize(OriImg, out width, out height);

                    FitImage(hWindow, width, height);

                    HOperatorSet.DispObj(OriImg, hWindow);
                }
            }
            catch { }
        }

        #region Direction

        public void SetTriangleDirectionMark(int baseLen, int heightLen)
        {
            // 1. 검은 배경 이미지 생성 (byte, 0으로 초기화)
            HOperatorSet.GenImageConst(out HObject img, "byte", baseLen + 30, heightLen + 30);

            // 2. 삼각형 꼭짓점 좌표 계산
            double rowBottom = heightLen - 1 + 15;
            double colLeft = 0 + 15;
            double colRight = baseLen - 1 + 15;
            double rowTop = 0 + 15;
            double colTop = baseLen / 2.0 + 15;

            // 3. 꼭짓점 배열 (Row/Col)
            HTuple rows = new HTuple();
            rows = rows.TupleConcat(rowBottom); // 밑변 왼쪽
            rows = rows.TupleConcat(rowBottom); // 밑변 오른쪽
            rows = rows.TupleConcat(rowTop);    // 꼭짓점 위쪽 중앙

            HTuple cols = new HTuple();
            cols = cols.TupleConcat(colLeft);
            cols = cols.TupleConcat(colRight);
            cols = cols.TupleConcat(colTop);

            // 4. 삼각형 Region 생성 (이등변 삼각형 영역)
            HOperatorSet.GenRegionPolygonFilled(out HObject triRegion, rows, cols);

            // 5. 삼각형 영역을 흰색(255)으로 채운 이미지 생성
            HOperatorSet.PaintRegion(triRegion, img, out HObject imgTri, 255, "fill");


            HTuple AngleStart = new HTuple(0).TupleRad();
            HTuple AngleExtent = new HTuple(360).TupleRad();
            HTuple NumLevel = new HTuple(0);
            HOperatorSet.ClearShapeModel(m_SquareModel);

            HOperatorSet.GenRectangle1(out HObject Rect, 0, 0, baseLen + 30, heightLen + 30);
            HOperatorSet.ReduceDomain(imgTri, Rect, out imgTri);

            HOperatorSet.CreateShapeModel(imgTri,
                NumLevel, //NumLevel
                AngleStart, AngleExtent, "auto", // angle
                "auto",
                "use_polarity",
                "auto",
                "auto",
                out m_triangleModel);


            // 결과 이미지 리턴 (검정 배경 + 흰 이등변 삼각형)
            //HOperatorSet.WriteImage(imgTri, "bmp", 0, "D:\\1.Project\\7.IPS-Align\\Temp\\Test");

            triRegion.Dispose();
            img.Dispose();
            imgTri.Dispose();
            NumLevel?.Dispose();

            return;
        }

        public void SetSquarDirectionMark(int nWidth, int nHeight)
        {
            HTuple Width = new HTuple(nWidth);
            HTuple Height = new HTuple(nHeight);
            HTuple NumLevel = new HTuple(0);

            HTuple ForeGround, BackGround;

            BackGround = new HTuple(0); ForeGround = new HTuple(255);

            HOperatorSet.GenImageConst(out HObject image, "byte", Width + 30, Height + 30);
            HOperatorSet.PaintRegion(image, image, out HObject whiteBg, BackGround, "fill");

            HOperatorSet.GenRectangle1(out HObject Rect1, 0 + 15, 0 + 15, Height + 15, Width + 15);
            HOperatorSet.PaintRegion(Rect1, whiteBg, out HObject domainImg, ForeGround, "fill");

            //HOperatorSet.WriteImage(domainImg, "bmp", 0, "D:\\1.Project\\7.IPS-Align\\Temp\\Test");

            HTuple AngleStart = new HTuple(0).TupleRad();
            HTuple AngleExtent = new HTuple(360).TupleRad();
            HOperatorSet.ClearShapeModel(m_SquareModel);

            HOperatorSet.GenRectangle1(out HObject Rect, 0, 0, Height + 30, Width + 30);
            HOperatorSet.ReduceDomain(domainImg, Rect, out domainImg);

            HOperatorSet.CreateShapeModel(domainImg,
                NumLevel, //NumLevel
                AngleStart, AngleExtent, "auto", // angle
                "auto",
                "use_polarity",
                "auto",
                "auto",
                out m_SquareModel);

            NumLevel?.Dispose();
            Width?.Dispose();
            Height?.Dispose();
            ForeGround.Dispose();
            BackGround.Dispose();
            image.Dispose();
            whiteBg.Dispose();
            Rect1.Dispose();
        }

        public bool Find_DirectionMark(out ModelRslt rslt)
        {
            rslt = new ModelRslt(); 
            try
            {
                HTuple AngleStart = new HTuple(0).TupleRad();
                HTuple AngleExtent = new HTuple(360).TupleRad();
                HTuple SubPixel = new HTuple("least_squares");
                SubPixel.Append(new HTuple("max_deformation 4"));

                // 모델 찾기 (전체 이미지에서 탐색)
                HOperatorSet.FindScaledShapeModel(ProcessImg, m_SquareModel,
                    AngleStart, AngleExtent,        // 회전 허용
                    1.0, 1.0, 
                    0.85,                // MinScore
                    1,                  // 최대 탐색 개수
                    0,                // 중첩 허용도  
                    SubPixel,
                    0,
                    0.9,    //d : 0.9   0 ~ 1
                    out HTuple row, out HTuple col, out HTuple angle, out HTuple scale, out HTuple score
                );
                if (score.Length > 0)
                {

                    rslt.dY = row.D;
                    rslt.dX = col.D;
                    rslt.dAngle = angle.TupleDeg();
                    rslt.dScore = score.D;
                    rslt.dScale = scale.D;

                    row?.Dispose();
                    col?.Dispose();
                    angle?.Dispose();
                    score?.Dispose();
                    scale?.Dispose();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch { return false; }
        }
        #endregion



        #region Ncc
        bool m_bCreatRoi = false;
        HObject m_Region = new HObject();

        public void DrawMove(HTuple windowHandle, double dInnerRadius)
        {
            // 외곽 원의 현재 파라미터 읽기
            HOperatorSet.GetDrawingObjectParams(m_hDrawID, "row", out HTuple row);
            HOperatorSet.GetDrawingObjectParams(m_hDrawID, "column", out HTuple col);

            DrawSemiTransparentCircle_Gray(windowHandle, ProcessImg, row, col, dInnerRadius);

            row?.Dispose();
            col?.Dispose();
        }

        private void DrawSemiTransparentCircle_Gray(HTuple windowHandle, HObject grayImage, double row, double col, double radius)
        {
            double alpha = 0.4;  // 투명도
            int R = 255, G = 0, B = 0;

            // ----- 1) RGB 오버레이 이미지 생성 -----
            // gray 이미지 크기 얻기

            HOperatorSet.GetImageSize(grayImage, out HTuple width, out HTuple height);

            HOperatorSet.CopyImage(grayImage, out HObject ProcessImg);

            // 3채널 오버레이 생성 (기본 0으로)
            HOperatorSet.GenImageConst(out HObject R_img, "byte", width, height);
            HOperatorSet.GenImageConst(out HObject G_img, "byte", width, height);
            HOperatorSet.GenImageConst(out HObject B_img, "byte", width, height);

            // ----- 2) 원 영역 만들기 -----
            HOperatorSet.GenCircle(out HObject region, row, col, radius);

            // ----- 3) 원 내부에만 빨간색(255,0,0) 채우기 -----
            HOperatorSet.PaintRegion(region, R_img, out R_img, R, "fill");
            HOperatorSet.PaintRegion(region, G_img, out G_img, G, "fill");
            HOperatorSet.PaintRegion(region, B_img, out B_img, B, "fill");

            // ----- 4) 빨간색 반투명 처리 (alpha blending) -----
            // blended = alpha*redOverlay + (1-alpha)*originalGray

            // gray → alpha_blended_gray
            HOperatorSet.ScaleImage(ProcessImg, out HObject grayScaled, 1.0, 0);

            // redOverlay → alpha_blended_overlay (R만 색 있음)
            HOperatorSet.ScaleImage(R_img, out HObject R_scaled, alpha, 0);
            HOperatorSet.ScaleImage(G_img, out HObject G_scaled, alpha, 0);
            HOperatorSet.ScaleImage(B_img, out HObject B_scaled, alpha, 0);

            // ----- 5) 회색 이미지를 R,G,B에 동일하게 섞기 -----
            // Final R = gray*0.6 + red*0.4
            HOperatorSet.AddImage(grayScaled, R_scaled, out HObject R_final, 1.0, 0.0);

            // Final G = gray*0.6 + 0
            HOperatorSet.AddImage(grayScaled, G_scaled, out HObject G_final, 1.0, 0.0);

            // Final B = gray*0.6 + 0
            HOperatorSet.AddImage(grayScaled, B_scaled, out HObject B_final, 1.0, 0.0);

            // ----- 6) RGB로 합성해서 하나의 반투명 원 오버레이 만들기 -----
            HOperatorSet.Compose3(R_final, G_final, B_final, out HObject overlay);

            // ----- 7) 화면 출력 (원본 → 오버레이 순) -----
            HOperatorSet.DispObj(grayImage, windowHandle);
            HOperatorSet.DispObj(overlay, windowHandle);


            width?.Dispose();
            height?.Dispose();
            region?.Dispose();
            grayScaled?.Dispose();
            R_img?.Dispose();
            G_img?.Dispose();
            B_img?.Dispose();
            R_scaled?.Dispose();
            G_scaled?.Dispose();
            B_scaled?.Dispose();
            R_final?.Dispose();
            G_final?.Dispose();
            B_final?.Dispose();
            overlay?.Dispose();
        }

        public void CreateRoi(HTuple window, HModelInfo modelInfo, double dRow = 0, double dCol = 0)
        {
            HTuple row, col, radius;

            HOperatorSet.ClearWindow(window);
            HOperatorSet.DispObj(OriImg, window);

            if(modelInfo.ModelID == null || modelInfo.ModelID.Length != 0)
            {
                HOperatorSet.GetNccModelRegion(out modelInfo.Region, modelInfo.ModelID);
            }
            else
                modelInfo.Region = null;

            if (modelInfo.Region != null) {
                HOperatorSet.SmallestCircle(modelInfo.Region, out row, out col, out radius);
                row = new HTuple(dRow);
                col = new HTuple(dCol);
            }
            else
            {
                row = 100; col = 100; radius = 50;
            }

            HOperatorSet.CreateDrawingObjectCircle(row, col, radius, out m_hDrawID);
            HOperatorSet.AttachDrawingObjectToWindow(window, m_hDrawID);

            row?.Dispose();
            col?.Dispose();
            radius?.Dispose();

            m_bCreatRoi= true;
        }

        public void SetRoi(HTuple window, double dInnerRadius, ref HModelInfo data)
        {
            if(!m_bCreatRoi) { data.Region = m_Region; data.dRow = data.dCol = 0; return; }

            HTuple row, col, radius;
            HOperatorSet.GenEmptyObj(out data.Region);
            HOperatorSet.GetDrawingObjectIconic(out data.Region, m_hDrawID);

            HOperatorSet.SmallestCircle(data.Region, out row, out col, out radius);
            HOperatorSet.GenCircle(out HObject innerMask, row, col, dInnerRadius);

            HOperatorSet.Difference(data.Region, innerMask, out data.Region);


            HOperatorSet.DetachDrawingObjectFromWindow(window, m_hDrawID);
            HOperatorSet.ClearDrawingObject(m_hDrawID);

            HOperatorSet.SetColor(window, "green");

            HOperatorSet.DispObj(data.Region, window);

            data.dRow = row.D;
            data.dCol = col.D;

            row?.Dispose();
            col?.Dispose();
            radius?.Dispose();
        }

        public void CreateNccModel(HTuple window, ModelParam modelParam, ref HModelInfo data, bool bTemp = false)
        {
            try
            {
                HOperatorSet.ClearNccModel(data.ModelID);
                data.ModelID?.Dispose();

                HTuple NumLevel    = new HTuple(modelParam.nNumLevels);
                HTuple AngleStart  = new HTuple(modelParam.dAngleMin).TupleRad();
                HTuple AngleExtent = new HTuple(modelParam.dAngleMax).TupleRad();
                HTuple Metric      = new HTuple(modelParam.strMetric);
                HTuple AngleStep = "auto";   //

                if(bTemp)
                    HOperatorSet.GenCircle(out data.Region, modelParam.dRadius + 15, modelParam.dRadius + 15, modelParam.dRadius + 15);

                HOperatorSet.ReduceDomain(ProcessImg, data.Region, out HObject NCC_ModelImg);

                HOperatorSet.CreateNccModel(
                    NCC_ModelImg,
                    NumLevel,   // NumLevels
                    AngleStart,
                    AngleExtent,        // AngleExtent
                    AngleStep,        // AngleStep
                    Metric, // Metric
                    out data.ModelID
                );

                HOperatorSet.ClearWindow(window);

                HOperatorSet.SetColor(window, "red");
                HOperatorSet.DispObj(ProcessImg, window);
                HOperatorSet.SetTposition(window, 10, 10);
                HOperatorSet.WriteString(window, "모델 생성이 완료되었습니다!!");
                HOperatorSet.DispObj(data.Region, window);

                NCC_ModelImg?.Dispose();
            }
            catch { }
        }

        

        public void OverlayNccModel(bool bRslt, HTuple window, double dX, double dY, double dAngle, double dScore, string strColor = "")
        {
            // cyan red green blue
            try
            {
                HOperatorSet.ClearWindow(window);
                HOperatorSet.DispObj(OriImg, window);

                if(bRslt) 
                { 
                    HTuple row = new HTuple(dY);
                    HTuple col = new HTuple(dX);
                    HTuple angle = new HTuple(dAngle);
                    HTuple color;
                    if (strColor == "") color = new HTuple("green");
                    else                color = new HTuple(strColor);

                    HOperatorSet.DispObj(OriImg, window);
                    HOperatorSet.GenCrossContourXld(out HObject cross, row, col, 50, angle);

                    HOperatorSet.SetColor(window, color);
                    HOperatorSet.SetLineWidth(window, 2);

                    HOperatorSet.DispObj(cross, window);

                    HOperatorSet.SetColor(window, "green");
                    HOperatorSet.SetTposition(window, 10, 10);
                    HOperatorSet.WriteString(window, string.Format("Find : {0:F02}", dScore * 100));
                    HOperatorSet.SetTposition(window, 100, 10);
                    HOperatorSet.WriteString(window, string.Format("X : {0:F02} / Y : {1:F02}, Angle : {2:F02}", dX, dY, dAngle));

                    row.Dispose();
                    col.Dispose();
                    angle.Dispose();
                    color.Dispose();
                } else
                {
                    HOperatorSet.SetColor(window, "red");
                    HOperatorSet.WriteString(window, string.Format("Not Find"));
                }
            }
            catch { }
        }

        #endregion

        #region Mask Templete
        /// <summary>
        /// Use modelParam  : dRadius, strBackGround
        /// </summary>
        public void CreateCircleImg(HTuple window, ModelParam modelParam)
        {
            HTuple Width  = new HTuple(modelParam.dRadius * 2) + 30;
            HTuple Height = new HTuple(modelParam.dRadius * 2) + 30;
            HTuple Rdaius = new HTuple(modelParam.dRadius);

            HTuple ForeGround, BackGround;
            if (modelParam.strBackGround.Contains("black")) { BackGround = new HTuple(0  ); ForeGround = new HTuple(255); }
            else                                            { BackGround = new HTuple(255); ForeGround = new HTuple(0  ); }

            HOperatorSet.GenImageConst(out HObject image, "byte", Width, Height);
            HOperatorSet.PaintRegion(image, image, out HObject whiteBg, BackGround, "fill");

            HOperatorSet.GenCircle(out HObject circle, Rdaius + 15, Rdaius + 15, Rdaius);
            HOperatorSet.PaintRegion(circle, whiteBg, out OriImg, ForeGround, "fill");

            HOperatorSet.CopyImage(OriImg, out ProcessImg);
            HOperatorSet.DispObj(OriImg, window);

            ForeGround?.Dispose();
            BackGround?.Dispose();
            image  ?.Dispose();
            whiteBg?.Dispose();
            circle ?.Dispose();
        }

        /// <summary>
        /// Use modelParam  : SizeX, SizeY, Depth, strBackGround
        /// </summary>
        public void CreateCrossImg(HTuple window, ModelParam modelParam)
        {
            HTuple Width = new HTuple(modelParam.dSizeX) ;
            HTuple Height = new HTuple(modelParam.dSizeY) ;
            HTuple Depth = new HTuple(modelParam.dDepth);

            HTuple ForeGround, BackGround;
            if (modelParam.strBackGround.Contains("Black")) { BackGround = new HTuple(0); ForeGround = new HTuple(255); }
            else { BackGround = new HTuple(255); ForeGround = new HTuple(0); }

            HOperatorSet.GenImageConst(out HObject image, "byte", Width, Height);
            HOperatorSet.PaintRegion(image, image, out HObject whiteBg, BackGround, "fill");

            HOperatorSet.GenRectangle1(out HObject Rect1, 0, (Width / 2) - (Depth / 2), Height-1, (Width / 2) + (Depth / 2));
            HOperatorSet.GenRectangle1(out HObject Rect2, (Height / 2) - (Depth / 2), 0, (Height / 2) + (Depth / 2), Width-1);

            HOperatorSet.Union2(Rect1, Rect2, out HObject cross);

            HOperatorSet.PaintRegion(cross, whiteBg, out OriImg, ForeGround, "fill");

            HOperatorSet.CopyImage(OriImg, out ProcessImg);
            HOperatorSet.DispObj(OriImg, window);

            ForeGround?.Dispose();
            BackGround?.Dispose();
            image  ?.Dispose();
            whiteBg?.Dispose();
            Rect1  ?.Dispose();
            Rect2  ?.Dispose();
            cross  ?.Dispose();
        }

        /// <summary>
        /// Use modelParam  : nNumLevels, dAngleMin, dAngleMax, strMetric, dRadius
        /// </summary>
        public void CreateShmMaskModel(HTuple window, ModelParam modelParam, ref HModelInfo data)
        {
            try
            {
                HTuple NumLevel    = new HTuple(modelParam.nNumLevels );
                HTuple AngleStart  = new HTuple(modelParam.dAngleMin  );
                HTuple AngleExtent = new HTuple(modelParam.dAngleMax  );
                HTuple Metric      = new HTuple(modelParam.strMetric  );

                HTuple NumMatch    = new HTuple(1);

                HOperatorSet.ClearShapeModel(data.ModelID);

                HOperatorSet.GenCircle(out data.Region, modelParam.dRadius + 15, modelParam.dRadius + 15, modelParam.dRadius + 15);

                HOperatorSet.ReduceDomain(ProcessImg, data.Region, out HObject domainImg);

                HOperatorSet.CreateShapeModel(
                    domainImg,
                    NumLevel,
                    AngleStart,
                    AngleExtent, 
                    "auto",
                    "auto",
                    Metric, 
                    "auto",
                    "auto", 
                    out data.ModelID);

                HOperatorSet.ClearWindow(window);

                HOperatorSet.SetColor(window, "red");
                HOperatorSet.DispObj(ProcessImg, window);
                HOperatorSet.SetTposition(window, 10, 10);
                HOperatorSet.WriteString(window, "모델 생성이 완료되었습니다!!");
                HOperatorSet.DispObj(data.Region, window);

                domainImg  ?.Dispose();
                NumLevel   ?.Dispose();
                AngleStart ?.Dispose();
                AngleExtent?.Dispose();
                Metric     ?.Dispose();
            }
            catch { }
        }

        /// <summary>
        /// Use modelParam  : nNumLevels, dAngleMin, dAngleMax, strMetric, dRadius, dcontrast, dcontrast min 
        /// </summary>
        public void CreateShmGlassModel(HTuple window, ModelParam modelParam, ref HModelInfo data)
        {
            try
            {
                HTuple NumLevel = new HTuple(modelParam.nNumLevels);
                HTuple AngleStart = new HTuple(modelParam.dAngleMin);
                HTuple AngleExtent = new HTuple(modelParam.dAngleMax);
                HTuple Metric = new HTuple(modelParam.strMetric);
                HTuple contrast, contrastMin;

                if (modelParam.bContrast) contrast = new HTuple("auto");
                else                      contrast = new HTuple(modelParam.dContrast); 

                if (modelParam.bContrastMin) contrastMin = new HTuple("auto");
                else                         contrastMin = new HTuple(modelParam.dContrastMin); 

                HTuple NumMatch = new HTuple(1);

                HOperatorSet.ClearShapeModel(data.ModelID);

                //HOperatorSet.GenCircle(out data.Region, modelParam.dRadius + 15, modelParam.dRadius + 15, modelParam.dRadius + 15);
                HOperatorSet.GenRectangle1(out data.Region, 0, 0, modelParam.dSizeX, modelParam.dSizeY);

                HOperatorSet.ReduceDomain(ProcessImg, data.Region, out HObject ShmImg);

                HOperatorSet.CreateScaledShapeModel( ShmImg,
                    NumLevel,
                    AngleStart, AngleExtent, "auto",
                    0.5, 1.5, 0.1,
                    "auto",
                    Metric,
                    contrast,
                    contrastMin,
                    out data.ModelID);

                HOperatorSet.ClearWindow(window);

                HOperatorSet.SetColor(window, "red");
                HOperatorSet.DispObj(ProcessImg, window);
                HOperatorSet.SetTposition(window, 10, 10);
                HOperatorSet.WriteString(window, "모델 생성이 완료되었습니다!!");
                HOperatorSet.DispObj(data.Region, window);

                ShmImg?.Dispose();

            }
            catch { }
        }

        #endregion

        /// <summary>
        /// Use modelParam  : nNumLevels, dAngleMin, dAngleMax, dMinScore, dMaxOverlap, strSubPixel
        /// </summary>
        public bool NCC_FindModel(ModelParam modelParam, HModelInfo data, out ModelRslt rslt)
        {
            rslt = new ModelRslt();
            try
            {
                HTuple NumLevel = new HTuple(modelParam.nNumLevels);
                HTuple AngleStart = new HTuple(modelParam.dAngleMin).TupleRad();
                HTuple AngleExtent = new HTuple(modelParam.dAngleMax).TupleRad();
                //HTuple Metric      = new HTuple(modelParam.strMetric  );
                HTuple MinScroe = new HTuple(modelParam.dMinScore / 100);
                HTuple MaxOverlap = new HTuple(modelParam.dMaxOverlap);
                HTuple SubPixel = new HTuple(modelParam.strSubPixel);

                HTuple NumMatch = new HTuple(1);

                HOperatorSet.FindNccModel(ProcessImg, data.ModelID,
                AngleStart, AngleExtent,            // 회전 허용 범위
                MinScroe,             // MinScore (매칭 임계값)
                NumMatch,               // NumMatches (최대 매칭 개수)
                MaxOverlap,             // MaxOverlap
                SubPixel,          // SubPixel 정밀도
                NumLevel,               // NumLevels (0 = 자동)
                out HTuple row,
                out HTuple col,
                out HTuple angle,
                out HTuple score
                );

                if (score.Length > 0)
                {
                    rslt.dY = row.D;
                    rslt.dX = col.D;
                    rslt.dAngle = angle.TupleDeg();
                    rslt.dScore = score.D;
                    rslt.dScale = 1.0;

                    row?.Dispose();
                    col?.Dispose();
                    angle?.Dispose();
                    score?.Dispose();

                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Use modelParam  : nNumLevels, dAngleMin, dAngleMax, MinScroe, MaxOverlap, SubPixel, GreedNess
        /// </summary>
        public bool FindShmModel(ModelParam modelParam, HModelInfo data, out ModelRslt rslt)
        {
            rslt = new ModelRslt();
            try
            {
                //HTuple hTuple1 = new HTuple("none");
                HTuple NumLevel = new HTuple(modelParam.nNumLevels);
                HTuple AngleStart = new HTuple(modelParam.dAngleMin).TupleRad();
                HTuple AngleExtent = new HTuple(modelParam.dAngleMax).TupleRad();
                HTuple MinScroe = new HTuple(modelParam.dMinScore / 100);
                HTuple MaxOverlap = new HTuple(modelParam.dMaxOverlap);
                HTuple SubPixel = new HTuple(modelParam.strSubPixel);
                HTuple GreedNess = new HTuple(modelParam.dGreedNess);
                SubPixel.Append(new HTuple("max_deformation 3"));

                // 모델 찾기 (전체 이미지에서 탐색)
                HOperatorSet.FindScaledShapeModel(ProcessImg, data.ModelID,
                    AngleStart, AngleExtent,        // 회전 허용
                    0.8, 1.2,
                    MinScroe,                // MinScore
                    1,                  // 최대 탐색 개수
                    MaxOverlap,                // 중첩 허용도  
                    SubPixel,
                    NumLevel,
                    GreedNess,    //d : 0.9   0 ~ 1
                    out HTuple row, out HTuple col, out HTuple angle, out HTuple scale, out HTuple score
                );
                if (score.Length > 0)
                {

                    rslt.dY = row.D;
                    rslt.dX = col.D;
                    rslt.dAngle = angle.TupleDeg();
                    rslt.dScore = score.D;
                    rslt.dScale = scale.D;

                    row?.Dispose();
                    col?.Dispose();
                    angle?.Dispose();
                    score?.Dispose();
                    scale?.Dispose();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch {
            return false;
            }
        }

        public void ClearOverlay(HTuple window)
        {
            try
            {
                HOperatorSet.ClearWindow(window);
                HOperatorSet.DispObj(OriImg, window);
            } catch { }
        }

        public static void OverlayCross(HTuple window, double dX, double dY, double dAngle, string strColor, int nLienWidth = 2, int nCrossSize = 50)
        {
            try
            {
                HTuple row = new HTuple(dY);
                HTuple col = new HTuple(dX);
                HTuple lineWidth = new HTuple(nLienWidth);
                HTuple angle = new HTuple(dAngle).TupleRad();
                HTuple color = new HTuple(strColor);
                HTuple crossSize = new HTuple(nCrossSize);

                HOperatorSet.GenCrossContourXld(out HObject cross, row, col, crossSize, angle);

                HOperatorSet.SetColor(window, color);
                HOperatorSet.SetLineWidth(window, lineWidth);

                HOperatorSet.DispObj(cross, window);

                row?.Dispose();
                col?.Dispose();
                angle?.Dispose();
                lineWidth?.Dispose();
                color?.Dispose();
                crossSize?.Dispose();
            } catch { }
        }

        public void Overlay(HTuple window, double[] dX, double[] dY, double[] dAngle, string[] strColor)
        {
            // cyan red green blue

            try
            {
                if (dX.Length <= 0) return;
                HOperatorSet.ClearWindow(window);
                HOperatorSet.DispObj(OriImg, window);

                for (int n = 0; n < dX.Length; n++)
                {
                    HTuple row = new HTuple(dY[n]);
                    HTuple col = new HTuple(dX[n]);
                    HTuple angle = new HTuple(dAngle[n]);
                    HTuple color = new HTuple(strColor[n]);

                    HOperatorSet.DispObj(OriImg, window);
                    HOperatorSet.GenCrossContourXld(out HObject cross, row, col, 50, angle);

                    HOperatorSet.SetColor(window, color);
                    HOperatorSet.SetLineWidth(window, 2);

                    HOperatorSet.DispObj(cross, window);

                    row.Dispose();
                    col.Dispose();
                    angle.Dispose();
                }
                

            }
            catch { }
        }

        public void SaveModel(Matching_Type type, HModelInfo data, string strPath)
        {
            try
            {
                switch (type)
                {
                    case Matching_Type.MARK_GRAY:
                        HOperatorSet.WriteNccModel(data.ModelID, strPath + ".ncm");
                        break;
                    case Matching_Type.MASK_SHAPE:
                        HOperatorSet.WriteShapeModel(data.ModelID, strPath + ".shm");
                        break;
                    
                    case Matching_Type.GLASS_SHAPE:
                        HOperatorSet.WriteShapeModel(data.ModelID, strPath + ".shm");
                        break;
                }

                HOperatorSet.WriteImage(OriImg, "bmp", 0, strPath);
            }
            catch { }
        }

        public void LoadModel(Matching_Type type, string strPath, out HModelInfo data)
        {
            data = new HModelInfo();
            try
            {
                switch (type)
                {
                    case Matching_Type.MARK_GRAY:
                        HOperatorSet.ClearNccModel(data.ModelID);
                        GC.Collect();
                        HOperatorSet.ReadNccModel(strPath + ".ncm", out data.ModelID);
                        break;
                    case Matching_Type.MASK_SHAPE:
                        HOperatorSet.ClearShapeModel(data.ModelID);
                        GC.Collect();
                        HOperatorSet.ReadShapeModel(strPath + ".shm", out data.ModelID);
                        break;
                    case Matching_Type.GLASS_SHAPE:
                        HOperatorSet.ClearShapeModel(data.ModelID);
                        GC.Collect();
                        HOperatorSet.ReadShapeModel(strPath + ".shm", out data.ModelID);
                        break;
                }
            }
            catch { }
        }

        public bool TryCopyOriImage(out HObject copy)
        {
            copy = null;
            lock (_imageSync)
            {
                try
                {
                    if (OriImg == null) return false;
                    HOperatorSet.CopyImage(OriImg, out copy);
                    return true;
                }
                catch
                {
                    copy?.Dispose();
                    copy = null;
                    return false;
                }
            }
        }

        public double MeasureSharpness()
        {
            lock (_imageSync)
            {
                if (OriImg == null)
                    return 0;

                HObject hLaplace = null;
                HObject fullRegion = null;
                HTuple width = null;
                HTuple height = null;
                HTuple mean = null;
                HTuple deviation = null;

                try
                {
                    HOperatorSet.GetImageSize(OriImg, out width, out height);
                    HOperatorSet.Laplace(OriImg, out hLaplace, "absolute", 3, "n_4");
                    HOperatorSet.GenRectangle1(out fullRegion, 0, 0, height - 1, width - 1);
                    HOperatorSet.Intensity(fullRegion, hLaplace, out mean, out deviation);
                    return deviation.D * deviation.D;
                }
                finally
                {
                    hLaplace?.Dispose();
                    fullRegion?.Dispose();
                    width?.Dispose();
                    height?.Dispose();
                    mean?.Dispose();
                    deviation?.Dispose();
                }
            }
        }

        public bool TryCopyProcessingImage(out HObject copy)
        {
            copy = null;
            lock (_imageSync)
            {
                try
                {
                    if (ProcessImg == null) return false;
                    HOperatorSet.CopyImage(ProcessImg, out copy);
                    return true;
                }
                catch
                {
                    copy?.Dispose();
                    copy = null;
                    return false;
                }
            }
        }
    }
}
