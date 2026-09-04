using Newtonsoft.Json.Linq;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using uEye;
using static System.Net.Mime.MediaTypeNames;

namespace Vision_Align
{
    public enum SeqStep
    {
        WAIT = 0,
        PRE_ALIGN = 100,
        CONTACT_ALIGN,
        MODEL_CHANGE,
        CALIBRATION_MODE,
        ALARM,
        CLEAR,
        MAX,
    }

    public class ClsAutoThread // OST
    {
        Thread m_MainThread = null;
        SeqStep m_Step = SeqStep.WAIT;
        SeqStep m_StepPrev = SeqStep.MAX;

        public event EventHandler<string> ProcessStatusMsg;


        public SeqStep CurrentStep
        {
            get { return m_Step; }
        }

        public ClsAutoThread()
        {
            m_MainThread = new Thread(MainThreadRunAsync);
            m_MainThread.Start();
        }

        public void Release()
        {
            if (m_MainThread != null)
                m_MainThread.Abort();

        }
        private bool GrabAllCamera()
        {
            for (int cam = 0; cam < (int)CamInfo.MAX; cam++)
            {
                if (!Global.dicClsCam.TryGetValue((CamInfo)cam, out var camObj))
                {
                    SetProcessMsg(m_Step, $"    {cam.ToString()} Grab Failed : Can not read Camera Object");
                    return false;
                }

                if (!camObj.Grab())
                {
                    SetProcessMsg(m_Step, $"    {cam.ToString()} Grab Failed : Failed to grab");
                    return false;
                }

                Global.inforResult.dFocus[cam] = Global.clsAlgorithm[cam].MeasureSharpness();
            }
            return true;
        }

        public void MainThreadRunAsync() // OST
        {
            bool success = false;
            int startTime = Environment.TickCount;
            const int REQ_OFF_TIMEOUT_MS = 10000;
            DateTime ts = DateTime.Now;
            while (true)
            {
                Thread.Sleep(10);

                //AUTO 모드인지 확인.
                if (!Global.bAutoMode)
                {
                    Global.inforPLC.OutReady = false;
                    m_Step = SeqStep.WAIT;
                    Thread.Sleep(20);
                    continue;
                }

                if (m_Step != m_StepPrev)
                {
                    SetProcessMsg(m_Step, "Start");
                    m_StepPrev = m_Step;
                }
                switch (m_Step)
                {
                    case SeqStep.WAIT:
                        {
                            // 각종 REQ 확인
                            if (Global.bAlarmClearReq || (Global.m_AlarmCode != AlarmCode.None && (  Global.inforPLC.InPreAlignRequest || Global.inforPLC.InContactRequest || Global.inforPLC.InMaskModelRequest || Global.inforPLC.InCalibrationRequest))) //각종 PMC REQ 에도 자동 Alarm Clear 
                            {
                                m_Step = SeqStep.CLEAR;
                                break;
                            }

                            if (Global.m_AlarmCode != AlarmCode.None)
                            {
                                if (Global.inforPLC.OutAlarmCode != (int)Global.m_AlarmCode)
                                {
                                    m_Step = SeqStep.ALARM;
                                    break;
                                }
                            }

                            //설비 상태 READY 상태로 보고.
                            Global.inforPLC.OutReady = true;
                            Global.inforPLC.OutBusy = false;


                            if (Global.inforPLC.InPreAlignRequest)
                                m_Step = SeqStep.PRE_ALIGN;
                            else if (Global.inforPLC.InContactRequest)
                                m_Step = SeqStep.CONTACT_ALIGN;
                            else if (Global.inforPLC.InMaskModelRequest)
                                m_Step = SeqStep.MODEL_CHANGE;
                            else if (Global.inforPLC.InCalibrationRequest)
                                m_Step = SeqStep.CALIBRATION_MODE;

                            if (Global.PreConfig_Param.bAlways_Live)
                            {
                                ///     이미지 취득
                                GrabAllCamera();
                            }
                        }
                        break;
                    case SeqStep.PRE_ALIGN:
                        {
                            Global.inforPLC.OutBusy = true;
                            Global.inforPLC.OutPrealignReply = true;

                            success = false;
                            // Teaching Data Set
                            if (!SetTeachingData(1))
                            {
                                Global.m_AlarmCode = AlarmCode.CameraMotionCommError;
                                m_Step = SeqStep.ALARM;
                                break;
                            }

                            ///     GetUvwTargetPos
                            double preX = 0, preY = 0, preT = 0;
                            int imageCount = Global.Calibration_Param.AverageImageCount;
                            double mX1 = 0, mY1 = 0, mScore1 = 0, mScale1 = 0;
                            double mX2 = 0, mY2 = 0, mScore2 = 0, mScale2 = 0;
                            SetProcessMsg(m_Step, "    Image Grab && Measurement");
                            for (int i = 0; i < imageCount; i++)
                            {
                                double mX1Temp = 0, mY1Temp = 0, mScore1Temp = 0, mScale1Temp = 0;
                                double mX2Temp = 0, mY2Temp = 0, mScore2Temp = 0, mScale2Temp = 0;                                ///     이미지 취득
                                if (!GrabAllCamera())
                                    continue;

                                Global.clsVision.FindMaskHole(CamInfo.CAM_1, out mX1Temp, out mY1Temp, out mScore1Temp, out mScale1Temp);
                                Global.clsVision.FindMaskHole(CamInfo.CAM_2, out mX2Temp, out mY2Temp, out mScore2Temp, out mScale2Temp);

                                mX1 += mX1Temp;
                                mY1 += mY1Temp;
                                if (mScore1Temp < mScore1) //Score는 최소값 적용.
                                    mScore1 = mScore1Temp;
                                mScale1 += mScale1Temp;

                                mX2 += mX2Temp;
                                mY2 += mY2Temp;
                                if (mScore2Temp < mScore2) //Score는 최소값 적용.
                                    mScore2 = mScore2Temp;
                                mScale2 += mScale2Temp;
                            }
                            mX1 /= imageCount;
                            mY1 /= imageCount;
                            mScale1 /= imageCount;
                            mX2 /= imageCount;
                            mY2 /= imageCount;
                            mScale2 /= imageCount;

                            if (Global.clsVision.GetUvwTargetPos(out preX, out preY, out preT,  mX1, mY1, mScore1, mScale1,
                                                                                                mX2, mY2, mScore2, mScale2))
                            {
                                if (Global.inforResult.strJudg == "OK")
                                    success = true;
                                else
                                    success = false;
                            }
                            else
                            {
                                //Glass or Mask 인식 실패 알람 처리
                                success = false;
                            }

                            SetProcessMsg(m_Step, $"    Measurement Result : {Global.inforResult.strJudg}");

                            ///     결과 저장
                            ts = DateTime.Now;
                            Global.clsFolderThread.RequestSave(ts, "PRE_ALIGN");
                            SetProcessMsg(m_Step, $"    Result Saved");

                            /// 결과 전송
                            WritePreAlignAoResult();

                            // DO로 결과 핸드셰이크 (REQ=16에 대한 응답 17/18/19) 
                            Global.inforPLC.OutPrealignOk = success;
                            Global.inforPLC.OutPrealignNg = !success;

                            // PLC가 PREALIGN_REQ(16) 내릴 때까지 대기
                            Stopwatch stopwatch = new Stopwatch();
                            stopwatch.Restart();
                            while (stopwatch.ElapsedMilliseconds < REQ_OFF_TIMEOUT_MS)
                            {
                                if (!Global.inforPLC.InPreAlignRequest)
                                    break;
                                Thread.Sleep(5);
                            }

                            // 비트 정리
                            Global.inforPLC.OutPrealignReply = false;
                            Global.inforPLC.OutPrealignOk = false;
                            Global.inforPLC.OutPrealignNg = false;

                            if (Global.inforResult.strJudg == "XY Align NG" || Global.inforResult.strJudg == "T Align NG") //XY,T NG 일때만 구동
                            {
                                if (Global.inforPLC.InPmcCurrentRetry <= Global.inforPLC.InPmcRetry) //현재 Retry 횟수가 설정된 횟수보다 작거나 같으면 UVW 구동
                                {
                                    /// UVW 이동
                                    SetProcessMsg(m_Step, $"    UVW Stage Move Start");
                                    SetProcessMsg(m_Step, $"    X={preX.ToString("F3")}, Y={preY.ToString("F3")}, T={preT.ToString("F3")}");
                                    string moveMsg;
                                    //if (!Global.clsUvwStage.Move(preX + Global.inforResult.ContactErrorX, preY + Global.inforResult.ContactErrorY, preT + Global.inforResult.ContactErrorT, out moveMsg))
                                    if (!Global.clsUvwStage.Move(preX , preY , preT , out moveMsg))
                                    {
                                        m_Step = SeqStep.ALARM;
                                        Global.m_AlarmCode = AlarmCode.UvwStageMoveError;
                                        SetProcessMsg(m_Step, $"    UVW Stage Move Failed : {moveMsg}");
                                        continue;
                                    }
                                    SetProcessMsg(m_Step, "    UVW Stage Move End");
                                }
                            }
                            else if (Global.inforResult.strJudg.Contains("Mask Score NG"))
                            {
                                
                                /// UVW 이동
                                SetProcessMsg(m_Step, $"    UVW Stage Move Start (Calibration Start Pos)");
                                SetProcessMsg(m_Step, $"    X={Global.Calibration_Param.StartPositionX.ToString("F3")}, Y={Global.Calibration_Param.StartPositionY.ToString("F3")}, T={Global.Calibration_Param.StartPositionAngle.ToString("F3")}");
                                string moveMsg;
                                if (!Global.clsUvwStage.Move(Global.Calibration_Param.StartPositionX, Global.Calibration_Param.StartPositionY, Global.Calibration_Param.StartPositionAngle, out moveMsg))
                                {
                                    m_Step = SeqStep.ALARM;
                                    Global.m_AlarmCode = AlarmCode.UvwStageMoveError;
                                    SetProcessMsg(m_Step, $"    UVW Stage Move Failed : {moveMsg}");
                                    continue;
                                }
                                SetProcessMsg(m_Step, "    UVW Stage Move End");
                            }

                            m_Step = SeqStep.WAIT;
                        }
                        break;
                    case SeqStep.CONTACT_ALIGN:
                        {
                            Global.inforPLC.OutBusy = true;
                            Global.inforPLC.OutContactReply = true;

                            success = false;
                            // Z축 ContactAlign 위치로 이동
                            if(!SetTeachingData(2))
                            {
                                Global.m_AlarmCode = AlarmCode.CameraMotionCommError;
                                m_Step = SeqStep.ALARM;
                                break;
                            }

                            ///     이미지 취득
                            if (!GrabAllCamera())
                                continue;
                            SetProcessMsg(m_Step, "    Image Grab");

                            ///     GetUvwTargetPos
                            double contactX, contactY, contactT;
                            SetProcessMsg(m_Step, "    Measurement...");
                            if (Global.clsVision.GetUvwTargetPos(out contactX, out contactY, out contactT))
                            {
                                if (Global.inforResult.strJudg == "OK")
                                    success = true;
                                else
                                    success = false;
                            }
                            else
                                success = false;

                            if(success)
                                SetProcessMsg(m_Step, $"    Measurement Result : OK (X={contactX.ToString("F3")}, Y={contactY.ToString("F3")}, T={contactT.ToString("F3")}");
                            else
                                SetProcessMsg(m_Step, $"    Measurement Result : NG (X={contactX.ToString("F3")}, Y={contactY.ToString("F3")}, T={contactT.ToString("F3")}");

                            ///     결과 저장
                            ts = DateTime.Now;
                            Global.clsFolderThread.RequestSave(ts, "CONTACT_ALIGN");
                            SetProcessMsg(m_Step, $"    Result Saved");

                            /// 결과 전송
                            WriteContactAlignAoResult();

                            // DO로 결과 핸드셰이크 (REQ=16에 대한 응답 17/18/19) 
                            Global.inforPLC.OutContactOk = success;
                            Global.inforPLC.OutContactNg = !success;

                            // PLC가 CONTACT_REQ(16) 내릴 때까지 대기
                            startTime = Environment.TickCount;

                            Stopwatch stopwatch = new Stopwatch();
                            stopwatch.Restart();
                            while (stopwatch.ElapsedMilliseconds < REQ_OFF_TIMEOUT_MS)
                            {
                                if (!Global.inforPLC.InContactRequest)
                                {
                                    break;
                                }
                                Thread.Sleep(5);
                            }

                            // 비트 정리
                            Global.inforPLC.OutContactOk = false;
                            Global.inforPLC.OutContactNg = false;
                            Global.inforPLC.OutContactReply = false;

                            m_Step = SeqStep.WAIT;
                        }
                        break;
                    case SeqStep.MODEL_CHANGE:
                        {
                            //Model Chage
                            //로그
                            //결과 회신
                        }
                        break;
                    case SeqStep.CALIBRATION_MODE:
                        {
                            // Reply 전송
                            Global.inforPLC.OutCalibrationReply = true;
                            if (Global.bCalibrationManualMode)
                                break;

                            if (!SetTeachingData(0))
                            {
                                Global.m_AlarmCode = AlarmCode.CameraMotionCommError;
                                m_Step = SeqStep.ALARM;
                            }

                            #region UVW Start Position Offset
                            int imageCount = Global.Calibration_Param.AverageImageCount;

                            double preMoveX = Global.Calibration_Param.StartPositionX; //300;
                            double preMoveY = Global.Calibration_Param.StartPositionY; //-1200;
                            double preMoveT = Global.Calibration_Param.StartPositionAngle; //0;

                            #endregion

                            if (Global.m_AlarmCode == AlarmCode.None)
                            {

                                #region 9-Point Camera Calibration
                                SetProcessMsg(m_Step, "9-Point Camera Calibration Start");

                                int pointIndex = 0;
                                PointF[][] camCalPoints = new PointF[(int)CamInfo.MAX][];
                                PointF[][] camViberationPoints = new PointF[(int)CamInfo.MAX][];
                                camCalPoints[(int)CamInfo.CAM_1] = new PointF[9] { new PointF(0, 0), new PointF(0, 0), new PointF(0, 0), new PointF(0, 0), new PointF(0, 0), new PointF(0, 0), new PointF(0, 0), new PointF(0, 0), new PointF(0, 0) };
                                camCalPoints[(int)CamInfo.CAM_2] = new PointF[9] { new PointF(0, 0), new PointF(0, 0), new PointF(0, 0), new PointF(0, 0), new PointF(0, 0), new PointF(0, 0), new PointF(0, 0), new PointF(0, 0), new PointF(0, 0) };
                                camViberationPoints[(int)CamInfo.CAM_1] = new PointF[imageCount];
                                camViberationPoints[(int)CamInfo.CAM_2] = new PointF[imageCount];
                                for(int points = 0; points <  imageCount; points++)
                                {
                                    camViberationPoints[(int)CamInfo.CAM_1][points] = new PointF(0, 0);
                                    camViberationPoints[(int)CamInfo.CAM_2][points] = new PointF(0, 0);
                                }
                                bool camCalSuccess = true;
                                double camCalX, camCalY, camCalT = 0.0;
                                double camCalOffset = Global.Calibration_Param.MoveOffsetXY;


                                //     총 9개 포인트 일정간격(Config 설정) XY 이동하여 이미지 취득 & Fiducial Mark 좌표 저장 (FindFiducialMark 활용)
                                for (int yy = -1; yy <= 1; yy++)
                                {
                                    camCalY = camCalOffset * yy;
                                    for (int xx = -1; xx <= 1; xx++)
                                    {
                                        SetProcessMsg(m_Step, $"    UVW Stage Move {pointIndex.ToString()} Start");
                                        camCalX = camCalOffset * xx;
                                        if (!Global.clsUvwStage.Move(preMoveX + camCalX, preMoveY + camCalY, preMoveT + camCalT))
                                        {
                                            //UVW 이동 실패로 인한 Calibration 실패.
                                            SetProcessMsg(m_Step, $"    UVW Stage Move {pointIndex.ToString()} Failed");
                                            Global.m_AlarmCode = AlarmCode.UvwStageCommError;
                                            camCalSuccess = false;
                                            break;
                                        }
                                        SetProcessMsg(m_Step, $"    UVW Stage Move {pointIndex.ToString()} End");

                                        PointF[] sumPoints = new PointF[(int)CamInfo.MAX];
                                        PointF[] averagePoint = new PointF[(int)CamInfo.MAX];
                                        sumPoints[(int)CamInfo.CAM_1] = new PointF(0f, 0f);
                                        sumPoints[(int)CamInfo.CAM_2] = new PointF(0f, 0f);
                                        averagePoint[(int)CamInfo.CAM_1] = new PointF(0f, 0f);
                                        averagePoint[(int)CamInfo.CAM_2] = new PointF(0f, 0f);

                                        SetProcessMsg(m_Step, $"    Find Cal Index {pointIndex.ToString()} Point Start");
                                        for (int icnt = 0; icnt < imageCount; icnt++)
                                        {
                                            // 이미지 취득
                                            if (!GrabAllCamera())
                                            {
                                                Global.m_AlarmCode = AlarmCode.CameraCommError;
                                                camCalSuccess = false;
                                                break;
                                            }

                                            for (int c = 0; c < (int)CamInfo.MAX; c++)
                                            {
                                                CamInfo cam = (CamInfo)c;


                                                double camPixelX, camPixelY, camScore, camScale;
                                                if (!Global.clsVision.FindMaskHole(cam, out camPixelX, out camPixelY, out camScore, out camScale))
                                                {
                                                    Global.m_AlarmCode = AlarmCode.CalibrationError;
                                                    camCalSuccess = false;
                                                    SetProcessMsg(m_Step, $"    Failed : Failed to find Mask Hole");
                                                }
                                                if (camScore < Global.Calibration_Param.MarkScoreLimit)
                                                {
                                                    Global.m_AlarmCode = AlarmCode.CalibrationError;
                                                    camCalSuccess = false;
                                                    SetProcessMsg(m_Step, $"    Failed : Mask Shape score limited");
                                                }

                                                sumPoints[(int)cam].X += (float)camPixelX;
                                                sumPoints[(int)cam].Y += (float)camPixelY;

                                                if(icnt == 4)
                                                {
                                                    camViberationPoints[(int)CamInfo.CAM_1][icnt].X = (float)camPixelX;
                                                    camViberationPoints[(int)CamInfo.CAM_1][icnt].Y = (float)camPixelY;
                                                }

                                                if(icnt == imageCount-1 && camCalSuccess) //마지막 이미지 취득할 때 값 적용.
                                                {
                                                    averagePoint[(int)cam].X = sumPoints[(int)cam].X / (float)imageCount;
                                                    averagePoint[(int)cam].Y = sumPoints[(int)cam].Y / (float)imageCount;
                                                    camCalPoints[(int)cam][pointIndex] = averagePoint[(int)cam];
                                                }
                                            }

                                            if (!camCalSuccess)
                                                break;
                                        }
                                        SetProcessMsg(m_Step, $"    Find Cal Index {pointIndex.ToString()} Point End");

                                        if (!camCalSuccess)
                                            break;
                                        pointIndex++;

                                        for(int c= 0; c< (int)CamInfo.MAX; c++)
                                        {
                                            //지난 Calibration Points 그리기
                                            Global.clsAlgorithm[c].ClearOverlay(Global.formHDisplay[c].HWindow);
                                            for (int p = 0; p < pointIndex; p++)
                                                ClsAlgorithm.OverlayCross(Global.formHDisplay[c].HWindow, camCalPoints[c][p].X, camCalPoints[c][p].Y, 0, "yellow");
                                        }
                                    }
                                    if (!camCalSuccess)
                                        break;
                                }
                                if (!camCalSuccess)
                                {
                                    Global.m_AlarmCode = AlarmCode.CalibrationError;
                                    SetProcessMsg(m_Step, "9-Point Camera Calibration Failed");
                                }

                                if (camCalSuccess)
                                {
                                    //     CalibrationCameraTilt 사용하여 Calibration.
                                    if (!Global.clsVision.CalibrationCamera(CamInfo.CAM_1, Global.Calibration_Param.MoveOffsetXY, camCalPoints[(int)CamInfo.CAM_1]))
                                    {
                                        Global.m_AlarmCode = AlarmCode.CalibrationError;
                                        SetProcessMsg(m_Step, "9-Point Camera Calibration Failed");
                                    }
                                    if (!Global.clsVision.CalibrationCamera(CamInfo.CAM_2, Global.Calibration_Param.MoveOffsetXY, camCalPoints[(int)CamInfo.CAM_2]))
                                    {
                                        Global.m_AlarmCode = AlarmCode.CalibrationError;
                                        SetProcessMsg(m_Step, "9-Point Camera Calibration Failed");
                                    }

                                    Global.IsLoad_List(false, Global.Calibration_Param);


                                    Global.clsFolderThread.SaveBothCamsBmp(DateTime.Now, "Cal", "CALIBRATION", false);
                                    Global.clsCSV.WriteCalibrationCsv(camCalPoints);
                                    Global.clsCSV.WriteVibrationCsv(camViberationPoints);


                                    SetProcessMsg(m_Step, "9-Point Camera Calibration End");
                                }
                                #endregion

                                #region uvw Stage Calibration
                                // UVW Stage map Cal
                                pointIndex = 0;
                                List<PointF>[] uvwCalPoints = new List<PointF>[(int)CamInfo.MAX];
                                uvwCalPoints[(int)CamInfo.CAM_1] = new List<PointF>();
                                uvwCalPoints[(int)CamInfo.CAM_2] = new List<PointF>();
                                bool uvwCalMoveSuccess = true;
                                double uvwCalX = 0.0, uvwCalY = 0.0, uvwCalT = 0.0;
                                double uvwCalOffset = Global.Calibration_Param.MoveOffsetAngle;
                                int uvwCalOffsetCount = Global.Calibration_Param.MoveOffsetAngleCount;

                                double startAngle = -(uvwCalOffset * (uvwCalOffsetCount - 1)) / 2.0;

                                SetProcessMsg(m_Step, "UVW Stage Calibration Start");
                                for (int i = 0; i < uvwCalOffsetCount; i++)
                                {
                                    uvwCalT = startAngle + uvwCalOffset * i;
                                    SetProcessMsg(m_Step, $"    UVW Stage Move {pointIndex.ToString()} Start");
                                    if (!Global.clsUvwStage.Move(preMoveX + uvwCalX, preMoveY + uvwCalY, preMoveT + uvwCalT))
                                    {
                                        //UVW 이동 실패로 인한 Calibration 실패.
                                        Global.m_AlarmCode = AlarmCode.UvwStageCommError;
                                        uvwCalMoveSuccess = false;
                                        SetProcessMsg(m_Step, $"    UVW Stage Move {pointIndex.ToString()} Failed");
                                        break;
                                    }
                                    SetProcessMsg(m_Step, $"    UVW Stage Move {pointIndex.ToString()} End");

                                    PointF[] sumPoints = new PointF[(int)CamInfo.MAX];
                                    PointF[] averagePoint = new PointF[(int)CamInfo.MAX];
                                    sumPoints[(int)CamInfo.CAM_1] = new PointF(0f, 0f);
                                    sumPoints[(int)CamInfo.CAM_2] = new PointF(0f, 0f);
                                    averagePoint[(int)CamInfo.CAM_1] = new PointF(0f, 0f);
                                    averagePoint[(int)CamInfo.CAM_2] = new PointF(0f, 0f);

                                    SetProcessMsg(m_Step, $"    Find Cal Index {pointIndex.ToString()} Point Start");
                                    for (int icnt = 0; icnt < imageCount; icnt++)
                                    {
                                        // 이미지 취득
                                        if (!GrabAllCamera())
                                        {
                                            Global.m_AlarmCode = AlarmCode.CameraCommError;
                                            uvwCalMoveSuccess = false;
                                            break;
                                        }

                                        for (int c = 0; c < (int)CamInfo.MAX; c++)
                                        {

                                            CamInfo cam = (CamInfo)c;
                                            double uvwPixelX, uvwPixelY, uvwScore, uvwScale;
                                            if (!Global.clsVision.FindMaskHole(cam, out uvwPixelX, out uvwPixelY, out uvwScore, out uvwScale))
                                            {
                                                Global.m_AlarmCode = AlarmCode.CalibrationError;
                                                uvwCalMoveSuccess = false;
                                            }
                                            if (uvwScore < Global.Calibration_Param.MarkScoreLimit)
                                            {
                                                Global.m_AlarmCode = AlarmCode.CalibrationError;
                                                uvwCalMoveSuccess = false;
                                            }

                                            sumPoints[(int)cam].X += (float)uvwPixelX;
                                            sumPoints[(int)cam].Y += (float)uvwPixelY;

                                            if (icnt == imageCount - 1 && uvwCalMoveSuccess) //마지막 이미지 취득할 때 값 적용.
                                            {
                                                averagePoint[(int)cam].X = sumPoints[(int)cam].X / (float)imageCount;
                                                averagePoint[(int)cam].Y = sumPoints[(int)cam].Y / (float)imageCount;
                                                uvwCalPoints[(int)cam].Add(averagePoint[(int)cam]);
                                            }
                                        }
                                    }
                                    SetProcessMsg(m_Step, $"    Find Cal Index {pointIndex.ToString()} Point End");

                                    if (!uvwCalMoveSuccess)
                                        break;

                                    pointIndex++;

                                    //결과 그리기
                                    for (int c = 0; c < (int)CamInfo.MAX; c++)
                                    {
                                        Global.clsAlgorithm[c].ClearOverlay(Global.formHDisplay[c].HWindow);
                                        //Camera Cal Point 그리기
                                        for (int p = 0; p < camCalPoints[c].Length; p++)
                                            ClsAlgorithm.OverlayCross(Global.formHDisplay[c].HWindow, camCalPoints[c][p].X, camCalPoints[c][p].Y, 0, "white");
                                        //지난 Calibration Points 그리기
                                        for (int p = 0; p < pointIndex; p++)
                                            ClsAlgorithm.OverlayCross(Global.formHDisplay[c].HWindow, uvwCalPoints[c][p].X, uvwCalPoints[c][p].Y, 0, "yellow");
                                    }
                                }
                                if (!uvwCalMoveSuccess)
                                {
                                    Global.m_AlarmCode = AlarmCode.CalibrationError;
                                    SetProcessMsg(m_Step, "UVW Stage Calibration Failed");
                                }

                                if (uvwCalMoveSuccess && camCalSuccess)
                                {
                                    if (!Global.clsVision.CalibrationUvwStage(CamInfo.CAM_1, camCalPoints[(int)CamInfo.CAM_1][4]))
                                    {
                                        Global.m_AlarmCode = AlarmCode.CalibrationError;
                                        SetProcessMsg(m_Step, "UVW Stage Calibration Failed");
                                    }
                                    if (!Global.clsVision.CalibrationUvwStage(CamInfo.CAM_2, camCalPoints[(int)CamInfo.CAM_2][4]))
                                    {
                                        Global.m_AlarmCode = AlarmCode.CalibrationError;
                                        SetProcessMsg(m_Step, "UVW Stage Calibration Failed");
                                    }

                                    // FiducialMarkDistance 계산 (중간 포인트 사용 - 각도 0에 가장 가까운 위치)
                                    if (Global.m_AlarmCode == AlarmCode.None)
                                    {
                                        int midIndex = uvwCalPoints[(int)CamInfo.CAM_1].Count / 2;
                                        PointF cam1Point = uvwCalPoints[(int)CamInfo.CAM_1][midIndex];
                                        PointF cam2Point = uvwCalPoints[(int)CamInfo.CAM_2][midIndex];
                                        Global.clsVision.GetFiducialMarkDistance(cam1Point, cam2Point);
                                    }

                                    Global.IsLoad_List(false, Global.Calibration_Param);

                                    SetProcessMsg(m_Step, "UVW Stage Calibration End");
                                }
                                #endregion

                                #region Mask move to Camera Center Position.
                                if (uvwCalMoveSuccess && camCalSuccess)
                                {
                                    SetProcessMsg(m_Step, "Mask Move to Camera Center Start");
                                    double camCenX, camCenY, camCenT = 0.0;

                                    ///     이미지 취득
                                    if (!GrabAllCamera())
                                        continue;

                                    if (Global.clsVision.GetUvwCameraCenterPos(out camCenX, out camCenY, out camCenT))
                                    {
                                        if (Global.inforResult.strJudg == "OK")
                                            success = true;
                                        else
                                            success = false;
                                    }
                                    else
                                    {
                                        //Glass or Mask 인식 실패 알람 처리
                                        success = false;
                                    }

                                    SetProcessMsg(m_Step, $"    UVW Stage Move Start");
                                    if (!Global.clsUvwStage.Move(camCenX, camCenY, camCenT))
                                    {
                                        //UVW 이동 실패로 인한 Calibration 실패.
                                        SetProcessMsg(m_Step, $"    UVW Stage Move Failed");
                                        Global.m_AlarmCode = AlarmCode.UvwStageCommError;
                                        break;
                                    }


                                    Global.Calibration_Param.StartPositionX = camCenX; 
                                    Global.Calibration_Param.StartPositionY = camCenY; 
                                    Global.Calibration_Param.StartPositionAngle = camCenT;



                                    ///     이미지 취득
                                    if (!GrabAllCamera())
                                        continue;


                                    Global.IsLoad_List(false, Global.Calibration_Param);
                                    SetProcessMsg(m_Step, "Mask Move to Camera Center End");
                                }
                                #endregion

                            }

                            Global.inforPLC.OutCalibrationStop = true;
                            Thread.Sleep(500);
                            Global.inforPLC.OutCalibrationReply = false;

                            Stopwatch stopwatch = new Stopwatch();
                            stopwatch.Restart();
                            while (stopwatch.ElapsedMilliseconds < REQ_OFF_TIMEOUT_MS)
                            {
                                if (!Global.inforPLC.InCalibrationRequest)
                                    break;
                                Thread.Sleep(5);
                            }

                            Global.inforPLC.OutCalibrationStop = false;

                            m_Step = SeqStep.WAIT;
                        }
                        break;
                    case SeqStep.ALARM:
                        {
                            if (Global.m_AlarmCode == AlarmCode.None)
                                Global.m_AlarmCode = AlarmCode.Unknown;

                            if (Global.inforPLC.OutAlarmCode == (int)Global.m_AlarmCode)
                            {
                                m_Step = SeqStep.WAIT;
                                break;
                            }

                            Global.inforPLC.OutAlarm = true;
                            Global.inforPLC.OutAlarmCode = (int)Global.m_AlarmCode;
                            Global.inforPLC.OutReady = false;

                            m_Step = SeqStep.WAIT;
                        }
                        break;
                    case SeqStep.CLEAR:
                        {
                            Global.m_AlarmCode = AlarmCode.None;

                            //PLC 출력 영역 모두 초기화
                            Global.inforPLC.ClearOutput();

                            Global.bAlarmClearReq = false;
                            m_Step = SeqStep.WAIT;
                        }
                        break;
                }
                if(m_Step != m_StepPrev)
                    SetProcessMsg(m_StepPrev, "End");
            }
        }

        private void SetProcessMsg(SeqStep step, string msg)
        {
            if (ProcessStatusMsg == null)
                return;
            ProcessStatusMsg.Invoke(this, $"[{step.ToString()}] {msg}");
        }

        private void WritePreAlignAoResult()
        {
            Global.inforPLC.OutPreAlignTime = Global.inforResult.dFullProcessTime;

            // Glass 결과
            Global.inforPLC.OutPreAlignCam1GlassX = Global.inforResult.dTarget_X[0];
            Global.inforPLC.OutPreAlignCam1GlassY = Global.inforResult.dTarget_Y[0];
            Global.inforPLC.OutPreAlignCam1GlassScore = Global.inforResult.dTarget_Score[0];
            Global.inforPLC.OutPreAlignCam2GlassX =              Global.inforResult.dTarget_X[1]     ;
            Global.inforPLC.OutPreAlignCam2GlassY =              Global.inforResult.dTarget_Y[1]     ;
            Global.inforPLC.OutPreAlignCam2GlassScore = Global.inforResult.dTarget_Score[1];

            // Mask 결과
            Global.inforPLC.OutPreAlignCam1MaskX = Global.inforResult.dMark_X[0];
            Global.inforPLC.OutPreAlignCam1MaskY = Global.inforResult.dMark_Y[0];
            Global.inforPLC.OutPreAlignCam1MaskScore = Global.inforResult.dMark_Score[0];
            Global.inforPLC.OutPreAlignCam2MaskX = Global.inforResult.dMark_X[1];
            Global.inforPLC.OutPreAlignCam2MaskY = Global.inforResult.dMark_Y[1];
            Global.inforPLC.OutPreAlignCam2MaskScore = Global.inforResult.dMark_Score[1];

            // 종합 Slip 결과
            Global.inforPLC.OutPreAlignErrorX = Global.inforResult.resultErrorX;
            Global.inforPLC.OutPreAlignErrorY = Global.inforResult.resultErrorY;
            Global.inforPLC.OutPreAlignErrorT = Global.inforResult.resultErrorT;
        }

        private void WriteContactAlignAoResult()
        {
            Global.inforPLC.OutContactAlignTime = Global.inforResult.dFullProcessTime;

            // Glass 결과
            Global.inforPLC.OutContactAlignCam1GlassX = Global.inforResult.dTarget_X[0];
            Global.inforPLC.OutContactAlignCam1GlassY = Global.inforResult.dTarget_Y[0];
            Global.inforPLC.OutContactAlignCam1GlassScore = Global.inforResult.dTarget_Score[0];
            Global.inforPLC.OutContactAlignCam2GlassX = Global.inforResult.dTarget_X[1];
            Global.inforPLC.OutContactAlignCam2GlassY = Global.inforResult.dTarget_Y[1];
            Global.inforPLC.OutContactAlignCam2GlassScore = Global.inforResult.dTarget_Score[1];
                               
            // Mask 결과       
            Global.inforPLC.OutContactAlignCam1MaskX = Global.inforResult.dMark_X[0];
            Global.inforPLC.OutContactAlignCam1MaskY = Global.inforResult.dMark_Y[0];
            Global.inforPLC.OutContactAlignCam1MaskScore = Global.inforResult.dMark_Score[0];
            Global.inforPLC.OutContactAlignCam2MaskX = Global.inforResult.dMark_X[1];
            Global.inforPLC.OutContactAlignCam2MaskY = Global.inforResult.dMark_Y[1];
            Global.inforPLC.OutContactAlignCam2MaskScore = Global.inforResult.dMark_Score[1];
                               
            // 종합 Slip 결과  
            Global.inforPLC.OutContactAlignErrorX = Global.inforResult.resultErrorX;
            Global.inforPLC.OutContactAlignErrorY = Global.inforResult.resultErrorY;
            Global.inforPLC.OutContactAlignErrorT = Global.inforResult.resultErrorT;

            Global.inforResult.ContactErrorX = Global.inforResult.resultErrorX;
            Global.inforResult.ContactErrorY = Global.inforResult.resultErrorY;
            Global.inforResult.ContactErrorT = Global.inforResult.resultErrorT;
        }

        /// <summary>
        /// Z축 구동
        /// </summary>
        /// <param name="teachingIndex">0:Common, 1:PreAlign, 2:Contact</param>
        /// <returns></returns>
        private bool MoveZAxis(int teachingIndex)
        {
            // Z축 ContactAlign 위치로 이동
            int model = teachingIndex;
            double speed = 1;
            double pos = 1;
            bool moveSuccess = true;
            for (int i = 0; i < (int)CamInfo.MAX; i++)
            {
                Thread.Sleep(10);
                CamInfo cam = (CamInfo)i;
                speed = Global.CamSet_Param.dicZ_Speed[cam][model];
                pos = Global.CamSet_Param.dicZ_Pos[cam][model];
                if (!Global.dicClsMotion[cam].Move(pos, speed, speed * 3, speed * 3))
                {
                    SetProcessMsg(m_Step, $"    {cam.ToString()} Z Axis Move Failed : Can not start move");
                    continue;
                }
                SetProcessMsg(m_Step, $"    {cam.ToString()} Z Axis Move Start");

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Restart();
                Global.dicClsMotion[cam].UpdateStatus();

                while (Global.inforMotion.bBasy[i])
                {
                    Global.dicClsMotion[cam].UpdateStatus();
                    if (stopwatch.ElapsedMilliseconds > 15000)
                    {
                        SetProcessMsg(m_Step, $"    {cam.ToString()} Z Axis Move Failed : Inposition TimeOut");
                        moveSuccess = false;
                        break;
                    }
                    Thread.Sleep(10);
                }
                SetProcessMsg(m_Step, $"    {cam.ToString()} Z Axis Move End");
            }

            return moveSuccess;
        }

        private bool SetCamera(int teachingIndex)
        {
            for(int i = 0; i < (int)CamInfo.MAX; i++)
            {
                CamInfo cam = (CamInfo)i;

                Global.dicClsCam[cam].SetParam(
                    Global.CamSet_Param.dicExposure[cam][teachingIndex],
                    Global.CamSet_Param.dicGain[cam][teachingIndex],
                    Global.CamSet_Param.dicGamma[cam][teachingIndex],
                    Global.CamSet_Param.dicFlipX[cam][teachingIndex],
                    Global.CamSet_Param.dicFlipY[cam][teachingIndex],
                    Global.CamSet_Param.dicRotation[cam][teachingIndex]);

                SetProcessMsg(m_Step, $"    {cam.ToString()} Camera Parameter Set");

            }
            return true;
        }

        private bool SetLight(int teachingIndex)
        {
            for (int i = 0; i < (int)CamInfo.MAX; i++)
            {
                CamInfo cam = (CamInfo)i;
                int spotLightChennel = cam == 0 ? 2 : 0;
                int LingLightChennel = cam == 0 ? 3 : 1;

                Global.clsLight.LightValue(Global.CamSet_Param.dicLightSpot[cam][teachingIndex], spotLightChennel);
                Global.clsLight.LightValue(Global.CamSet_Param.dicLightRing[cam][teachingIndex], LingLightChennel);

                Global.clsLight.LightOnOff(true, spotLightChennel);
                Global.clsLight.LightOnOff(true, LingLightChennel);

                SetProcessMsg(m_Step, $"    {cam.ToString()} Light Controller Set");
            }
            return true;
        }

        private bool SetTeachingData(int teachingIndex)
        {
            if (!MoveZAxis(teachingIndex))
            {
                return false;
            }
            if(!SetCamera(teachingIndex))
            {
                return false;
            }
            if (!SetLight(teachingIndex))
            {
                return false;
            }
            return true;
        }

    }
}
