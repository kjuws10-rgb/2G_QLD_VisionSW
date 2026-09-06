using System;
using System.Threading;
using System.Windows.Forms;

namespace Vision_Align
{
    public static class SplashManager
    {
        private static FormSplash s_splash;
        private static Thread s_thread;

        public static void Show()
        {
            s_thread = new Thread(() =>
            {
                s_splash = new FormSplash();
                Application.Run(s_splash);
            });
            s_thread.SetApartmentState(ApartmentState.STA);
            s_thread.IsBackground = true;
            s_thread.Start();

            // Wait until handle is created so BeginInvoke can be used
            while (s_thread.IsAlive && (s_splash == null || !s_splash.IsHandleCreated))
            {
                Thread.Sleep(10);
            }
        }

        public static void UpdateStatus(string strText)
        {
            try
            {
                if (s_splash != null && s_splash.IsHandleCreated && !s_splash.IsDisposed)
                {
                    s_splash.BeginInvoke(new Action(() => s_splash.SetStatus(strText)));
                }
            }
            catch { }
        }

        public static void Close()
        {
            try
            {
                if (s_splash != null && s_splash.IsHandleCreated && !s_splash.IsDisposed)
                {
                    s_splash.BeginInvoke(new Action(() => s_splash.Close()));
                }

                if (s_thread != null && s_thread.IsAlive)
                {
                    s_thread.Join(3000);
                }
            }
            catch { }
            finally
            {
                s_splash = null;
                s_thread = null;
            }
        }
    }
}
