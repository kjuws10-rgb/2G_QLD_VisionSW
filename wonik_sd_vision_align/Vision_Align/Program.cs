using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Text;
using System.Threading.Tasks;

namespace Vision_Align
{
    static class Program
    {
        private static readonly object CrashLogSync = new object();

        /// <summary>
        /// 해당 응용 프로그램의 주 진입점입니다.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
                WriteExceptionLog("AppDomain.UnhandledException", e.ExceptionObject as Exception, e.IsTerminating);
            Application.ThreadException += (sender, e) =>
                WriteExceptionLog("Application.ThreadException", e.Exception, false);
            TaskScheduler.UnobservedTaskException += (sender, e) =>
            {
                WriteExceptionLog("TaskScheduler.UnobservedTaskException", e.Exception, false);
                e.SetObserved();
            };


            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Process[] objProcess = Process.GetProcessesByName("Vision_Align");
            Process objCurrentProcess = Process.GetCurrentProcess();
            if (objProcess.Length > 1)
            {
                MessageBox.Show(String.Format("Vision_Align process already running.\nKill \"Vision_Align.exe\" in the TaskManager"));
                objCurrentProcess.Kill();
                return;
            }
            try
            {
                SplashManager.Show();
                Application.Run(new FormBase());
            }
            catch (Exception ex)
            {
                WriteExceptionLog("Program.Main", ex, true);
            }
        }

        public static string CreateExceptionString(Exception e)
        {
            StringBuilder sb = new StringBuilder();
            CreateExceptionString(sb, e, string.Empty);

            return sb.ToString();
        }

        private static void CreateExceptionString(StringBuilder sb, Exception e, string indent)
        {
            if (indent == null)
            {
                indent = string.Empty;
            }
            else if (indent.Length > 0)
            {
                sb.AppendFormat("{0}Inner ", indent);
            }

            sb.AppendFormat("Exception Found:\n{0}Type: {1}", indent, e.GetType().FullName);
            sb.AppendFormat("\n{0}Message: {1}", indent, e.Message);
            sb.AppendFormat("\n{0}Source: {1}", indent, e.Source);
            sb.AppendFormat("\n{0}Stacktrace: {1}", indent, e.StackTrace);

            if (e.InnerException != null)
            {
                sb.Append("\n");
                CreateExceptionString(sb, e.InnerException, indent + "  ");
            }
        }

        internal static void WriteExceptionLog(string source, Exception exception, bool terminating)
        {
            if (exception == null)
            {
                exception = new NotSupportedException("Unhandled exception object was null or was not a System.Exception.");
            }

            DateTime objTime = DateTime.Now;
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("\r\n{0:yyyy/MM/dd HH:mm:ss.fff}\r\n", objTime);
            sb.AppendFormat("Source: {0}\r\n", string.IsNullOrWhiteSpace(source) ? "Unknown" : source);
            sb.AppendFormat("Terminating: {0}\r\n", terminating);
            sb.AppendFormat("ProcessId: {0}\r\n", Process.GetCurrentProcess().Id);
            sb.AppendFormat("Thread: {0} (ManagedId={1})\r\n",
                Thread.CurrentThread.Name ?? "unnamed", Thread.CurrentThread.ManagedThreadId);
            sb.AppendFormat("BaseDirectory: {0}\r\n", AppDomain.CurrentDomain.BaseDirectory);
            sb.AppendFormat("CurrentDirectory: {0}\r\n", Environment.CurrentDirectory);

            try
            {
                sb.AppendFormat("AutoMode: {0}\r\n", Global.bAutoMode);
                sb.AppendFormat("AutoStep: {0}\r\n", Global.clsAutoThread == null ? "NotCreated" : Global.clsAutoThread.CurrentStep.ToString());
                sb.AppendFormat("Alarm: {0}\r\n", Global.clsAlarmManager == null ? "NotCreated" : Global.clsAlarmManager.CurrentAlarm.ToString());
                sb.AppendFormat("Connections: PLC={0}, Camera={1}, Motion={2}, Light={3}\r\n",
                    Global.bConnectPLC, Global.bConnectCam, Global.bConnectAxisZ, Global.bConnectLight);
                sb.AppendFormat("PLCRequests: PreAlign={0}, Contact={1}, Model={2}, Calibration={3}\r\n",
                    Global.inforPLC.InPreAlignRequest,
                    Global.inforPLC.InContactRequest,
                    Global.inforPLC.InMaskModelRequest,
                    Global.inforPLC.InCalibrationRequest);
            }
            catch (Exception stateException)
            {
                sb.AppendFormat("StateCaptureError: {0}\r\n", stateException.Message);
            }

            sb.Append(CreateExceptionString(exception));
            string strExceptionData = sb.ToString();

            bool primaryWritten = TryWriteCrashFile(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LOG", "CRASH"),
                objTime,
                strExceptionData);

            if (!primaryWritten)
            {
                TryWriteCrashFile(
                    Path.Combine(Path.GetTempPath(), "Vision_Align", "CRASH"),
                    objTime,
                    strExceptionData);
            }

            try
            {
                if (Global.logger != null && Global.logger.ContainsKey(LogType.EXCEPTION))
                    Global.logger[LogType.EXCEPTION].Write(strExceptionData);
            }
            catch
            {
                // The independent crash file above must remain the final fallback.
            }
        }

        private static bool TryWriteCrashFile(string directory, DateTime time, string data)
        {
            try
            {
                lock (CrashLogSync)
                {
                    Directory.CreateDirectory(directory);
                    string path = Path.Combine(directory, time.ToString("yyyyMMdd") + ".log");
                    using (FileStream fs = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                    using (StreamWriter writer = new StreamWriter(fs, new UTF8Encoding(false)))
                    {
                        writer.WriteLine(data);
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
