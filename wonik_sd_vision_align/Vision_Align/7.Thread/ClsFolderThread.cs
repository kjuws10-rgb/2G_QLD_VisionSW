using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HalconDotNet;
using System.Collections.Concurrent;

namespace Vision_Align
{
    public class ClsFolderThread
    {
        Thread m_MainThread = null;

        private readonly AutoResetEvent _signal = new AutoResetEvent(false);
        private readonly ConcurrentQueue<SaveJob> _jobs = new ConcurrentQueue<SaveJob>();

        // RESULT 폴더 하위에서 날짜 기반 폴더를 관리할 서브디렉터리 목록
        private static readonly string[] ManagedSubDirs = { "Image", "Capture", "CALIBRATION" };

        private static readonly TimeSpan StorageCheckInterval = TimeSpan.FromMinutes(5);
        private DateTime _lastStorageCheck = DateTime.MinValue;

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

                // 5분 주기 저장소 관리
                if ((DateTime.Now - _lastStorageCheck) >= StorageCheckInterval)
                {
                    _lastStorageCheck = DateTime.Now;
                    ManageStorage();
                }
            }
        }

        // ─────────────────────────── Storage Management ───────────────────────────

        private void ManageStorage()
        {
            try
            {
                int  retentionDays = Global.PreConfig_Param.nStorageRetentionDays;
                long minFreeBytes  = (long)Global.PreConfig_Param.nStorageMinFreeGB * 1024L * 1024L * 1024L;

                // Step 1: 보관 기간 초과 폴더 삭제
                DeleteExpiredResultFolders(retentionDays);

                // Step 2: 잔여 공간 확인
                if (!HasSufficientFreeSpace(minFreeBytes, out long freeBytes))
                {
                    // Step 3: 공간 부족 → 오래된 폴더부터 추가 삭제
                    FreeSpaceByDeletingOldest(minFreeBytes);

                    // Step 4: 재확인 — 여전히 부족하면 알람
                    if (!HasSufficientFreeSpace(minFreeBytes, out freeBytes))
                    {
                        string msg = $"Drive free: {freeBytes / (1024.0 * 1024 * 1024):F1} GB  (min: {Global.PreConfig_Param.nStorageMinFreeGB} GB)";
                        Global.logger[LogType.SYSTEM].Write($"[Storage] ALARM - {msg}");
                        Global.clsAlarmManager.SetAlarm(AlarmCode.StorageLowError, msg);
                    }
                }
            }
            catch (Exception ex)
            {
                try { Global.logger[LogType.EXCEPTION].Write($"[Storage] ManageStorage error: {ex.Message}"); } catch { }
            }
        }

        /// <summary>RESULT 드라이브 잔여 공간을 반환합니다.</summary>
        private bool HasSufficientFreeSpace(long minFreeBytes, out long freeBytes)
        {
            freeBytes = long.MaxValue;
            try
            {
                string root = Path.GetPathRoot(Path.GetFullPath(Global.strRsltPath));
                freeBytes = new DriveInfo(root).AvailableFreeSpace;
                return freeBytes >= minFreeBytes;
            }
            catch
            {
                return true; // 확인 불가 시 알람 발생 방지
            }
        }

        /// <summary>
        /// RESULT 하위 ManagedSubDirs 안의 yyyyMMdd 폴더를 날짜 오름차순으로 반환합니다.
        /// </summary>
        private List<DirectoryInfo> GetResultDateFolders()
        {
            var folders = new List<DirectoryInfo>();
            if (!Directory.Exists(Global.strRsltPath)) return folders;

            foreach (string sub in ManagedSubDirs)
            {
                string subPath = Path.Combine(Global.strRsltPath, sub);
                if (!Directory.Exists(subPath)) continue;

                foreach (var dir in new DirectoryInfo(subPath).GetDirectories())
                {
                    if (DateTime.TryParseExact(dir.Name, "yyyyMMdd", CultureInfo.InvariantCulture,
                            DateTimeStyles.None, out _))
                        folders.Add(dir);
                }
            }

            // yyyyMMdd 이름 기준 오름차순(= 오래된 것부터)
            folders.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));
            return folders;
        }

        /// <summary>보관 기간 초과 yyyyMMdd 폴더를 삭제합니다.</summary>
        private void DeleteExpiredResultFolders(int retentionDays)
        {
            DateTime cutoff = DateTime.Today.AddDays(-retentionDays);

            foreach (var dir in GetResultDateFolders())
            {
                if (!DateTime.TryParseExact(dir.Name, "yyyyMMdd", CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out DateTime folderDate)) continue;

                if (folderDate >= cutoff) continue;

                try
                {
                    dir.Delete(true);
                    Global.logger[LogType.SYSTEM].Write($"[Storage] Deleted expired: {dir.FullName}");
                }
                catch (Exception ex)
                {
                    Global.logger[LogType.EXCEPTION].Write($"[Storage] Delete failed: {dir.FullName} - {ex.Message}");
                }
            }
        }

        /// <summary>잔여 공간이 확보될 때까지 오래된 폴더부터 삭제합니다.</summary>
        private void FreeSpaceByDeletingOldest(long minFreeBytes)
        {
            foreach (var dir in GetResultDateFolders())
            {
                if (HasSufficientFreeSpace(minFreeBytes, out _)) break;

                try
                {
                    dir.Delete(true);
                    Global.logger[LogType.SYSTEM].Write($"[Storage] Deleted for space: {dir.FullName}");
                }
                catch (Exception ex)
                {
                    Global.logger[LogType.EXCEPTION].Write($"[Storage] Delete failed: {dir.FullName} - {ex.Message}");
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
