using HalconDotNet;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Vision_Align
{
    public class ClsCsvFile
    {
        private const int FileWriteRetryCount = 5;
        private const int FileWriteRetryDelayMs = 200;
        private static readonly object FileWriteLock = new object();

        public ClsCsvFile()
        {

        }

        private void WriteHeader(string strPath)
        {
            string strHeader = "DateTime, SystemMode, AlignMode, " +
                               "AlignSpecXY[um], AlignSpecTh[mdeg], " +
                               "Retry, ProcessTime[ms], Judge, " +
                               "Final ErrX[um], Final ErrY[um], Final ErrT[mDeg], " +
                               "AlarmNo, " +
                               "Motor X1[um], Motor X2[um], Motor Y1[um], Motor Y2[um], " +
                               "Motor X1Move[um], Motor X2Move[um], Motor Y1Move[um], Motor Y2Move[um],";

            for(int n = 0; n < (int)CamInfo.MAX; n++)
            {
                strHeader += string.Format("Cam{0} Glass Row[pixel], ", n + 1);
                strHeader += string.Format("Cam{0} Glass Col[pixel], ", n + 1);
                strHeader += string.Format("Cam{0} Glass Row[um], "   , n + 1);
                strHeader += string.Format("Cam{0} Glass Col[um], "   , n + 1);
                strHeader += string.Format("Cam{0} Glass Scale, "     , n + 1);
                strHeader += string.Format("Cam{0} Glass Score, "     , n + 1);

                strHeader += string.Format("Cam{0} Mask Row[pixel], ", n + 1);
                strHeader += string.Format("Cam{0} Mask Col[pixel], ", n + 1);
                strHeader += string.Format("Cam{0} Mask Row[um], "   , n + 1);
                strHeader += string.Format("Cam{0} Mask Col[um], "   , n + 1);
                strHeader += string.Format("Cam{0} Mask Scale, "     , n + 1);
                strHeader += string.Format("Cam{0} Mask Score, "     , n + 1);

                strHeader += string.Format("Cam{0} SaveImageNo,"      , n + 1);
                strHeader += string.Format("Cam{0} MeasureTime[msec],", n + 1);
                strHeader += string.Format("Cam{0} InTencity,"        , n + 1);
            }


            AppendLineWithRetry(strPath, strHeader, true);
        }

        private void WriteCalHeader(string strPath)
        {
            string strHeader = "DateTime, Average Image Count,";
            string dir = strPath.Remove(strPath.LastIndexOf('\\'));
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            for (int n = 0; n < (int)CamInfo.MAX; n++)
            {
                strHeader += string.Format("Cam{0} Pixel Resolution_X, Cam{0} Pixel Resolution_Y, Cam{0} Tilt,", n + 1);
                for (int i = 0; i < 9; i++)
                {
                    strHeader += string.Format("Cam{0} Point_{1}.X[pixel], ", n + 1, i + 1);
                    strHeader += string.Format("Cam{0} Point_{1}.Y[pixel], ", n + 1, i + 1);
                }
            }

            AppendLineWithRetry(strPath, strHeader, true);
        }

        private void WriteVibeHeader(string strPath)
        {
            string strHeader = "DateTime, Average Image Count,";
            string dir = strPath.Remove(strPath.LastIndexOf('\\'));
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);


            for (int n = 0; n < (int)CamInfo.MAX; n++)
            {
                strHeader += string.Format("Cam{0} Pixel Resolution_X, Cam{0} Pixel Resolution_Y, Cam{0} Tilt,", n + 1);
                for (int i = 0; i < Global.Calibration_Param.AverageImageCount; i++)
                {
                    strHeader += string.Format("Cam{0} Point_{1}.X[pixel], ", n + 1, i + 1);
                    strHeader += string.Format("Cam{0} Point_{1}.Y[pixel], ", n + 1, i + 1);
                }
            }

            AppendLineWithRetry(strPath, strHeader, true);
        }


        public void WriteCsv(string alignStep)
        {
            DateTime time = DateTime.Now;
            string strTime1 = string.Format("{0:D4}/{1:D2}/{2:D2}", time.Year, time.Month, time.Day);
            string strTime2 = string.Format("{0:D2}:{1:D2}:{2:D2}:{3:D3}", time.Hour, time.Minute, time.Second, time.Millisecond);

            string strName = string.Format("[{0:D4}{1:D2}{2:D2}]AlignDataResult.csv", time.Year, time.Month, time.Day);
            string strPath = string.Format("{0}\\{1}", Global.strRsltPath, strName);

            if (!File.Exists(strPath))
                WriteHeader(strPath);

            string strData = "";
            

            strData  = string.Format("[{0} {1}], ", strTime1, strTime2);
            strData += string.Format("{0}, ", Global.bAutoMode ? "AUTO" : "MANUAL") ;
            strData += string.Format("{0}, ", alignStep);


            strData += string.Format("{0:F6}, ", Global.PreConfig_Param.dPreAlignXY);
            strData += string.Format("{0:F6}, ", Global.PreConfig_Param.dPreAlignTh);
            strData += string.Format("{0}, ", Global.inforResult.nRetry);
            strData += string.Format("{0:F6}, ", Global.inforResult.dFullProcessTime);
            strData += string.Format("{0}, ", Global.inforResult.strJudg);

            strData += string.Format("{0:F6}, ", Global.inforResult.ContactErrorX); 
            strData += string.Format("{0:F6}, ", Global.inforResult.ContactErrorY);
            strData += string.Format("{0:F6}, ", Global.inforResult.ContactErrorT);

            strData += string.Format("{0:F6}, ", Global.m_AlarmCode.ToString()); 
            strData += string.Format("{0:F6}, ", Global.inforPLC.InStageCurrentU);
            strData += string.Format("{0:F6}, ", Global.inforPLC.InStageCurrentW);
            strData += string.Format("{0:F6}, ", Global.inforPLC.InStageCurrentV);
            strData += string.Format("{0:F6}, ", 0);
            strData += string.Format("{0:F6}, ", Global.inforResult.dMove_UVW_X1);
            strData += string.Format("{0:F6}, ", Global.inforResult.dMove_UVW_X2);
            strData += string.Format("{0:F6}, ", Global.inforResult.dMove_UVW_Y1);
            strData += string.Format("{0:F6}, ", Global.inforResult.dMove_UVW_Y2);

            for (int n  = 0; n < (int)CamInfo.MAX; n++)
            {
                strData += string.Format("{0:F6}, ", Global.inforResult.dTarget_Y[n]);
                strData += string.Format("{0:F6}, ", Global.inforResult.dTarget_X[n]);
                strData += string.Format("{0:F6}, ", Global.inforResult.dCenterToTargetY[n]);
                strData += string.Format("{0:F6}, ", Global.inforResult.dCenterToTargetX[n]);
                strData += string.Format("{0:F6}, ", Global.inforResult.dTarget_Scale[n]);
                strData += string.Format("{0:F6}, ", Global.inforResult.dTarget_Score[n]);

                strData += string.Format("{0:F6}, ", Global.inforResult.dMark_Y[n]);
                strData += string.Format("{0:F6}, ", Global.inforResult.dMark_X[n]);
                strData += string.Format("{0:F6}, ", Global.inforResult.dCenterToMarkY[n]);
                strData += string.Format("{0:F6}, ", Global.inforResult.dCenterToMarkX[n]);
                strData += string.Format("{0:F6}, ", Global.inforResult.dMark_Scale[n]);
                strData += string.Format("{0:F6}, ", Global.inforResult.dMark_Score[n]);

                strData += string.Format("{0:F6}, ", Global.inforResult.strSaveImgNo[n]);
                strData += string.Format("{0:F6}, ", Global.inforResult.dProcessTime[n]);
                strData += string.Format("{0:F6}, ", Global.inforResult.dGray[n]);
            }

            AppendLineWithRetry(strPath, strData);

        }

        public List<string> ReadCsvColumn(string strPaht, string strHeader)
        {
            List<string> result = new List<string>();
            using (var sr = new StreamReader(strPaht))
            {
                // 첫 줄 = 헤더
                string headerLine = sr.ReadLine();
                if (headerLine == null)
                    return result;

                headerLine = headerLine.Replace(" ", "");

                string[] headers = headerLine.Split(',');

                // 찾는 헤더의 인덱스 구하기
                int colIndex = Array.IndexOf(headers, strHeader);
                if (colIndex < 0)
                    return result;   // 없는 헤더 → 빈 리스트 반환

                // 나머지 줄 읽기
                while (!sr.EndOfStream)
                {
                    Thread.Sleep(10);
                    string line = sr.ReadLine();
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    string[] cols = line.Split(',');

                    if (cols.Length > colIndex)
                        result.Add(cols[colIndex]);
                }
            }

            return result;
        }

        public void WriteCalibrationCsv(PointF[][] camCalPoints)
        {
            DateTime time = DateTime.Now;
            string strTime1 = string.Format("{0:D4}{1:D2}{2:D2}", time.Year, time.Month, time.Day);
            string strTime2 = string.Format("{0:D2}{1:D2}{2:D2}", time.Hour, time.Minute, time.Second);

            string strName = string.Format("9PointResult.csv");
            string strPath = Path.Combine(Global.strRsltPath, "CALIBRATION", time.ToString("yyyyMMdd"),  strName);

            if (!File.Exists(strPath))
                WriteCalHeader(strPath);

            string strData = "";

            strData = string.Format("[{0} {1}], ", strTime1, strTime2);


            strData += string.Format("{0}, ", Global.Calibration_Param.AverageImageCount); // Image Average Count


            for (int n  = 0; n < (int)CamInfo.MAX; n++)
            {
                strData += string.Format("{0:F6}, ", Global.Calibration_Param.CameraPixelResolutionX[n]);
                strData += string.Format("{0:F6}, ", Global.Calibration_Param.CameraPixelResolutionY[n]);
                strData += string.Format("{0:F6}, ", Global.Calibration_Param.CameraTilt[n]);

                for(int i = 0; i < camCalPoints[n].Length; i++)
                {
                    strData += string.Format("{0:F3}, ", camCalPoints[n][i].X);
                    strData += string.Format("{0:F3}, ", camCalPoints[n][i].Y);
                }
            }

            AppendLineWithRetry(strPath, strData);
        }
   
    
        public void WriteVibrationCsv(PointF[][] camViberationPoints)
        {
            DateTime time = DateTime.Now;
            string strTime1 = string.Format("{0:D4}{1:D2}{2:D2}", time.Year, time.Month, time.Day);
            string strTime2 = string.Format("{0:D2}{1:D2}{2:D2}", time.Hour, time.Minute, time.Second);

            string strName = string.Format("ViberationResult.csv");
            string strPath = Path.Combine(Global.strRsltPath, "CALIBRATION", time.ToString("yyyyMMdd"),  strName);

            if (!File.Exists(strPath))
                WriteVibeHeader(strPath);

            string strData = "";

            strData = string.Format("[{0} {1}], ", strTime1, strTime2);


            strData += string.Format("{0}, ", Global.Calibration_Param.AverageImageCount); // Image Average Count


            for (int n  = 0; n < (int)CamInfo.MAX; n++)
            {
                strData += string.Format("{0:F6}, ", Global.Calibration_Param.CameraPixelResolutionX[n]);
                strData += string.Format("{0:F6}, ", Global.Calibration_Param.CameraPixelResolutionY[n]);
                strData += string.Format("{0:F6}, ", Global.Calibration_Param.CameraTilt[n]);

                for(int i = 0; i < camViberationPoints[n].Length; i++)
                {
                    strData += string.Format("{0:F3}, ", camViberationPoints[n][i].X);
                    strData += string.Format("{0:F3}, ", camViberationPoints[n][i].Y);
                }
            }

            AppendLineWithRetry(strPath, strData);
        }

        private static void AppendLineWithRetry(string path, string line, bool onlyIfEmpty = false)
        {
            Exception lastException = null;

            for (int attempt = 1; attempt <= FileWriteRetryCount; attempt++)
            {
                try
                {
                    lock (FileWriteLock)
                    {
                        string directory = Path.GetDirectoryName(path);
                        if (!string.IsNullOrWhiteSpace(directory))
                            Directory.CreateDirectory(directory);

                        using (FileStream stream = new FileStream(
                            path,
                            FileMode.OpenOrCreate,
                            FileAccess.Write,
                            FileShare.ReadWrite))
                        {
                            if (onlyIfEmpty && stream.Length > 0)
                                return;

                            stream.Seek(0, SeekOrigin.End);
                            using (StreamWriter writer = new StreamWriter(stream, new UTF8Encoding(false)))
                            {
                                writer.WriteLine(line);
                            }
                        }
                    }

                    return;
                }
                catch (IOException ex)
                {
                    lastException = ex;
                }
                catch (UnauthorizedAccessException ex)
                {
                    lastException = ex;
                }

                if (attempt < FileWriteRetryCount)
                    Thread.Sleep(FileWriteRetryDelayMs * attempt);
            }

            throw new IOException(
                "Failed to append CSV after " + FileWriteRetryCount + " attempts: " + path,
                lastException);
        }
    }

}
