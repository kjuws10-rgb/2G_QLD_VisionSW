using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using System.Text;

namespace Vision_Align
{
    static class Program
    {
        /// <summary>
        /// 해당 응용 프로그램의 주 진입점입니다.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //try
            //{
            AppDomain.CurrentDomain.UnhandledException += (sender, e)
            => FatalExceptionObject(e.ExceptionObject);

            Application.ThreadException += (sender, e)
            => FatalExceptionObject(e.Exception);


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
            Application.Run(new FormBase());
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

        static void FatalExceptionObject(object exceptionObject)
        {
            var huh = exceptionObject as Exception;
            if (huh == null)
            {
                huh = new NotSupportedException(
                "Unhandled exception doesn't derive from System.Exception: "
                + exceptionObject.ToString()
                );
            }

            DateTime objTime = DateTime.Now;
            string strCurrentDateTime = string.Format("\r\n{0:D04}/{1:D02}/{2:D02} {3:D02}:{4:D02}:{5:D02}.{6:D03}\r\n", objTime.Year, objTime.Month, objTime.Day, objTime.Hour, objTime.Minute, objTime.Second, objTime.Millisecond);
            string strExceptionData = strCurrentDateTime + CreateExceptionString(huh);

            Global.logger[LogType.EXCEPTION].Write(strExceptionData);
        }
    }
}
