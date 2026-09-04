///아래 DEFINE 사용 시 ECAT : LIBTYPE_ECAT  /// 범용 : LIBTYPE_UNIT 로 변경 후 사용권장////////////////
#define LIBTYPE_ECAT

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vision_Align
{
    public class ClsMotion
    {
        public static bool m_bOpen = false;

        public bool m_bServo = false;
        public bool m_bHome = false;

        public bool m_bAlram = false;
        public bool m_bBasy = false;
        public bool m_bInPos = false;
        public bool m_bDirNegative = false;

        public bool m_bLimit_P = false;
        public bool m_bLimit_M = false;
        public bool m_bLimit_SW_P = false;
        public bool m_bLimit_SW_M = false;

        public double m_dCmdPos = 0;
        public double m_dActPos = 0;
        public double m_dErrPos = 0;


        static int m_nAxisCount;

        int m_nAxisNo = -1;

        public double dPos = .0;

        public bool IsConnect { get => m_bOpen; }

        public ClsMotion(CamInfo axis)
        {

            m_nAxisNo = (int)axis;
        }

        public static void OpenLib()
        {
            string strMotPaht = System.IO.Path.Combine(Global.strConfigPath, "Vision_Align.mot");
            CAXL.AxlClose();

            //++ AXL(AjineXtek Library)을 사용가능하게 하고 장착된 보드들을 초기화합니다.
            if (CAXL.AxlOpen(7) != (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS)
            {
                System.Windows.Forms.MessageBox.Show("Motion Intialize Fail..!!", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                m_bOpen = false;
                return;
            }

            //++ 지정한 Mot파일의 설정값들로 모션보드의 설정값들을 일괄변경 적용합니다.
            if (CAXM.AxmMotLoadParaAll(strMotPaht) != (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS)
            {
                System.Windows.Forms.MessageBox.Show("Motion Mot File Not Found", "Warning", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                m_bOpen = false;
                return;
            }
            m_bOpen = true;

            byte byteVersion = 0;

            int nBoardNo = 0, nModulePos = 0;
            uint uModuleID = 0;


            CAXL.AxlGetLibVersion(ref byteVersion);
            CAXM.AxmInfoGetAxisCount(ref m_nAxisCount);

            String strAxis = "";


            for (int nAxisNo = 0; nAxisNo < m_nAxisCount; nAxisNo++)
            {
                CAXM.AxmInfoGetAxis(nAxisNo, ref nBoardNo, ref nModulePos, ref uModuleID);
#if LIBTYPE_ECAT
                {
                    switch (uModuleID)
                    {
                        //++ 지정한 축의 정보를 반환합니다.
                        // [INFO] 여러개의 정보를 읽는 함수 사용시 불필요한 정보는 NULL(0)을 입력하면 됩니다.
                        case (uint)AXT_MODULE.AXT_SMC_4V04: strAxis = String.Format("{0:00}-[PCIB-QI4A]", nAxisNo); break;
                        case (uint)AXT_MODULE.AXT_SMC_2V04: strAxis = String.Format("{0:00}-[PCIB-QI2A]", nAxisNo); break;
                        case (uint)AXT_MODULE.AXT_SMC_R1V04: strAxis = String.Format("{0:00}-(RTEX-PM)", nAxisNo); break;
                        case (uint)AXT_MODULE.AXT_SMC_R1V04PM2Q: strAxis = String.Format("{0:00}-(RTEX-PM2Q)", nAxisNo); break;
                        case (uint)AXT_MODULE.AXT_SMC_R1V04PM2QE: strAxis = String.Format("{0:00}-(RTEX-PM2QE)", nAxisNo); break;
                        case (uint)AXT_MODULE.AXT_SMC_R1V04A4: strAxis = String.Format("{0:00}-[RTEX-A4N]", nAxisNo); break;
                        case (uint)AXT_MODULE.AXT_SMC_R1V04A5: strAxis = String.Format("{0:00}-[RTEX-A5N]", nAxisNo); break;
                        case (uint)AXT_MODULE.AXT_SMC_R1V04MLIISV: strAxis = String.Format("{0:00}-[MLII-SGDV]", nAxisNo); break;
                        case (uint)AXT_MODULE.AXT_SMC_R1V04MLIIPM: strAxis = String.Format("{0:00}-(MLII-PM)", nAxisNo); break;
                        case (uint)AXT_MODULE.AXT_SMC_R1V04MLIICL: strAxis = String.Format("{0:00}-[MLII-CSDL]", nAxisNo); break;
                        case (uint)AXT_MODULE.AXT_SMC_R1V04MLIICR: strAxis = String.Format("{0:00}-[MLII-CSDH]", nAxisNo); break;
                        case (uint)AXT_MODULE.AXT_SMC_R1V04SIIIHMIV: strAxis = String.Format("{0:00}-[SIIIH-MRJ4]", nAxisNo); break;
                        case (uint)AXT_MODULE.AXT_SMC_R1V04SIIIHMIV_R: strAxis = String.Format("{0:00}-[SIIIH-MRJ4]", nAxisNo); break;
                        case (uint)AXT_MODULE.AXT_SMC_R1V04PM4QSIIIH: strAxis = String.Format("{0:00}-[SIIIH-PM4Q]", nAxisNo); break;
                        case (uint)AXT_MODULE.AXT_SMC_R1V04PM2QSIIIH: strAxis = String.Format("{0:00}-[SIIIH-PM2Q]", nAxisNo); break;
                        case (uint)AXT_MODULE.AXT_SMC_R1V04MLIIIS7S: strAxis = String.Format("{0:00}-[MLIII-S7S]", nAxisNo); break;
                        case (uint)AXT_MODULE.AXT_SMC_R1V04MLIIIS7W: strAxis = String.Format("{0:00}-[MLIII-S7W]", nAxisNo); break;
                        case (uint)AXT_MODULE.AXT_SMC_R1V04MLIIIPM: strAxis = String.Format("{0:00}-[MLIII-PM]", nAxisNo); break;
                        case (uint)AXT_MODULE.AXT_ECAT_MOTION:
                            {
                                uint uModuleSubID = 0;
                                var szModuleName = new System.Text.StringBuilder(50);
                                var szModuleDesc = new System.Text.StringBuilder(80);
                                CAXM.AxmInfoGetAxisEx(nAxisNo, ref uModuleSubID, szModuleName, szModuleDesc);
                                strAxis = String.Format("{0:00}-[ECAT-{1}]", nAxisNo, szModuleName);
                                break;
                            }
                        default:
                            strAxis = String.Format("{0:00}-[Unknown]", nAxisNo); break;
                    }
                }
#else
                {
                    switch (uModuleID)
                    {
                        //++ 지정한 축의 정보를 반환합니다.
                        // [INFO] 여러개의 정보를 읽는 함수 사용시 불필요한 정보는 NULL(0)을 입력하면 됩니다.
                        case (uint)AXT_MODULE.AXT_SMC_4V04: strAxis = String.Format("{0:00}-[PCIB-QI4A]", i); break;
                        case (uint)AXT_MODULE.AXT_SMC_2V04: strAxis = String.Format("{0:00}-[PCIB-QI2A]", i); break;
                        case (uint)AXT_MODULE.AXT_SMC_R1V04: strAxis = String.Format("{0:00}-(RTEX-PM)", i); break;
                        case (uint)AXT_MODULE.AXT_SMC_R1V04PM2Q: strAxis = String.Format("{0:00}-(RTEX-PM2Q)", i); break;
                        case (uint)AXT_MODULE.AXT_SMC_R1V04PM2QE: strAxis = String.Format("{0:00}-(RTEX-PM2QE)", i); break;
                        case (uint)AXT_MODULE.AXT_SMC_R1V04A4: strAxis = String.Format("{0:00}-[RTEX-A4N]", i); break;
                        case (uint)AXT_MODULE.AXT_SMC_R1V04A5: strAxis = String.Format("{0:00}-[RTEX-A5N]", i); break;
                        case (uint)AXT_MODULE.AXT_SMC_R1V04MLIISV: strAxis = String.Format("{0:00}-[MLII-SGDV]", i); break;
                        case (uint)AXT_MODULE.AXT_SMC_R1V04MLIIPM: strAxis = String.Format("{0:00}-(MLII-PM)", i); break;
                        case (uint)AXT_MODULE.AXT_SMC_R1V04MLIICL: strAxis = String.Format("{0:00}-[MLII-CSDL]", i); break;
                        case (uint)AXT_MODULE.AXT_SMC_R1V04MLIICR: strAxis = String.Format("{0:00}-[MLII-CSDH]", i); break;
                        case (uint)AXT_MODULE.AXT_SMC_R1V04SIIIHMIV: strAxis = String.Format("{0:00}-[SIIIH-MRJ4]", i); break;
                        case (uint)AXT_MODULE.AXT_SMC_R1V04SIIIHMIV_R: strAxis = String.Format("{0:00}-[SIIIH-MRJ4]", i); break;
                        case (uint)AXT_MODULE.AXT_SMC_R1V04PM4QSIIIH: strAxis = String.Format("{0:00}-[SIIIH-PM4Q]", i); break;
                        case (uint)AXT_MODULE.AXT_SMC_R1V04PM2QSIIIH: strAxis = String.Format("{0:00}-[SIIIH-PM2Q]", i); break;
                        case (uint)AXT_MODULE.AXT_SMC_R1V04MLIIIS7S: strAxis = String.Format("{0:00}-[MLIII-S7S]", i); break;
                        case (uint)AXT_MODULE.AXT_SMC_R1V04MLIIIS7W: strAxis = String.Format("{0:00}-[MLIII-S7W]", i); break;
                        case (uint)AXT_MODULE.AXT_SMC_R1V04MLIIIPM: strAxis = String.Format("{0:00}-[MLIII-PM]", i); break;
                        default: strAxis = String.Format("{0:00}-[Unknown]", i); break;
                    }
                }
#endif
            }
        }

        public static void CloseLib()
        {
            CAXL.AxlClose();
            m_bOpen = false;
        }

        public void UpdateStatus()
        {
            uint iRetCode = 0;
            uint iStatus = 0;
            string strAlarmString = "";
            if (!m_bOpen) return;
            int nAxis = m_nAxisNo + 2;
            if ((uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS != (iRetCode = CAXM.AxmSignalIsServoOn(nAxis, ref iStatus))) { strAlarmString = "UpdateMotionStatus AxmSignalIsServoOn failed."; }
            m_bServo = Convert.ToBoolean(iStatus);

            if ((uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS != (iRetCode = CAXM.AxmStatusReadMechanical(nAxis, ref iStatus))) { strAlarmString = "UpdateMotionStatus AxmStatusReadMechanical failed."; }
            m_bLimit_P = Convert.ToBoolean(iStatus >> 0 & 0x1);
            m_bLimit_M = Convert.ToBoolean(iStatus >> 1 & 0x1);
            m_bAlram = Convert.ToBoolean(iStatus >> 4 & 0x1);
            m_bInPos = Convert.ToBoolean(iStatus >> 5 & 0x1);

            if ((uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS != (iRetCode = CAXM.AxmStatusReadStop(nAxis, ref iStatus))) { strAlarmString = "UpdateMotionStatus AxmStatusReadStop failed."; }
            m_bLimit_SW_P = Convert.ToBoolean(iStatus >> 4 & 0x1);
            m_bLimit_SW_M = Convert.ToBoolean(iStatus >> 5 & 0x1);

            if ((uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS != (iRetCode = CAXM.AxmStatusReadInMotion(nAxis, ref iStatus))) { strAlarmString = "UpdateMotionStatus AxmStatusReadMotion failed."; }
            m_bBasy = Convert.ToBoolean(iStatus >> 0 & 0x1);
            m_bDirNegative = Convert.ToBoolean(iStatus >> 11 & 0x1);

            uint duStepMain = 0, duStepSub = 0;
            if ((uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS != (iRetCode = CAXM.AxmHomeGetRate(nAxis, ref duStepMain, ref duStepSub))) { strAlarmString = "UpdateMotionStatus AxmHomeGetRate failed."; }
            m_bHome = duStepSub == 100 ? false : true;


            if ((uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS != (iRetCode = CAXM.AxmHomeReadSignal(nAxis, ref iStatus))) { strAlarmString = "UpdateMotionStatus AxmHomeGetRate failed."; }
            m_bHome = Convert.ToBoolean(iStatus);

            // Command, Actual, error position
            double dPosition = 0, dErrorPos = .0;
            if ((uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS != (iRetCode = CAXM.AxmStatusGetCmdPos(nAxis, ref dPosition))) { strAlarmString = "UpdateMotionStatus AxmStatusGetCmdPos failed."; }
            m_dCmdPos = dPosition;
            if ((uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS != (iRetCode = CAXM.AxmStatusGetActPos(nAxis, ref dPosition))) { strAlarmString = "UpdateMotionStatus AxmStatusGetActPos failed."; }
            m_dActPos = dPosition;
            if ((uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS != (iRetCode = CAXM.AxmStatusReadPosError(nAxis, ref dErrorPos))) { strAlarmString = "UpdateMotionStatus AxmStatusGetActPos failed."; }
            m_dErrPos = dErrorPos;

            Global.inforMotion.bHome[m_nAxisNo] = m_bHome;
            Global.inforMotion.bServo[m_nAxisNo] = m_bServo;
            Global.inforMotion.bLimitP[m_nAxisNo] = m_bLimit_P;
            Global.inforMotion.bLimitM[m_nAxisNo] = m_bLimit_M;
            Global.inforMotion.bInPos[m_nAxisNo] = m_bInPos;
            Global.inforMotion.bBasy[m_nAxisNo] = m_bBasy;
            Global.inforMotion.bAlram[m_nAxisNo] = m_bAlram;

            Global.inforMotion.dCmdPos[m_nAxisNo] = m_dCmdPos;
        }

        public bool ServoOnOff(bool bOn)
        {
            //bool bRet = false; 
            uint iRetCode = 0;
            string strAlarmString = "";
            string strOnOff = "";
            int nAxis = m_nAxisNo + 2;
            if (!m_bOpen) return false;

            strOnOff = (bOn ? "ON" : "OFF").ToString();

            if ((uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS != (iRetCode = CAXM.AxmSignalServoOn(nAxis, Convert.ToUInt32(bOn))))
            {
                strAlarmString = string.Format((CamInfo)nAxis + " ServoOn(" + strOnOff + ") failed.");
            }

            return true;
        }

        public bool Home()
        {
            bool bRet = false; uint iRetCode = 0; string strAlarmString = "";
            int iHmDir = 0; uint iHomeSignal = 0, iZphas = 0; double dHomeClrTime = 0, dHomeOffset = 0;
            double m_dPositionOffsetAll = 0;

            int nAxis = m_nAxisNo + 2;

            if (!m_bOpen) return false;

            if ((uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS != (iRetCode = CAXM.AxmHomeGetMethod(nAxis, ref iHmDir, ref iHomeSignal, ref iZphas, ref dHomeClrTime, ref dHomeOffset))) { strAlarmString = "AxmHomeGetMethod failed."; }
            if ((uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS != (iRetCode = CAXM.AxmHomeSetMethod(nAxis, iHmDir, iHomeSignal, iZphas, dHomeClrTime, m_dPositionOffsetAll))) { strAlarmString = "AxmHomeSetMethod failed."; }
            if ((uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS != (iRetCode = CAXM.AxmHomeSetStart(nAxis))) { strAlarmString = "AxmHomeSetStart failed."; }


            return true;
        }

        public bool Move(double dPosition, double dVelocity, double dAcceleration, double dDeceleration, double dSpeedRatio = 1.0)
        {
            uint iRetCode = 0;
            string strAlarmString = "";
            int nAxis = m_nAxisNo + 2;
            if (!m_bOpen) return false;

            if ((uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS != (iRetCode = CAXM.AxmMoveStartPos(nAxis, dPosition, dVelocity * dSpeedRatio, dAcceleration * dSpeedRatio, dDeceleration * dSpeedRatio)))
            {
                strAlarmString = string.Format((CamInfo)nAxis + "AxmMoveStartPos failed."); ;
            }

            return true;
        }



        public void JogMove(int nDir, double dVelocity = 1000, double dAcceleration = 1000, double dDeceleration = 1000)
        {
            double dDir = 1;
            uint iRetCode = 0;
            string strAlarmString = "";
            if (!m_bOpen) return;
            int nAxis = m_nAxisNo + 2;
            // Plus : 0 , Minus : 1
            if (nDir == 0) dDir = 1;
            else dDir = -1;

            if ((uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS != (iRetCode = CAXM.AxmMoveVel(nAxis, dVelocity * dDir, dAcceleration, dDeceleration)))
            {
                strAlarmString = string.Format((CamInfo)nAxis + "AxmMoveVel failed.");
            }
        }

        public bool SlowStop()
        {
            //bool bRet = false;i no
            uint iRetCode = 0;
            string strAlarmString = "";
            int nAxis = m_nAxisNo + 2;
            if (!m_bOpen) return false;

            if ((uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS != (iRetCode = CAXM.AxmMoveSStop(nAxis)))
            {
                strAlarmString = string.Format((CamInfo)nAxis + "AxmMoveSStop failed.");
            }

            return true;
        }

        public bool EmoStop()
        {
            //bool bRet = false;
            uint iRetCode = 0;
            string strAlarmString = "";
            int nAxis = m_nAxisNo + 2;
            if (!m_bOpen) return false;

            if ((uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS != (iRetCode = CAXM.AxmMoveEStop(nAxis)))
            {
                strAlarmString = string.Format((CamInfo)nAxis + "AxmMoveEStop failed.");
            }

            return true;
        }

    }
}
