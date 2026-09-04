using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vision_Align
{
    public class ClsLight
    {
        ClsSerial m_clsSerial = new ClsSerial();

        public bool Open(int nPortName, int nBaudRate)
        {
            // baud       : 9600
            // data bit   : 8
            // parity bit : none
            // stop bit   : 1
            string strPortName = string.Format("COM" + (nPortName + 1).ToString());

            m_clsSerial.Open(strPortName, nBaudRate, System.IO.Ports.Parity.None, 8, System.IO.Ports.StopBits.One);
            return true;
        }

        public void Close()
        {
            m_clsSerial.Close();
        }

        public bool IsOpen()
        {
            return m_clsSerial.IsOpen;
        }

        public void LightOnOff(bool bOn, int nCh)
        {
            // 프로토콜
            // stx : byte   : 0x02 
            // ch  : string : "0" ~ "3"
            // cmd : string : "o" / open , "f" / off
            // etx : byte   : 0x03 

            if (!m_clsSerial.IsOpen) return;
            
            byte[] byteTX = new byte[2];
            string strMessage = "";

            byteTX[0] = 0x02;
            byteTX[1] = 0x03;

            strMessage = Encoding.Default.GetString(byteTX, 0, 1);
            
            strMessage += string.Format("{0}{1}", nCh, bOn? "o" :"f");
            strMessage += Encoding.Default.GetString(byteTX, 1, 1);

            m_clsSerial.SendData(strMessage);
        }

        public void LightValue(int nCh, int nValue)
        {
            // 프로토콜
            // stx  : byte   : 0x02 
            // ch   : string : "0" ~ "3"
            // cmd  : string : "w" write
            // data : btye   : "0000" ~ "0512"
            // etx  : byte   : 0x03 

            if (!m_clsSerial.IsOpen) return;

            byte[] byteTX = new byte[2];
            string strMessage = "";

            byteTX[0] = 0x02;
            byteTX[1] = 0x03;

            strMessage = Encoding.Default.GetString(byteTX, 0, 1);
            strMessage += string.Format("{0}w{1:0000}",nCh, nValue);
            strMessage += Encoding.Default.GetString(byteTX, 1, 1);

            m_clsSerial.SendData(strMessage);
        }


    }
}
