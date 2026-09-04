using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading;

namespace Vision_Align
{
    class ClsSerial
    {
        private SerialPort _serialPort = new SerialPort();
        public event EventHandler<string> DataReceived;
        public string strSerialReadData;
        public bool bSerialDataReading;

        public bool IsOpen
        {
            get => _serialPort.IsOpen;
        }

        public bool Open(string strPort, long lBaudRate, Parity ParityType, int nDataBits, StopBits StopBitsType)
        {
            try
            {
                _serialPort.PortName = strPort;

                _serialPort.BaudRate = (int)lBaudRate;
                _serialPort.Parity = ParityType;
                _serialPort.DataBits = (int)nDataBits;

                if (StopBitsType == StopBits.None        ) _serialPort.StopBits = StopBits.None;
                if (StopBitsType == StopBits.One         ) _serialPort.StopBits = StopBits.One;
                if (StopBitsType == StopBits.OnePointFive) _serialPort.StopBits = StopBits.OnePointFive;
                if (StopBitsType == StopBits.Two         ) _serialPort.StopBits = StopBits.Two;

                _serialPort.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(this.serialPort_DataReceived);

                _serialPort.Open();

                if (_serialPort.IsOpen == true) return true;
                else return false;
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return false;
            }
        }

        public void Close()
        {
            try{
                if (_serialPort.IsOpen)
                    _serialPort.Close();
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        public void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                Delay(50);

                string strMsg = _serialPort.ReadExisting();
                strSerialReadData = strMsg;
                bSerialDataReading = true;
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        private DateTime Delay(int MS)
        {
            try
            {
                DateTime ThisMoment = DateTime.Now;
                TimeSpan duration = new TimeSpan(0, 0, 0, 0, MS);
                DateTime AfterWards = ThisMoment.Add(duration);

                while (AfterWards >= ThisMoment)
                {
                    Thread.Sleep(10);
                    ThisMoment = DateTime.Now;
                }

                return DateTime.Now;
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return DateTime.Now;
            }
        }

        public void SendData(string data)
        {
            if (_serialPort.IsOpen)
            {
                string str = "";
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(data);
                for (int n = 0; n < buffer.Length; n++)
                {
                    str += Convert.ToString(buffer[n], 16);
                }
                _serialPort.Write(buffer, 0, buffer.Length);
            }
        }

    }
}
