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
        
        int m_nCamNo = 0;
        string m_strSerial = "";

        public int Width { get => m_nWidth; }
        public int Height { get => m_nHeight; }

        int m_nWidth = 0;
        int m_nHeight = 0;

        private void CameraAdded(object sender, EventArgs e)
        {
            InitCam(m_strSerial);
        }

        private void CameraRemoved(object sender, EventArgs e)
        {
            m_Cam.Exit();
            m_Cam = null;
        }
        
        public ClsCamera(CamInfo nCamNo, string strSerial)
        {
            m_nCamNo = (int)nCamNo;
            m_strSerial = strSerial;
            InitCam(m_strSerial);
        } 

        public void InitCam(string strSerial)
        {
            if (m_Cam != null) return;

            m_Cam = new Camera();
            m_Cam.EventDeviceRemove += CameraRemoved;
            uEye.Info.Camera.EventNewDevice += CameraAdded;

            uEye.Defines.Status lCameraStatus;
            uEye.Types.CameraInformation[] cameraList;
            uEye.Info.Camera.GetCameraList(out cameraList);
            foreach (uEye.Types.CameraInformation info in cameraList)
            {
                if (strSerial == info.SerialNumber)
                {
                    lCameraStatus = m_Cam.Init((int)info.CameraID); //| (Int32)uEye.Defines.DeviceEnumeration.UseDeviceID);
                    lCameraStatus = m_Cam.Memory.Allocate();
                }
            }
            string strCamParamPath = "";
            if (m_nCamNo == 0) strCamParamPath = Global.strConfigPath + "\\IDS_8345.ini";// 현장확인필요
            else strCamParamPath = Global.strConfigPath + "\\IDS_3522.ini";// 현장확인필요

            m_Cam.Parameter.Load(strCamParamPath);
            System.Drawing.Rectangle rect;
            m_Cam.Size.AOI.Get(out rect);
            m_nWidth = rect.Width;
            m_nHeight = rect.Height;    
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
            if (m_Cam == null) return false;
#if !TEST
            if(!m_Cam.IsOpened)
            {
                return false;
            }
            byte[] temp = GetImgBuff();

            Global.inforResult.dGray[m_nCamNo] = Global.clsAlgorithm[m_nCamNo].SetImage(Global.formHDisplay[(int)HWindowType.MAIN_1 + m_nCamNo].HWindow, temp, m_nWidth, m_nHeight);

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

        public bool IsOpen()
        {
            if (m_Cam == null) return false;
            return m_Cam.IsOpened;
        }

        public byte[] GetImgBuff()
        {
            uEye.Defines.Status statusRet = uEye.Defines.Status.SUCCESS;

            try
            {
                if (m_Cam == null) return null;

                if (m_Cam.Acquisition.Freeze(1) == uEye.Defines.Status.Success)
                {
                    int s32MemId;

                    m_Cam.Memory.GetActive(out s32MemId);
                    m_Cam.Memory.Lock(s32MemId);

                    m_Cam.Memory.GetSize(s32MemId, out m_nWidth, out m_nHeight);

                    // 이미지 정보 가져오기
                    //m_Cam.Memory.GetSize(s32MemId, out int width, out int height);
                    //m_Cam.Memory.GetBitsPerPixel(s32MemId, out int bpp);

                    // 픽셀 데이터 포인터 가져오기
                    m_Cam.Memory.CopyToArray(s32MemId, out byte[] imageData);

                    m_Cam.Memory.Unlock(s32MemId);
                    m_Cam.Memory.Clear(s32MemId);
                    m_Cam.Memory.Free(s32MemId);

                    statusRet = m_Cam.Memory.Allocate();

                    return imageData;
                }
            }
            catch { return null; }

            return null;
        }


        public void SetParam(double dExposure, int nGain, double dGamma, bool bFilpX, bool bFilpY, bool bRotate )
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

        #region Paramer
        public void GetRangeExposure(out double dMin, out double dMax)
        {
            dMin = 0; dMax = 0;

            if (m_Cam == null) return;

            uEye.Types.Range<Double> range;
            m_Cam.Timing.Exposure.GetRange(out range);

            dMin = range.Minimum;
            dMax = range.Maximum;
        }

        public int GetGain()
        {
            int nValue = 0;
            try
            {
                if (m_Cam == null) return nValue;
                m_Cam.Gain.Hardware.Scaled.GetMaster(out nValue);
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.Print("[ERROR] " + ex.Message);
            }

            return nValue;
        }

        public void SetGain(int nValue)
        {
            try
            {
                if (m_Cam == null) return;
                m_Cam.Gain.Hardware.Scaled.SetMaster(nValue);
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.Print("[ERROR] " + ex.Message);
            }
        }

        public double GetGamma()
        {
            int nValue = 0;
            try
            {
                if (m_Cam == null) return nValue;
                m_Cam.Gamma.Software.Get(out nValue);
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.Print("[ERROR] " + ex.Message);
            }

            return Convert.ToDouble(nValue / 100) ;
        }

        public void SetGamma(double dValue)
        {
            try
            {
                if (m_Cam == null) return;

                int nValue = Convert.ToInt32(dValue * 100);
                m_Cam.Gamma.Software.Set(nValue);
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.Print("[ERROR] " + ex.Message);
            }
        }

        public double GetExposure()
        {
            double dValue = 0;
            try
            {
                if (m_Cam == null) return dValue;
                m_Cam.Timing.Exposure.Get(out dValue);
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.Print("[ERROR] " + ex.Message);
            }

            return dValue;
        }

        public void SetExposure(double dValue)
        {
            try
            {
                if (m_Cam == null) return;
                m_Cam.Timing.Exposure.Set(dValue);
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.Print("[ERROR] " + ex.Message);
            }
        }

        public double GetFocus()
        {
            UInt32 Value = 0;
            try
            {
                if (m_Cam == null) return Value;
                m_Cam.Focus.Manual.Get(out Value);
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.Print("[ERROR] " + ex.Message);
            }

            return Convert.ToDouble(Value);
        }

        public void SetFocus(double dValue)
        {
            try
            {
                if (m_Cam == null) return;
                m_Cam.Focus.Manual.Set(Convert.ToUInt32(dValue));
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.Print("[ERROR] " + ex.Message);
            }
        }

        public void MirrorRow(bool bMirror)
        {
            if (m_Cam == null) return;

            uEye.Defines.Status statusRet;

            statusRet = m_Cam.RopEffect.Set(uEye.Defines.RopEffectMode.LeftRight, bMirror);
        }

        public void MirrorCol(bool bMirror)
        {
            if (m_Cam == null) return;
            uEye.Defines.Status statusRet;

            statusRet = m_Cam.RopEffect.Set(uEye.Defines.RopEffectMode.UpDown, bMirror);
        }
    }
    #endregion
}
