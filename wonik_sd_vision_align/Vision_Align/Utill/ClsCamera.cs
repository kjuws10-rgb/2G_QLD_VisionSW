using HalconDotNet;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using uEye;
using uEye.Defines;
using uEye.Types;
using static System.Net.Mime.MediaTypeNames;
using static uEye.Extensions;

namespace Vision_Align
{
    public class ClsCamera
    {
        private Camera m_Cam;
        private static uEye.Types.CameraInformation[] m_camList;
        private readonly object _cameraSync = new object();
        
        int m_nCamNo = 0;
        string m_strSerial = "";

        public int Width { get => m_nWidth; }
        public int Height { get => m_nHeight; }

        int m_nWidth = 0;
        int m_nHeight = 0;

        private void CameraAdded(object sender, EventArgs e)
        {
            try
            {
                InitCam(m_strSerial);
            }
            catch (Exception ex)
            {
                CrashDiagnostics.ReportRecoverableException("Camera " + (m_nCamNo + 1) + " device-added event", ex);
            }
        }

        private void CameraRemoved(object sender, EventArgs e)
        {
            try
            {
                lock (_cameraSync)
                {
                    if (m_Cam == null)
                        return;

                    Camera removedCamera = m_Cam;
                    m_Cam = null;
                    Global.bConnectCam = false;

                    removedCamera.EventDeviceRemove -= CameraRemoved;
                    removedCamera.Exit();
                    CrashDiagnostics.RecordActivity("Camera removed: CAM_" + (m_nCamNo + 1));
                }
            }
            catch (Exception ex)
            {
                CrashDiagnostics.ReportRecoverableException("Camera " + (m_nCamNo + 1) + " device-removed event", ex);
            }
        }
        
        public ClsCamera(CamInfo nCamNo, string strSerial)
        {
            m_nCamNo = (int)nCamNo;
            m_strSerial = strSerial;
            InitCam(m_strSerial);
        } 

        public void InitCam(string strSerial)
        {
            lock (_cameraSync)
            {
                Camera candidate = null;
                try
                {
                    if (m_Cam != null && m_Cam.IsOpened)
                        return;

                    if (m_Cam != null)
                    {
                        try
                        {
                            m_Cam.EventDeviceRemove -= CameraRemoved;
                            m_Cam.Exit();
                        }
                        catch (Exception staleCameraException)
                        {
                            CrashDiagnostics.ReportRecoverableException(
                                "Camera " + (m_nCamNo + 1) + " stale handle cleanup",
                                staleCameraException);
                        }
                        finally
                        {
                            m_Cam = null;
                        }
                    }

                    // Keep exactly one global device-added subscription per wrapper instance.
                    uEye.Info.Camera.EventNewDevice -= CameraAdded;
                    uEye.Info.Camera.EventNewDevice += CameraAdded;

                    uEye.Types.CameraInformation[] cameraList;
                    EnsureSuccess(uEye.Info.Camera.GetCameraList(out cameraList), "get camera list");
                    uEye.Types.CameraInformation? cameraInfo = null;
                    foreach (uEye.Types.CameraInformation info in cameraList)
                    {
                        if (strSerial == info.SerialNumber)
                        {
                            cameraInfo = info;
                            break;
                        }
                    }

                    if (!cameraInfo.HasValue)
                    {
                        CrashDiagnostics.RecordActivity("Camera not found: serial " + strSerial);
                        return;
                    }

                    candidate = new Camera();
                    EnsureSuccess(candidate.Init(cameraInfo.Value.CameraID), "initialize serial " + strSerial);

                    string cameraParameterPath = Path.Combine(
                        Global.strConfigPath,
                        m_nCamNo == 0 ? "IDS_8345.ini" : "IDS_3522.ini");

                    // Parameter loading can change AOI and pixel format. Allocate image memory only
                    // after it, otherwise the buffer can retain the old dimensions.
                    EnsureSuccess(candidate.Parameter.Load(cameraParameterPath), "load parameters " + cameraParameterPath);
                    Rectangle rect;
                    EnsureSuccess(candidate.Size.AOI.Get(out rect), "read AOI");
                    EnsureSuccess(candidate.Memory.Allocate(), "allocate image memory");

                    candidate.EventDeviceRemove += CameraRemoved;
                    m_Cam = candidate;
                    m_nWidth = rect.Width;
                    m_nHeight = rect.Height;
                    CrashDiagnostics.RecordActivity("Camera initialized: CAM_" + (m_nCamNo + 1));
                }
                catch (Exception ex)
                {
                    if (candidate != null)
                    {
                        try
                        {
                            candidate.Exit();
                        }
                        catch
                        {
                        }
                    }

                    m_Cam = null;
                    CrashDiagnostics.ReportRecoverableException("Camera " + (m_nCamNo + 1) + " initialization", ex);
                }
            }
        }

        public static bool DeviceList()
        {
            uEye.Info.Camera.GetCameraList(out m_camList);

            if (m_camList.Count() < 2) return false;
            else                       return true;
        }

        public bool OpenCam()
        {
            uEye.Defines.Status status = m_Cam.Init(m_nCamNo);

            if (status == uEye.Defines.Status.SUCCESS) return true ;
            else                                       return false;
        }


        public bool Grab()
        {
            lock (_cameraSync)
            {
                if (m_Cam == null) return false;
#if !TEST
                if(!m_Cam.IsOpened)
                {
                    return false;
                }
                byte[] temp = GetImgBuff();
                long requiredLength = (long)m_nWidth * m_nHeight;
                if (temp == null || requiredLength <= 0 || temp.LongLength < requiredLength)
                {
                    CrashDiagnostics.RecordActivity(string.Format(
                        "Camera {0} returned an invalid buffer. Buffer={1}, Width={2}, Height={3}",
                        m_nCamNo + 1,
                        temp == null ? "null" : temp.LongLength.ToString(),
                        m_nWidth,
                        m_nHeight));
                    return false;
                }

                try
                {
                    Global.inforResult.dGray[m_nCamNo] = Global.clsAlgorithm[m_nCamNo].SetImage(
                        Global.formHDisplay[(int)HWindowType.MAIN_1 + m_nCamNo].HWindow,
                        temp,
                        m_nWidth,
                        m_nHeight);
                }
                catch (Exception ex)
                {
                    CrashDiagnostics.ReportRecoverableException("Camera " + (m_nCamNo + 1) + " image conversion", ex);
                    return false;
                }

#else
                byte[] temp;
                Bitmap bmp;
                if (m_nCamNo == 0)
                {
                    OpenFileDialog dlg = new OpenFileDialog();
                    dlg.ShowDialog();
                    if (dlg.FileName != null && dlg.FileName.Length > 0)
                    {
                        bmp = new Bitmap(dlg.FileName);
                    }
                    else
                        return false;
                }
                else
                {
                    OpenFileDialog dlg = new OpenFileDialog();
                    dlg.ShowDialog();
                    if (dlg.FileName != null && dlg.FileName.Length > 0)
                    {
                        bmp = new Bitmap(dlg.FileName);
                    }
                    else
                        return false;
                }

                // Bitmap을 Grayscale byte[] 배열로 변환
                m_nWidth = bmp.Width;
                m_nHeight = bmp.Height;
                temp = new byte[m_nWidth * m_nHeight];

                BitmapData bmpData = bmp.LockBits(
                    new Rectangle(0, 0, m_nWidth, m_nHeight),
                    ImageLockMode.ReadOnly,
                    bmp.PixelFormat);

                int bytesPerPixel = System.Drawing.Image.GetPixelFormatSize(bmp.PixelFormat) / 8;
                int stride = bmpData.Stride;
                IntPtr ptr = bmpData.Scan0;
                byte[] rawData = new byte[Math.Abs(stride) * m_nHeight];
                Marshal.Copy(ptr, rawData, 0, rawData.Length);

                for (int y = 0; y < m_nHeight; y++)
                {
                    for (int x = 0; x < m_nWidth; x++)
                    {
                        int idx = y * stride + x * bytesPerPixel;
                        if (bytesPerPixel >= 3)
                        {
                            // RGB -> Grayscale (BT.601)
                            byte b = rawData[idx];
                            byte g = rawData[idx + 1];
                            byte r = rawData[idx + 2];
                            temp[y * m_nWidth + x] = (byte)(0.299 * r + 0.587 * g + 0.114 * b);
                        }
                        else
                        {
                            // 이미 Grayscale
                            temp[y * m_nWidth + x] = rawData[idx];
                        }
                    }
                }

                bmp.UnlockBits(bmpData);
                bmp.Dispose();

                Global.inforResult.dGray[m_nCamNo] = Global.clsAlgorithm[m_nCamNo].SetImage(Global.formHDisplay[(int)HWindowType.MAIN_1 + m_nCamNo].HWindow, temp, m_nWidth, m_nHeight);
#endif

                return true;
            }
        }

        public bool IsOpen()
        {
            lock (_cameraSync)
            {
                try
                {
                    return m_Cam != null && m_Cam.IsOpened;
                }
                catch (Exception ex)
                {
                    CrashDiagnostics.ReportRecoverableException("Camera " + (m_nCamNo + 1) + " state check", ex);
                    return false;
                }
            }
        }

        public byte[] GetImgBuff()
        {
            lock (_cameraSync)
            {
                int memoryId = 0;
                bool memoryLocked = false;

                try
                {
                    if (m_Cam == null || !m_Cam.IsOpened)
                        return null;

                    EnsureSuccess(m_Cam.Acquisition.Freeze(1), "freeze image");

                    EnsureSuccess(m_Cam.Memory.GetActive(out memoryId), "get active image memory");
                    EnsureSuccess(m_Cam.Memory.Lock(memoryId), "lock image memory");
                    memoryLocked = true;

                    EnsureSuccess(m_Cam.Memory.GetSize(memoryId, out m_nWidth, out m_nHeight), "read image size");

                    byte[] imageData;
                    EnsureSuccess(m_Cam.Memory.CopyToArray(memoryId, out imageData), "copy image memory");

                    return imageData;
                }
                catch (Exception ex)
                {
                    CrashDiagnostics.ReportRecoverableException("Camera " + (m_nCamNo + 1) + " acquisition", ex);
                    return null;
                }
                finally
                {
                    if (memoryLocked && m_Cam != null)
                    {
                        try
                        {
                            m_Cam.Memory.Unlock(memoryId);
                        }
                        catch (Exception ex)
                        {
                            CrashDiagnostics.ReportRecoverableException("Camera " + (m_nCamNo + 1) + " memory unlock", ex);
                        }
                    }
                }
            }
        }

        public void Close()
        {
            lock (_cameraSync)
            {
                uEye.Info.Camera.EventNewDevice -= CameraAdded;

                if (m_Cam == null)
                    return;

                m_Cam.EventDeviceRemove -= CameraRemoved;
                m_Cam.Exit();
                m_Cam = null;
            }
        }

        private void EnsureSuccess(uEye.Defines.Status status, string operation)
        {
            if (status != uEye.Defines.Status.SUCCESS)
            {
                throw new InvalidOperationException(
                    "Camera " + (m_nCamNo + 1) + " failed to " + operation + ": " + status);
            }
        }


        public void SetParam(double dExposure, int nGain, double dGamma, bool bFilpX, bool bFilpY, bool bRotate )
        {
            lock (_cameraSync)
            {
                SetExposure(dExposure);
                SetGain(nGain);
                SetGamma(dGamma);
                MirrorCol(bFilpX);
                MirrorRow(bFilpY);

                if(bRotate)
                {
                    MirrorCol(bRotate);
                    MirrorRow(bRotate);
                }
            }
        }

        #region Paramer
        public void GetRangeExposure(out double dMin, out double dMax)
        {
            dMin = 0; dMax = 0;

            lock (_cameraSync)
            {
                try
                {
                    if (m_Cam == null) return;

                    uEye.Types.Range<Double> range;
                    EnsureSuccess(m_Cam.Timing.Exposure.GetRange(out range), "read exposure range");

                    dMin = range.Minimum;
                    dMax = range.Maximum;
                }
                catch (Exception ex)
                {
                    CrashDiagnostics.ReportRecoverableException("Camera " + (m_nCamNo + 1) + " exposure range", ex);
                }
            }
        }

        public int GetGain()
        {
            int nValue = 0;
            lock (_cameraSync)
            {
                try
                {
                    if (m_Cam == null) return nValue;
                    EnsureSuccess(m_Cam.Gain.Hardware.Scaled.GetMaster(out nValue), "read gain");
                }
                catch (Exception ex)
                {
                    CrashDiagnostics.ReportRecoverableException("Camera " + (m_nCamNo + 1) + " gain read", ex);
                }
            }

            return nValue;
        }

        public void SetGain(int nValue)
        {
            lock (_cameraSync)
            {
                try
                {
                    if (m_Cam == null) return;
                    EnsureSuccess(m_Cam.Gain.Hardware.Scaled.SetMaster(nValue), "set gain");
                }
                catch (Exception ex)
                {
                    CrashDiagnostics.ReportRecoverableException("Camera " + (m_nCamNo + 1) + " gain write", ex);
                }
            }
        }

        public double GetGamma()
        {
            int nValue = 0;
            lock (_cameraSync)
            {
                try
                {
                    if (m_Cam == null) return nValue;
                    EnsureSuccess(m_Cam.Gamma.Software.Get(out nValue), "read gamma");
                }
                catch (Exception ex)
                {
                    CrashDiagnostics.ReportRecoverableException("Camera " + (m_nCamNo + 1) + " gamma read", ex);
                }
            }

            return nValue / 100.0;
        }

        public void SetGamma(double dValue)
        {
            lock (_cameraSync)
            {
                try
                {
                    if (m_Cam == null) return;

                    int nValue = Convert.ToInt32(dValue * 100);
                    EnsureSuccess(m_Cam.Gamma.Software.Set(nValue), "set gamma");
                }
                catch (Exception ex)
                {
                    CrashDiagnostics.ReportRecoverableException("Camera " + (m_nCamNo + 1) + " gamma write", ex);
                }
            }
        }

        public double GetExposure()
        {
            double dValue = 0;
            lock (_cameraSync)
            {
                try
                {
                    if (m_Cam == null) return dValue;
                    EnsureSuccess(m_Cam.Timing.Exposure.Get(out dValue), "read exposure");
                }
                catch (Exception ex)
                {
                    CrashDiagnostics.ReportRecoverableException("Camera " + (m_nCamNo + 1) + " exposure read", ex);
                }
            }

            return dValue;
        }

        public void SetExposure(double dValue)
        {
            lock (_cameraSync)
            {
                try
                {
                    if (m_Cam == null) return;
                    EnsureSuccess(m_Cam.Timing.Exposure.Set(dValue), "set exposure");
                }
                catch (Exception ex)
                {
                    CrashDiagnostics.ReportRecoverableException("Camera " + (m_nCamNo + 1) + " exposure write", ex);
                }
            }
        }

        public double GetFocus()
        {
            UInt32 Value = 0;
            lock (_cameraSync)
            {
                try
                {
                    if (m_Cam == null) return Value;
                    EnsureSuccess(m_Cam.Focus.Manual.Get(out Value), "read focus");
                }
                catch (Exception ex)
                {
                    CrashDiagnostics.ReportRecoverableException("Camera " + (m_nCamNo + 1) + " focus read", ex);
                }
            }

            return Convert.ToDouble(Value);
        }

        public void SetFocus(double dValue)
        {
            lock (_cameraSync)
            {
                try
                {
                    if (m_Cam == null) return;
                    EnsureSuccess(m_Cam.Focus.Manual.Set(Convert.ToUInt32(dValue)), "set focus");
                }
                catch (Exception ex)
                {
                    CrashDiagnostics.ReportRecoverableException("Camera " + (m_nCamNo + 1) + " focus write", ex);
                }
            }
        }

        public void MirrorRow(bool bMirror)
        {
            lock (_cameraSync)
            {
                try
                {
                    if (m_Cam == null) return;
                    EnsureSuccess(
                        m_Cam.RopEffect.Set(uEye.Defines.RopEffectMode.LeftRight, bMirror),
                        "set horizontal mirror");
                }
                catch (Exception ex)
                {
                    CrashDiagnostics.ReportRecoverableException("Camera " + (m_nCamNo + 1) + " horizontal mirror", ex);
                }
            }
        }

        public void MirrorCol(bool bMirror)
        {
            lock (_cameraSync)
            {
                try
                {
                    if (m_Cam == null) return;
                    EnsureSuccess(
                        m_Cam.RopEffect.Set(uEye.Defines.RopEffectMode.UpDown, bMirror),
                        "set vertical mirror");
                }
                catch (Exception ex)
                {
                    CrashDiagnostics.ReportRecoverableException("Camera " + (m_nCamNo + 1) + " vertical mirror", ex);
                }
            }
        }
    }
    #endregion
}
