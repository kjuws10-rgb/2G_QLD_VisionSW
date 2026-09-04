using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Vision_Align
{
    /// <summary>
    /// Records enough process and machine state to diagnose both managed exceptions and
    /// abrupt/native terminations. This class deliberately has no dependency on NLog so
    /// it can still write before the application logger and hardware objects are ready.
    /// </summary>
    internal static class CrashDiagnostics
    {
        private const int HeartbeatIntervalMs = 5000;
        private const int RecoverableReportThrottleSeconds = 30;

        private static readonly object SyncRoot = new object();
        private static readonly Dictionary<string, WorkerState> WorkerStates =
            new Dictionary<string, WorkerState>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, DateTime> RecoverableReports =
            new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);

        private static readonly DateTime StartedUtc = DateTime.UtcNow;
        private static readonly string SessionId = Guid.NewGuid().ToString("N");

        private static Timer _heartbeatTimer;
        private static string _diagnosticDirectory;
        private static string _activeSessionPath;
        private static volatile string _lastActivity = "Program entry";
        private static volatile bool _loggerReady;
        private static volatile bool _applicationReady;
        private static volatile bool _cleanShutdown;
        private static int _reportInProgress;

        public static string DiagnosticDirectory
        {
            get
            {
                lock (SyncRoot)
                {
                    return _diagnosticDirectory ?? ResolveDiagnosticDirectory();
                }
            }
        }

        public static void StartSession()
        {
            try
            {
                lock (SyncRoot)
                {
                    if (_heartbeatTimer != null)
                        return;

                    _diagnosticDirectory = ResolveDiagnosticDirectory();
                    Directory.CreateDirectory(_diagnosticDirectory);
                    _activeSessionPath = Path.Combine(_diagnosticDirectory, "active-session.log");

                    PreservePreviousUncleanSession();
                    WriteHeartbeatUnsafe("Running");

                    _heartbeatTimer = new Timer(
                        HeartbeatCallback,
                        null,
                        HeartbeatIntervalMs,
                        HeartbeatIntervalMs);
                }
            }
            catch (Exception ex)
            {
                // Diagnostics must never prevent the production application from starting.
                WriteEmergencyLine("Diagnostic session startup failed: " + ex);
            }
        }

        public static void MarkLoggerReady()
        {
            _loggerReady = true;
            RecordActivity("Application logger initialized");
        }

        public static void MarkApplicationReady()
        {
            _applicationReady = true;
            RecordActivity("Application initialization completed");
            WriteHeartbeat("Running");
        }

        public static void RecordActivity(string activity)
        {
            if (string.IsNullOrWhiteSpace(activity))
                return;

            _lastActivity = activity.Trim();
        }

        public static void PulseWorker(string workerName, string state)
        {
            if (string.IsNullOrWhiteSpace(workerName))
                return;

            lock (SyncRoot)
            {
                WorkerStates[workerName] = new WorkerState
                {
                    TimestampUtc = DateTime.UtcNow,
                    State = state ?? string.Empty
                };
            }
        }

        public static void ReportWorkerException(string workerName, Exception exception, string state)
        {
            string origin = string.Format(
                CultureInfo.InvariantCulture,
                "Worker {0} ({1})",
                workerName ?? "unknown",
                state ?? "state unavailable");

            ReportException(origin, exception, false);
        }

        public static void ReportRecoverableException(string origin, Exception exception)
        {
            if (exception == null)
                return;

            string throttleKey = (origin ?? "recoverable") + "|" + exception.GetType().FullName + "|" + exception.Message;
            DateTime now = DateTime.UtcNow;

            lock (SyncRoot)
            {
                DateTime lastReported;
                if (RecoverableReports.TryGetValue(throttleKey, out lastReported) &&
                    (now - lastReported).TotalSeconds < RecoverableReportThrottleSeconds)
                {
                    return;
                }

                RecoverableReports[throttleKey] = now;
            }

            ReportException(origin ?? "Recoverable exception", exception, false);
        }

        public static string ReportException(string origin, object exceptionObject, bool isTerminating)
        {
            Exception exception = exceptionObject as Exception;
            if (exception == null)
            {
                exception = new NotSupportedException(
                    "Unhandled exception object does not derive from System.Exception: " +
                    SafeToString(exceptionObject));
            }

            if (Interlocked.Exchange(ref _reportInProgress, 1) != 0)
            {
                WriteEmergencyLine("Concurrent exception while another report was being written: " + exception);
                return _diagnosticDirectory ?? "Diagnostic report unavailable";
            }

            string reportPath = "Diagnostic report unavailable";

            try
            {
                string diagnosticDirectory = DiagnosticDirectory;
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff", CultureInfo.InvariantCulture);
                string severity = isTerminating ? "FATAL" : "ERROR";
                string safeOrigin = MakeSafeFileName(origin);
                string baseName = string.Format(
                    CultureInfo.InvariantCulture,
                    "INCIDENT_{0}_{1}_{2}",
                    timestamp,
                    severity,
                    safeOrigin);

                reportPath = Path.Combine(diagnosticDirectory, baseName + ".log");

                StringBuilder report = new StringBuilder();
                report.AppendLine("Vision_Align incident report");
                report.AppendLine("============================");
                report.AppendLine("RecordedLocal=" + DateTime.Now.ToString("O", CultureInfo.InvariantCulture));
                report.AppendLine("RecordedUtc=" + DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture));
                report.AppendLine("Origin=" + (origin ?? "unknown"));
                report.AppendLine("IsTerminating=" + isTerminating);
                report.AppendLine();
                report.AppendLine(BuildSnapshot(isTerminating ? "Terminating" : "Exception contained"));
                report.AppendLine();
                report.AppendLine("Exception");
                report.AppendLine("---------");
                report.AppendLine(exception.ToString());

                WriteAllTextSafe(reportPath, report.ToString());
                AppendIncidentIndex(reportPath, origin, exception, isTerminating);
                TryWriteApplicationExceptionLog(origin, exception, isTerminating);

                if (isTerminating)
                    TryWriteMiniDump(Path.ChangeExtension(reportPath, ".dmp"));

                WriteHeartbeat(isTerminating ? "Terminating" : "Running after contained exception");
            }
            catch (Exception reportException)
            {
                WriteEmergencyLine(
                    "Failed to create incident report. Original: " + exception +
                    Environment.NewLine + "Reporter: " + reportException);
            }
            finally
            {
                Interlocked.Exchange(ref _reportInProgress, 0);
            }

            return reportPath;
        }

        public static void MarkCleanShutdown(string reason)
        {
            lock (SyncRoot)
            {
                if (_cleanShutdown)
                    return;

                _cleanShutdown = true;
                _lastActivity = "Clean shutdown: " + (reason ?? "unspecified");

                try
                {
                    string snapshot = BuildSnapshot("Clean shutdown");
                    string lastCleanPath = Path.Combine(DiagnosticDirectory, "last-clean-session.log");
                    WriteAllTextSafe(lastCleanPath, snapshot);

                    if (!string.IsNullOrEmpty(_activeSessionPath) && File.Exists(_activeSessionPath))
                        File.Delete(_activeSessionPath);
                }
                catch (Exception ex)
                {
                    WriteEmergencyLine("Failed to mark clean shutdown: " + ex);
                }
            }
        }

        public static void Stop()
        {
            Timer timer;

            lock (SyncRoot)
            {
                timer = _heartbeatTimer;
                _heartbeatTimer = null;
            }

            if (timer != null)
                timer.Dispose();
        }

        private static void HeartbeatCallback(object state)
        {
            WriteHeartbeat("Running");
        }

        private static void WriteHeartbeat(string status)
        {
            lock (SyncRoot)
            {
                if (_cleanShutdown || string.IsNullOrEmpty(_activeSessionPath))
                    return;

                try
                {
                    WriteHeartbeatUnsafe(status);
                }
                catch (Exception ex)
                {
                    WriteEmergencyLine("Heartbeat write failed: " + ex.Message);
                }
            }
        }

        private static void WriteHeartbeatUnsafe(string status)
        {
            WriteAllTextAtomic(_activeSessionPath, BuildSnapshot(status));
        }

        private static string BuildSnapshot(string status)
        {
            StringBuilder snapshot = new StringBuilder();
            using (Process process = Process.GetCurrentProcess())
            {
                Assembly assembly = Assembly.GetExecutingAssembly();

                snapshot.AppendLine("Vision_Align session state");
                snapshot.AppendLine("==========================");
                snapshot.AppendLine("Status=" + status);
                snapshot.AppendLine("SessionId=" + SessionId);
                snapshot.AppendLine("StartedUtc=" + StartedUtc.ToString("O", CultureInfo.InvariantCulture));
                snapshot.AppendLine("HeartbeatUtc=" + DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture));
                snapshot.AppendLine("LastActivity=" + _lastActivity);
                snapshot.AppendLine("ProcessId=" + process.Id.ToString(CultureInfo.InvariantCulture));
                snapshot.AppendLine("ProcessName=" + process.ProcessName);
                snapshot.AppendLine("Executable=" + SafeGet(() => assembly.Location));
                snapshot.AppendLine("Version=" + SafeGet(() => assembly.GetName().Version.ToString()));
                snapshot.AppendLine("WorkingDirectory=" + SafeGet(() => Environment.CurrentDirectory));
                snapshot.AppendLine("MachineName=" + SafeGet(() => Environment.MachineName));
                snapshot.AppendLine("UserInteractive=" + Environment.UserInteractive);
                snapshot.AppendLine("OSVersion=" + SafeGet(() => Environment.OSVersion.ToString()));
                snapshot.AppendLine("ClrVersion=" + SafeGet(() => Environment.Version.ToString()));
                snapshot.AppendLine("Is64BitProcess=" + Environment.Is64BitProcess);
                snapshot.AppendLine("WorkingSetBytes=" + SafeGet(() => process.WorkingSet64.ToString(CultureInfo.InvariantCulture)));
                snapshot.AppendLine("PrivateMemoryBytes=" + SafeGet(() => process.PrivateMemorySize64.ToString(CultureInfo.InvariantCulture)));
                snapshot.AppendLine("ManagedMemoryBytes=" + GC.GetTotalMemory(false).ToString(CultureInfo.InvariantCulture));
                snapshot.AppendLine("ThreadCount=" + SafeGet(() => process.Threads.Count.ToString(CultureInfo.InvariantCulture)));

                if (_applicationReady)
                    AppendApplicationState(snapshot);
                else
                    snapshot.AppendLine("ApplicationState=Initialization not completed");

                lock (SyncRoot)
                {
                    foreach (KeyValuePair<string, WorkerState> pair in WorkerStates)
                    {
                        snapshot.AppendLine(string.Format(
                            CultureInfo.InvariantCulture,
                            "Worker.{0}=Utc:{1:O}; State:{2}",
                            pair.Key,
                            pair.Value.TimestampUtc,
                            pair.Value.State));
                    }
                }
            }

            return snapshot.ToString();
        }

        private static void AppendApplicationState(StringBuilder snapshot)
        {
            try
            {
                snapshot.AppendLine("AutoMode=" + Global.bAutoMode);
                snapshot.AppendLine("AlarmCode=" + Global.m_AlarmCode);
                snapshot.AppendLine("CurrentView=" + Global.m_nCurrViewer);
                snapshot.AppendLine("RequestedView=" + Global.m_nChangViewer);
                snapshot.AppendLine("CameraConnected=" + Global.bConnectCam);
                snapshot.AppendLine("MotionConnected=" + Global.bConnectAxisZ);
                snapshot.AppendLine("PlcConnected=" + Global.bConnectPLC);
                snapshot.AppendLine("LightConnected=" + Global.bConnectLight);
                snapshot.AppendLine("CameraLive=" + SafeGet(() => Global.inforCam.bLive.ToString()));
                snapshot.AppendLine("AutoStep=" + SafeGet(() => Global.clsAutoThread == null ? "not-created" : Global.clsAutoThread.CurrentStep.ToString()));
                snapshot.AppendLine("PLC.InPreAlignRequest=" + SafeGet(() => Global.inforPLC.InPreAlignRequest.ToString()));
                snapshot.AppendLine("PLC.InContactRequest=" + SafeGet(() => Global.inforPLC.InContactRequest.ToString()));
                snapshot.AppendLine("PLC.InModelRequest=" + SafeGet(() => Global.inforPLC.InMaskModelRequest.ToString()));
                snapshot.AppendLine("PLC.InCalibrationRequest=" + SafeGet(() => Global.inforPLC.InCalibrationRequest.ToString()));
                snapshot.AppendLine("PLC.OutReady=" + SafeGet(() => Global.inforPLC.OutReady.ToString()));
                snapshot.AppendLine("PLC.OutBusy=" + SafeGet(() => Global.inforPLC.OutBusy.ToString()));
                snapshot.AppendLine("LastJudgement=" + SafeGet(() => Global.inforResult.strJudg));
            }
            catch (Exception ex)
            {
                snapshot.AppendLine("ApplicationStateError=" + ex.GetType().FullName + ": " + ex.Message);
            }
        }

        private static void PreservePreviousUncleanSession()
        {
            if (!File.Exists(_activeSessionPath))
                return;

            try
            {
                string previousState = File.ReadAllText(_activeSessionPath);
                string incidentPath = Path.Combine(
                    _diagnosticDirectory,
                    "INCIDENT_" + DateTime.Now.ToString("yyyyMMdd_HHmmss_fff", CultureInfo.InvariantCulture) +
                    "_UNCLEAN_SHUTDOWN.log");

                StringBuilder incident = new StringBuilder();
                incident.AppendLine("A previous Vision_Align session did not record a clean shutdown.");
                incident.AppendLine("This can indicate power loss, Task Manager termination, native SDK failure, or process crash.");
                incident.AppendLine("DetectedLocal=" + DateTime.Now.ToString("O", CultureInfo.InvariantCulture));
                incident.AppendLine();
                incident.AppendLine("Last heartbeat from previous session");
                incident.AppendLine("------------------------------------");
                incident.AppendLine(previousState);

                WriteAllTextSafe(incidentPath, incident.ToString());
                AppendIncidentIndex(incidentPath, "Previous session", null, true);
            }
            catch (Exception ex)
            {
                WriteEmergencyLine("Failed to preserve previous session state: " + ex);
            }
        }

        private static void AppendIncidentIndex(string reportPath, string origin, Exception exception, bool fatal)
        {
            string indexPath = Path.Combine(DiagnosticDirectory, "incidents.log");
            string line = string.Format(
                CultureInfo.InvariantCulture,
                "[{0:O}] Fatal={1}; Origin={2}; Exception={3}; Report={4}{5}",
                DateTime.Now,
                fatal,
                origin ?? "unknown",
                exception == null ? "n/a" : exception.GetType().FullName + ": " + exception.Message,
                reportPath,
                Environment.NewLine);

            AppendAllTextSafe(indexPath, line);
        }

        private static void TryWriteApplicationExceptionLog(string origin, Exception exception, bool fatal)
        {
            if (!_loggerReady)
                return;

            try
            {
                if (Global.logger != null && Global.logger.ContainsKey(LogType.EXCEPTION))
                {
                    Global.logger[LogType.EXCEPTION].Write(string.Format(
                        CultureInfo.InvariantCulture,
                        "[{0}] Fatal={1}{2}{3}",
                        origin,
                        fatal,
                        Environment.NewLine,
                        exception));
                }
            }
            catch (Exception ex)
            {
                WriteEmergencyLine("Application logger failed while reporting an exception: " + ex.Message);
            }
        }

        private static string ResolveDiagnosticDirectory()
        {
            string primary = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LOG", "DIAGNOSTIC");
            string fallbackRoot = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string localFallback = Path.Combine(fallbackRoot, "Vision_Align", "LOG", "DIAGNOSTIC");
            string tempFallback = Path.Combine(Path.GetTempPath(), "Vision_Align", "LOG", "DIAGNOSTIC");
            string[] candidates = { primary, localFallback, tempFallback };
            Exception lastException = null;

            foreach (string candidate in candidates)
            {
                try
                {
                    Directory.CreateDirectory(candidate);
                    return candidate;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                }
            }

            throw new IOException("No writable diagnostic directory is available.", lastException);
        }

        private static void WriteAllTextAtomic(string path, string value)
        {
            string tempPath = path + ".tmp";
            File.WriteAllText(tempPath, value, new UTF8Encoding(false));

            try
            {
                if (File.Exists(path))
                    File.Replace(tempPath, path, null, true);
                else
                    File.Move(tempPath, path);
            }
            catch
            {
                File.Copy(tempPath, path, true);
                File.Delete(tempPath);
            }
        }

        private static void WriteAllTextSafe(string path, string value)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, value, new UTF8Encoding(false));
        }

        private static void AppendAllTextSafe(string path, string value)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            using (FileStream stream = new FileStream(
                path,
                FileMode.Append,
                FileAccess.Write,
                FileShare.ReadWrite))
            using (StreamWriter writer = new StreamWriter(stream, new UTF8Encoding(false)))
            {
                writer.Write(value);
            }
        }

        private static void WriteEmergencyLine(string message)
        {
            try
            {
                string directory = _diagnosticDirectory ?? ResolveDiagnosticDirectory();
                string path = Path.Combine(directory, "emergency.log");
                string line = "[" + DateTime.Now.ToString("O", CultureInfo.InvariantCulture) + "] " +
                    message + Environment.NewLine;
                AppendAllTextSafe(path, line);
            }
            catch
            {
                // There is no remaining in-process persistence option.
            }
        }

        private static string SafeGet(Func<string> getter)
        {
            try
            {
                return getter() ?? string.Empty;
            }
            catch (Exception ex)
            {
                return "unavailable (" + ex.GetType().Name + ": " + ex.Message + ")";
            }
        }

        private static string SafeToString(object value)
        {
            try
            {
                return value == null ? "null" : value.ToString();
            }
            catch
            {
                return "unprintable exception object";
            }
        }

        private static string MakeSafeFileName(string value)
        {
            string source = string.IsNullOrWhiteSpace(value) ? "unknown" : value;
            StringBuilder result = new StringBuilder(source.Length);

            foreach (char character in source)
            {
                if (char.IsLetterOrDigit(character) || character == '-' || character == '_')
                    result.Append(character);
                else
                    result.Append('_');
            }

            const int maximumLength = 48;
            return result.Length <= maximumLength
                ? result.ToString()
                : result.ToString(0, maximumLength);
        }

        private static void TryWriteMiniDump(string dumpPath)
        {
            try
            {
                using (Process process = Process.GetCurrentProcess())
                using (FileStream dumpFile = new FileStream(
                    dumpPath,
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.Read))
                {
                    MiniDumpType dumpType =
                        MiniDumpType.MiniDumpWithThreadInfo |
                        MiniDumpType.MiniDumpWithUnloadedModules |
                        MiniDumpType.MiniDumpWithIndirectlyReferencedMemory;

                    bool success = MiniDumpWriteDump(
                        process.Handle,
                        process.Id,
                        dumpFile.SafeFileHandle,
                        dumpType,
                        IntPtr.Zero,
                        IntPtr.Zero,
                        IntPtr.Zero);

                    if (!success)
                    {
                        int errorCode = Marshal.GetLastWin32Error();
                        WriteEmergencyLine("MiniDumpWriteDump failed. Win32Error=" + errorCode);
                    }
                }
            }
            catch (Exception ex)
            {
                WriteEmergencyLine("Mini dump creation failed: " + ex);
            }
        }

        [DllImport("Dbghelp.dll", SetLastError = true)]
        private static extern bool MiniDumpWriteDump(
            IntPtr processHandle,
            int processId,
            SafeFileHandle fileHandle,
            MiniDumpType dumpType,
            IntPtr exceptionParam,
            IntPtr userStreamParam,
            IntPtr callbackParam);

        [Flags]
        private enum MiniDumpType : uint
        {
            MiniDumpWithUnloadedModules = 0x00000020,
            MiniDumpWithIndirectlyReferencedMemory = 0x00000040,
            MiniDumpWithThreadInfo = 0x00001000
        }

        private sealed class WorkerState
        {
            public DateTime TimestampUtc { get; set; }
            public string State { get; set; }
        }
    }
}
