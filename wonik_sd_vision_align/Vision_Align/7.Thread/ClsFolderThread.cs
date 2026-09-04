using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HalconDotNet;
using System.IO;
using System.Collections.Concurrent;

namespace Vision_Align
{
    public class ClsFolderThread
    {
        Thread m_MainThread = null;

        private readonly AutoResetEvent _signal = new AutoResetEvent(false);
        private readonly ConcurrentQueue<SaveJob> _jobs = new ConcurrentQueue<SaveJob>();

        private class SaveJob
        {
            public DateTime Ts;   // AutoThread에서 만든 기준 시각
            public string Tag;    // "PRE_ALIGN" 등
        }

        public ClsFolderThread()
        {
            m_MainThread = new Thread(MainThreadRunAsync);
            m_MainThread.Start();
        }

        public void Release()
        {
            if (m_MainThread != null)
                m_MainThread.Abort();

        }
        public void RequestSave(DateTime ts, string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
                tag = "RESULT";

            _jobs.Enqueue(new SaveJob { Ts = ts, Tag = tag });
            _signal.Set();
        }

        public void MainThreadRunAsync()
        {
            while (true)
            {
                Thread.Sleep(10);
                //OST 결과 이미지, 및 Data 저장.
                //OST ClsCsvFile 활용하여 결과값 저장

                if (_jobs.IsEmpty)
                    _signal.WaitOne(200);

                while (_jobs.TryDequeue(out var job))
                {
                    Thread.Sleep(10);
                    try
                    {
                        // BMP 2장 저장 + SaveImageNo 세팅
                        SaveBothCamsBmpAndSetSaveNo(job.Ts, job.Tag);

                        // CSV 1줄 저장 (Global 기반) 
                        Global.clsCSV.WriteCsv(job.Tag);
                    }
                    catch
                    {
                        // 예외 로그 처리
                    }
                }
            }
        }


        public void SaveBothCamsBmpAndSetSaveNo(DateTime ts, string tag)
        {
            string dir = Path.Combine(Global.strRsltPath, "Image", ts.ToString("yyyyMMdd"));
            Directory.CreateDirectory(dir);

            for (int cam = 0; cam < (int)CamInfo.MAX; cam++) // CAM_1, CAM_2
            {
                var algo = Global.clsAlgorithm[cam];
                if (algo == null)
                {
                    Global.inforResult.strSaveImgNo[cam] = "";
                    continue;
                }

                string fileName = $"{tag}_Cam{cam + 1}_{ts:yyyyMMdd_HHmmss_fff}.bmp";
                string fullPath = Path.Combine(dir, fileName);

                Global.inforResult.strSaveImgNo[cam] = fileName;

                if (algo.TryCopyOriImage(out HObject copy))
                {
                    try
                    {
                        HOperatorSet.WriteImage(copy, "bmp", 0, fullPath);
                    }
                    finally
                    {
                        copy.Dispose();
                    }
                }
                else
                {
                    // 저장 실패 시 CSV에도 빈 값 남김
                    Global.inforResult.strSaveImgNo[cam] = "";
                }
            }
        }

        public void SaveBothCamsBmp(DateTime ts, string tag)
        {
            SaveBothCamsBmp(ts, tag, "Capture",true);
        }

        public void SaveBothCamsBmp(DateTime ts, string tag, string folderName, bool isOriginal)
        {
            string dir = Path.Combine(Global.strRsltPath, folderName, ts.ToString("yyyyMMdd"));
            Directory.CreateDirectory(dir);

            for (int cam = 0; cam < (int)CamInfo.MAX; cam++) // CAM_1, CAM_2
            {
                var algo = Global.clsAlgorithm[cam];
                if (algo == null)
                {
                    continue;
                }

                string fileName = $"{tag}_Cam{cam + 1}_{ts:yyyyMMdd_HHmmss_fff}.bmp";
                string fullPath = Path.Combine(dir, fileName);

                if (isOriginal)
                {
                    if (algo.TryCopyOriImage(out HObject copy))
                    {
                        try
                        {
                            HOperatorSet.WriteImage(copy, "bmp", 0, fullPath);
                        }
                        finally
                        {
                            copy.Dispose();
                        }
                    }
                    else
                    {
                    }
                }
                else
                {
                    if (algo.TryCopyOriImage(out HObject copy))
                    {
                        try
                        {
                            HOperatorSet.WriteImage(copy, "bmp", 0, fullPath);
                        }
                        finally
                        {
                            copy.Dispose();
                        }
                    }
                    else
                    {
                    }

                }

            }
        }
    }
}
