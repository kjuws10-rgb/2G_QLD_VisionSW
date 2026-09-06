using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoAlgorithm;
using System.Threading;
using System.Diagnostics;

namespace Vision_Align
{
    internal class ClsUvwStage // OST
    {
        public double[] CurrentPos = new double[3];     //실시간 현재 위치 (x, y, t)
        public bool IsInPosition = false;         //실시간 InPosition 상황

        private const int DEFAULT_TIMEOUT_MS = 30000;

        public bool Move(double x, double y, double t)
        {
            string msg;
            return Move(x, y, t, out msg);
        }

        public bool Move(double x, double y, double t, out string msg)
        {
            bool stageEnable = Global.inforPLC.InStageEnable;

            if (!stageEnable)
            {
                msg = "PLC STAGE Enable Off";
                return false;
            }

            try
            {

                // 1) (x,y,t) -> (u,v,w) 변환
                double dX1, dX2, dY1, dY2;
                VisionAlgorithm.StageCal(x, y, t, out dX1, out dX2, out dY1, out dY2);

                // 결과값 로그/CSV용으로 저장해두고 싶으면 여기서 채워도 됨
                Global.inforResult.dMove_UVW_X1 = dX1;
                Global.inforResult.dMove_UVW_X2 = dX2;
                Global.inforResult.dMove_UVW_Y1 = dY1;
                Global.inforResult.dMove_UVW_Y2 = dY2;


                // AO_Vision에 목표값 쓰기
                int[] ao = (int[])Global.clsOmron.AO_Word.Clone();
                Global.inforPLC.OutStageTargetU = (float)dX1 + Global.PreConfig_Param.dUVW_OffsetX1;
                Global.inforPLC.OutStageTargetV = (float)dY1 + Global.PreConfig_Param.dUVW_OffsetY1;
                Global.inforPLC.OutStageTargetW = (float)dX2 + Global.PreConfig_Param.dUVW_OffsetX2;

                if (dX1 > Global.PreConfig_Param.dUVW_LimitPosX1 || dX1 < Global.PreConfig_Param.dUVW_LimitNegX1)
                {
                    msg = "X1 Software Limit Failed";
                    return false;
                }
                if (dX2 > Global.PreConfig_Param.dUVW_LimitPosX2 || dX2 < Global.PreConfig_Param.dUVW_LimitNegX2)
                {
                    msg = "X2 Software Limit Failed";
                    return false;
                }
                if (dY1 > Global.PreConfig_Param.dUVW_LimitPosY1 || dY1 < Global.PreConfig_Param.dUVW_LimitNegY1)
                {
                    msg = "Y1 Software Limit Failed";
                    return false;
                }

                Thread.Sleep(500);

                // UVW_MOVE_REQ ON (DO bit 24)
                Global.inforPLC.OutUvwMoveRequest = true;

                // PLC 응답/이동 완료 대기
                if(WaitMoveDoneAndInPos(dX1, dY1, dX2, Global.PreConfig_Param.nInPositionGap, DEFAULT_TIMEOUT_MS))
                {
                    msg = "OK";
                    return true;
                }
                else
                {
                    msg = "UVW Move done wait failed";
                    return true;
                }
            }
            catch (Exception ex)
            {
                Global.logger[LogType.EXCEPTION].Write("ClsUvwStage.Move Exception: " + ex);
                // 안전하게 요청 비트 OFF
                try { Global.inforPLC.OutUvwMoveRequest = false; } catch { }
                msg = "Move Exception";
                return false;
            }
        }

        private bool WaitMoveDoneAndInPos(double targetU, double targetV, double targetW, int inPosGap, int timeoutMs)
        {
            var start = Environment.TickCount;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Restart();
            bool reply = false;
            bool ok = false;
            bool ng = false;
            while (stopwatch.ElapsedMilliseconds < timeoutMs)
            {
                Thread.Sleep(10);

                // 현재 위치 업데이트
                CurrentPos[0] = Global.inforPLC.InStageCurrentU + Global.PreConfig_Param.dUVW_OffsetX1;
                CurrentPos[1] = Global.inforPLC.InStageCurrentV + Global.PreConfig_Param.dUVW_OffsetY1;
                CurrentPos[2] = Global.inforPLC.InStageCurrentW + Global.PreConfig_Param.dUVW_OffsetX2;

                // PLC 핸드쉐이크 응답 비트 확인 
                if (Global.inforPLC.InUvwMoveReply )
                {
                    reply = Global.inforPLC.InUvwMoveReply;
                    ok = Global.inforPLC.InUvwMoveOk;
                    ng = Global.inforPLC.InUvwMoveNg;
                }

                // InPosition (Gap 기반)
                IsInPosition =
                    Math.Abs(CurrentPos[0] - targetU) <= inPosGap &&
                    Math.Abs(CurrentPos[1] - targetV) <= inPosGap &&
                    Math.Abs(CurrentPos[2] - targetW) <= inPosGap;

                if (reply)
                {
                    // 요청 비트 OFF (정리)
                    Global.inforPLC.OutUvwMoveRequest = false;

                    if (ng) return false;

                    // ok는 떴는데 inpos가 아직이면 조금 더 기다려봄
                    if (ok && IsInPosition) return true;
                }

                Thread.Sleep(10);
            }

            // timeout -> 요청 비트 OFF
            Global.inforPLC.OutUvwMoveRequest = false;
            return false;
        }

    }

}
