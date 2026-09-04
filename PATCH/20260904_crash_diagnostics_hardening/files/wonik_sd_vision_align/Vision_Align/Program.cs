using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Vision_Align
{
    static class Program
    {
        private const string SingleInstanceMutexName = "Vision_Align.SingleInstance.2G_QLD";
        private static int _uiExceptionNoticeShown;

        /// <summary>
        /// 해당 응용 프로그램의 주 진입점입니다.
        /// </summary>
        [STAThread]
        static void Main()
        {
            bool ownsMutex;

            using (Mutex singleInstanceMutex = new Mutex(true, SingleInstanceMutexName, out ownsMutex))
            {
                if (!ownsMutex)
                {
                    MessageBox.Show(
                        "Vision_Align is already running.",
                        "Vision_Align",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                RegisterExceptionHandlers();

                try
                {
                    CrashDiagnostics.StartSession();
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new FormBase());
                }
                catch (Exception ex)
                {
                    string reportPath = CrashDiagnostics.ReportException("Program.Main", ex, true);
                    TryShowFatalStartupMessage(reportPath);
                }
                finally
                {
                    CrashDiagnostics.Stop();

                    try
                    {
                        singleInstanceMutex.ReleaseMutex();
                    }
                    catch (ApplicationException)
                    {
                        // The mutex was not owned anymore. Process shutdown can continue.
                    }
                }
            }
        }

        private static void RegisterExceptionHandlers()
        {
            // Force WinForms UI exceptions through Application.ThreadException.
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

            Application.ThreadException += OnThreadException;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
            Application.ApplicationExit += OnApplicationExit;
        }

        private static void OnThreadException(object sender, ThreadExceptionEventArgs e)
        {
            string reportPath = CrashDiagnostics.ReportException("Windows Forms UI thread", e.Exception, false);
            TryEnterSafeState("UI thread exception");

            // A recurring UI timer exception could otherwise display an endless message loop.
            if (Interlocked.CompareExchange(ref _uiExceptionNoticeShown, 1, 0) == 0)
            {
                try
                {
                    MessageBox.Show(
                        "An unexpected error was contained and recorded.\r\n" +
                        "Please stop automatic operation and contact the software engineer.\r\n\r\n" +
                        reportPath,
                        "Vision_Align error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
                catch
                {
                    // Never let error notification hide the original exception.
                }
            }
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            CrashDiagnostics.ReportException(
                "AppDomain unhandled exception",
                e.ExceptionObject,
                e.IsTerminating);
        }

        private static void OnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            CrashDiagnostics.ReportException("Unobserved task exception", e.Exception, false);
            e.SetObserved();
        }

        private static void OnApplicationExit(object sender, EventArgs e)
        {
            CrashDiagnostics.MarkCleanShutdown("ApplicationExit");
        }

        private static void TryEnterSafeState(string reason)
        {
            try
            {
                Global.bAutoMode = false;
                Global.inforPLC.ClearOutput();
                Global.inforPLC.OutReady = false;
                Global.inforPLC.OutBusy = false;
                Global.m_AlarmCode = AlarmCode.Unknown;
                CrashDiagnostics.RecordActivity(reason + "; automatic operation stopped and outputs cleared");
            }
            catch (Exception safeStateException)
            {
                CrashDiagnostics.ReportRecoverableException(
                    "Safe-state transition after " + reason,
                    safeStateException);
            }
        }

        private static void TryShowFatalStartupMessage(string reportPath)
        {
            try
            {
                MessageBox.Show(
                    "Vision_Align could not start. A diagnostic report was saved.\r\n\r\n" + reportPath,
                    "Vision_Align startup error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch
            {
                // The diagnostic file is the final fallback when the UI cannot be shown.
            }
        }
    }
}
