using System;
using System.Diagnostics;
using System.Threading;

namespace Vision_Align
{
    public class ClsOmronThread
    {
        private const int ReconnectIntervalMs = 10_000;
        private const int AliveSignMs = 1000;

        private readonly ManualResetEvent _stopEvent = new ManualResetEvent(false);
        private readonly Stopwatch _reconnectStopwatch = new Stopwatch();
        private readonly Stopwatch _aliveSignStopwatch = new Stopwatch();
        private readonly Stopwatch _heartbeatStopwatch = new Stopwatch();

        private Thread _mainThread;
        private bool _started;

        public void Start()
        {
            if (_started)
                return;

            _started = true;
            _stopEvent.Reset();
            _reconnectStopwatch.Restart();
            _aliveSignStopwatch.Restart();
            _heartbeatStopwatch.Restart();

            _mainThread = new Thread(MainThreadRun)
            {
                IsBackground = true,
                Name = "VisionAlign.PLC"
            };
            _mainThread.Start();
        }

        public void Release()
        {
            _stopEvent.Set();

            Thread thread = _mainThread;
            if (thread != null && thread != Thread.CurrentThread && thread.IsAlive)
                thread.Join(3000);
        }

        private void MainThreadRun()
        {
            CrashDiagnostics.PulseWorker("PLC", "Started");

            while (!_stopEvent.WaitOne(10))
            {
                try
                {
                    PulseHeartbeat();

                    if (!Global.clsOmron.IsConnect)
                    {
                        Global.bConnectPLC = false;

                        if (_reconnectStopwatch.ElapsedMilliseconds >= ReconnectIntervalMs)
                        {
                            _reconnectStopwatch.Restart();
                            CrashDiagnostics.RecordActivity("PLC reconnect attempt");
                            Global.clsOmron.Open("192.168.240.80");
                        }

                        _stopEvent.WaitOne(20);
                        continue;
                    }

                    Global.bConnectPLC = true;
                    Global.clsOmron.UpdateIO();

                    if (_aliveSignStopwatch.ElapsedMilliseconds >= AliveSignMs)
                    {
                        _aliveSignStopwatch.Restart();
                        Global.inforPLC.OutAlive = !Global.inforPLC.OutAlive;
                    }
                }
                catch (Exception ex)
                {
                    Global.bConnectPLC = false;
                    CrashDiagnostics.ReportRecoverableException("Worker PLC polling", ex);
                    _stopEvent.WaitOne(500);
                }
            }

            CrashDiagnostics.PulseWorker("PLC", "Stopped");
        }

        private void PulseHeartbeat()
        {
            if (_heartbeatStopwatch.ElapsedMilliseconds < 1000)
                return;

            _heartbeatStopwatch.Restart();
            CrashDiagnostics.PulseWorker(
                "PLC",
                Global.bConnectPLC ? "Connected and polling" : "Disconnected/reconnecting");
        }
    }
}
