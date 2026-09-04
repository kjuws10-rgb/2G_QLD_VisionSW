using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Vision_Align
{
    public class ClsOmronThread
    {
        Thread m_MainThread = null;
        private const int RECONNECT_INTERVAL_MS = 10_000;
        private const int ALIVE_SIGN_MS = 1000;
        Stopwatch _ReconnectStopwatch = new Stopwatch();
        Stopwatch _AliveSignStopwatch = new Stopwatch();


        public ClsOmronThread()
        {
            m_MainThread = new Thread(MainThreadRunAsync);
            m_MainThread.Start();
            _ReconnectStopwatch.Restart();
            _AliveSignStopwatch.Restart();
        }

        public void Release()
        {
            if (m_MainThread != null)
                m_MainThread.Abort();

        }


        public void MainThreadRunAsync()
        {
            int now = 0;
            while (true)
            {
                Thread.Sleep(10);
                //OST Omron 연결 실패시 자동으로 재연결 10초에 한번씩 재시도
                //OST 실시간 시스템 상태에 따라 Alarm, Auto, Alive 등을 보냄

                if (!Global.clsOmron.IsConnect)
                {
                    Global.bConnectPLC = false;

                    // 2) 10초마다 재연결 시도
                    if (_ReconnectStopwatch.ElapsedMilliseconds >= RECONNECT_INTERVAL_MS)
                    {
                        _ReconnectStopwatch.Restart();

                        try
                        {
                            Global.clsOmron.Open("192.168.240.80");
                        }
                        catch (Exception ex)
                        {

                        }
                    }

                    Thread.Sleep(20);
                    continue;
                }

                // 여기까지 왔으면 연결 OK
                Global.bConnectPLC = true;

                // 실시간 Omron 값 Read 하여 Global.clsOmron 값 반영
                Global.clsOmron.UpdateIO();

                
                if (_AliveSignStopwatch.ElapsedMilliseconds >= ALIVE_SIGN_MS)
                {
                    _AliveSignStopwatch.Restart();
                    Global.inforPLC.OutAlive = !Global.inforPLC.OutAlive;
                }


            }
        }
    }
}
