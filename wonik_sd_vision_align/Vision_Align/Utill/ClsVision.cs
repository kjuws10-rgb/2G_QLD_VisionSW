using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vision_Align
{
    internal class ClsVision
    {
        /// <summary>
        /// Fiducial Mark 대비 Mask hole 까지 UVW Stage가 이동해야할 Target 위치를 구함.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public bool GetUvwTargetPos(out double x, out double y, out double t)
        {
            return GetUvwTargetPos(out x, out y, out t, -1, -1, -1, -1, -1, -1, -1, -1);
        }
        /// <summary>
        /// Fiducial Mark 대비 Mask hole 까지 UVW Stage가 이동해야할 Target 위치를 구함.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public bool GetUvwTargetPos(out double x, out double y, out double t, 
            double maskPixelX1, double maskPixelY1, double maskScore1, double maskScale1,
            double maskPixelX2, double maskPixelY2, double maskScore2, double maskScale2)
        {
            x = 0;
            y = 0;
            t = 0;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Restart();

            try
            {
                // 1. FindFiducialMark와 FindMaskHole을 사용하여 값들을 찾고 Global.inforResult에 저장
                // CAM_1에서 찾기
                Global.clsAlgorithm[0].ClearOverlay(Global.formHDisplay[0].HWindow);
                FindFiducialMark(CamInfo.CAM_1, out double fiducialPixelX1, out double fiducialPixelY1, out double fiducialScore1, out double fiducialScale1);
                if(maskPixelX1 < 0) // 0보다 작은값이 들어오면 값이 없다고 인지 Hole 위치 가져오기
                    FindMaskHole(CamInfo.CAM_1, out maskPixelX1, out maskPixelY1, out maskScore1, out maskScale1);

                // CAM_2에서 찾기
                Global.clsAlgorithm[1].ClearOverlay(Global.formHDisplay[1].HWindow);
                FindFiducialMark(CamInfo.CAM_2, out double fiducialPixelX2, out double fiducialPixelY2, out double fiducialScore2, out double fiducialScale2);
                if(maskPixelX2 < 0) // 0보다 작은값이 들어오면 값이 없다고 인지하고 Hole 위치 가져오기
                    FindMaskHole(CamInfo.CAM_2, out maskPixelX2, out maskPixelY2, out maskScore2, out maskScale2);

                // Global.inforResult에 저장
                Global.inforResult.dTarget_X[0] = fiducialPixelX1;
                Global.inforResult.dTarget_Y[0] = fiducialPixelY1;
                Global.inforResult.dTarget_Score[0] = fiducialScore1;
                Global.inforResult.dTarget_Scale[0] = fiducialScale1;
                Global.inforResult.strTarget_Match[0] = Matching_Type.GLASS_SHAPE.ToString();

                Global.inforResult.dTarget_X[1] = fiducialPixelX2;
                Global.inforResult.dTarget_Y[1] = fiducialPixelY2;
                Global.inforResult.dTarget_Score[1] = fiducialScore2;
                Global.inforResult.dTarget_Scale[1] = fiducialScale2;
                Global.inforResult.strTarget_Match[1] = Matching_Type.GLASS_SHAPE.ToString();

                Global.inforResult.dMark_X[0] = maskPixelX1;
                Global.inforResult.dMark_Y[0] = maskPixelY1;
                Global.inforResult.dMark_Score[0] = maskScore1;
                Global.inforResult.dMark_Scale[0] = maskScale1;
                Global.inforResult.strMark_Match[0] = Matching_Type.MASK_SHAPE.ToString();

                Global.inforResult.dMark_X[1] = maskPixelX2;
                Global.inforResult.dMark_Y[1] = maskPixelY2;
                Global.inforResult.dMark_Score[1] = maskScore2;
                Global.inforResult.dMark_Scale[1] = maskScale2;
                Global.inforResult.strMark_Match[1] = Matching_Type.MASK_SHAPE.ToString();

                // 2. PixelPosToRealPos를 사용하여 Hardware 좌표로 변환
                PixelPosToRealPos(CamInfo.CAM_1, fiducialPixelX1, fiducialPixelY1, out double fiducialRealX1, out double fiducialRealY1);
                PixelPosToRealPos(CamInfo.CAM_2, fiducialPixelX2, fiducialPixelY2, out double fiducialRealX2, out double fiducialRealY2);
                PixelPosToRealPos(CamInfo.CAM_1, maskPixelX1, maskPixelY1, out double maskRealX1, out double maskRealY1);
                PixelPosToRealPos(CamInfo.CAM_2, maskPixelX2, maskPixelY2, out double maskRealX2, out double maskRealY2);

                // 3. MaskHole 2점을 이용하여 Mask Center 점 계산
                double maskCenterX = (maskRealX1 + maskRealX2) / 2.0;
                double maskCenterY = (maskRealY1 + maskRealY2) / 2.0;
                double maskAngle = Math.Atan2(maskRealY2 - maskRealY1, maskRealX2 - maskRealX1) * 180.0 / Math.PI;

                // 4. FiducialMark 2점을 이용하여 Glass Center 점 계산
                double glassCenterX = (fiducialRealX1 + fiducialRealX2) / 2.0;
                double glassCenterY = (fiducialRealY1 + fiducialRealY2) / 2.0;
                double glassAngle = Math.Atan2(fiducialRealY2 - fiducialRealY1, fiducialRealX2 - fiducialRealX1) * 180.0 / Math.PI;

                // 5. Mask와 Glass의 Center점을 비교하여 필요한 Glass 이동량 계산 (순수 Image 상 이동량)
                double deltaX = glassCenterX - maskCenterX;
                double deltaY = glassCenterY - maskCenterY;
                double deltaAngle = glassAngle - maskAngle;

                // 6. Global.clsUvwStage.CurrentPos를 가져와서 StageCalInverse로 현재 X,Y,T 위치 역산
                double currentX1 = Global.inforPLC.InStageCurrentU;
                double currentX2 = Global.inforPLC.InStageCurrentW;
                double currentY1 = Global.inforPLC.InStageCurrentV;

                // Stage Virture Offset 적용
                currentX1 += Global.PreConfig_Param.dUVW_OffsetX1;
                currentX2 += Global.PreConfig_Param.dUVW_OffsetX2;
                currentY1 += Global.PreConfig_Param.dUVW_OffsetY1;

                ClsAlgorithm.StageCalInverse(currentX1, currentX2, currentY1,
                    out double currentX, out double currentY, out double currentAngle);

                // 7. 현재 위치에 이동량을 반영하여 Target Position 계산
                double targetX = currentX + deltaX;
                double targetY = currentY + deltaY;
                double targetAngle = currentAngle + deltaAngle;

                if (Global.bAutoMode)
                {
                    if (Global.clsAutoThread.CurrentStep == SeqStep.PRE_ALIGN)
                    {
                        // 오차 값 PRE Mode 일 때 Contact Slip Offset 차감 적용
                        deltaX -= Global.inforResult.ContactErrorX;
                        deltaY -= Global.inforResult.ContactErrorY;
                        deltaAngle -= Global.inforResult.ContactErrorT;

                        // Target 위치 PRE Mode 일 때 Contact Slip Offset 적용
                        targetX += Global.inforResult.ContactErrorX;
                        targetY += Global.inforResult.ContactErrorY;
                        targetAngle += Global.inforResult.ContactErrorT;
                    }
                }

                Global.inforResult.resultErrorX = deltaX;
                Global.inforResult.resultErrorY = deltaY;
                Global.inforResult.resultErrorT = deltaAngle;

                Global.inforResult.dFullProcessTime = Convert.ToDouble(stopwatch.ElapsedMilliseconds);
                Global.inforResult.dProcessTime[0] = Global.inforResult.dFullProcessTime / 2.0;
                Global.inforResult.dProcessTime[1] = Global.inforResult.dFullProcessTime / 2.0;

                double alignXY = Global.PreConfig_Param.dContactAlignXY;
                double alignT = Global.PreConfig_Param.dContactAlignTh;
                if (Global.bAutoMode)
                { 
                    if(Global.clsAutoThread.CurrentStep == SeqStep.PRE_ALIGN)
                    {
                        alignXY = Global.PreConfig_Param.dPreAlignXY;
                        alignT = Global.PreConfig_Param.dPreAlignTh;
                    }
                }

                string judgement = "";
                if (Global.PreConfig_Param.dLimitGlass > fiducialScore1)
                    judgement = "Cam1 Glass Score NG";
                else if (Global.PreConfig_Param.dLimitMark > maskScore1)
                    judgement = "Cam1 Mask Score NG";
                else if (Global.PreConfig_Param.dLimitGlass > fiducialScore2)
                    judgement = "Cam2 Glass Score NG";
                else if (Global.PreConfig_Param.dLimitMark > maskScore2)
                    judgement = "Cam2 Mask Score NG";
                else if (alignXY < Math.Abs(deltaX) || alignXY < Math.Abs(deltaY))
                    judgement = "XY Align NG";
                else if (alignT < Math.Abs(deltaAngle))
                    judgement = "T Align NG";
                else
                    judgement = "OK";

                Global.inforResult.strJudg = judgement;
                Global.inforResult.TargetX = targetX;
                Global.inforResult.TargetY = targetY;
                Global.inforResult.TargetT = targetAngle;
                Global.inforResult.nRetry = Global.inforPLC.InPmcCurrentRetry;

                // 출력값 설정
                x = targetX;
                y = targetY;
                t = targetAngle;

                return true;
            }
            catch
            {
                return false;
            }
        }


        /// <summary>
        /// Fiducial Mark 대비 Mask hole 까지 UVW Stage가 이동해야할 Target 위치를 구함.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public bool GetUvwCameraCenterPos(out double x, out double y, out double t)
        {
            x = 0;
            y = 0;
            t = 0;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Restart();

            try
            {
                // 1. FindFiducialMark와 FindMaskHole을 사용하여 값들을 찾고 Global.inforResult에 저장
                // CAM_1에서 찾기
                Global.clsAlgorithm[0].ClearOverlay(Global.formHDisplay[0].HWindow);
                double fiducialPixelX1 = Global.dicClsCam[CamInfo.CAM_1].Width / 2.0;
                double fiducialPixelY1 = Global.dicClsCam[CamInfo.CAM_1].Height / 2.0;
                FindMaskHole(CamInfo.CAM_1, out double maskPixelX1, out double maskPixelY1, out double maskScore1, out double maskScale1);

                // CAM_2에서 찾기
                Global.clsAlgorithm[1].ClearOverlay(Global.formHDisplay[1].HWindow);
                double fiducialPixelX2 = Global.dicClsCam[CamInfo.CAM_2].Width / 2.0;
                double fiducialPixelY2 = Global.dicClsCam[CamInfo.CAM_2].Height / 2.0;
                FindMaskHole(CamInfo.CAM_2, out double maskPixelX2, out double maskPixelY2, out double maskScore2, out double maskScale2);

                // Global.inforResult에 저장
                Global.inforResult.dMark_X[0] = maskPixelX1;
                Global.inforResult.dMark_Y[0] = maskPixelY1;
                Global.inforResult.dMark_Score[0] = maskScore1;
                Global.inforResult.dMark_Scale[0] = maskScale1;
                Global.inforResult.strMark_Match[0] = Matching_Type.MASK_SHAPE.ToString();

                Global.inforResult.dMark_X[1] = maskPixelX2;
                Global.inforResult.dMark_Y[1] = maskPixelY2;
                Global.inforResult.dMark_Score[1] = maskScore2;
                Global.inforResult.dMark_Scale[1] = maskScale2;
                Global.inforResult.strMark_Match[1] = Matching_Type.MASK_SHAPE.ToString();

                // 2. PixelPosToRealPos를 사용하여 Hardware 좌표로 변환
                PixelPosToRealPos(CamInfo.CAM_1, fiducialPixelX1, fiducialPixelY1, out double fiducialRealX1, out double fiducialRealY1);
                PixelPosToRealPos(CamInfo.CAM_2, fiducialPixelX2, fiducialPixelY2, out double fiducialRealX2, out double fiducialRealY2);
                PixelPosToRealPos(CamInfo.CAM_1, maskPixelX1, maskPixelY1, out double maskRealX1, out double maskRealY1);
                PixelPosToRealPos(CamInfo.CAM_2, maskPixelX2, maskPixelY2, out double maskRealX2, out double maskRealY2);

                // 3. MaskHole 2점을 이용하여 Mask Center 점 계산
                double maskCenterX = (maskRealX1 + maskRealX2) / 2.0;
                double maskCenterY = (maskRealY1 + maskRealY2) / 2.0;
                double maskAngle = Math.Atan2(maskRealY2 - maskRealY1, maskRealX2 - maskRealX1) * 180.0 / Math.PI;

                // 4. FiducialMark 2점을 이용하여 Glass Center 점 계산
                double glassCenterX = (fiducialRealX1 + fiducialRealX2) / 2.0;
                double glassCenterY = (fiducialRealY1 + fiducialRealY2) / 2.0;
                double glassAngle = Math.Atan2(fiducialRealY2 - fiducialRealY1, fiducialRealX2 - fiducialRealX1) * 180.0 / Math.PI;

                // 5. Mask와 Glass의 Center점을 비교하여 필요한 Glass 이동량 계산
                double deltaX = glassCenterX - maskCenterX;
                double deltaY = glassCenterY - maskCenterY;
                double deltaAngle = glassAngle - maskAngle;

                // 6. Global.clsUvwStage.CurrentPos를 가져와서 StageCalInverse로 현재 X,Y,T 위치 역산
                double currentX1 = Global.inforPLC.InStageCurrentU;
                double currentX2 = Global.inforPLC.InStageCurrentW;
                double currentY1 = Global.inforPLC.InStageCurrentV;

                ClsAlgorithm.StageCalInverse(currentX1, currentX2, currentY1,
                    out double currentX, out double currentY, out double currentAngle);

                // 7. 현재 위치에 이동량을 반영하여 Target Position 계산
                double targetX = currentX + deltaX;
                double targetY = currentY + deltaY;
                double targetAngle = currentAngle + deltaAngle;

                Global.inforResult.resultErrorX = deltaX;
                Global.inforResult.resultErrorY = deltaY;
                Global.inforResult.resultErrorT = deltaAngle;

                Global.inforResult.dFullProcessTime = Convert.ToDouble(stopwatch.ElapsedMilliseconds);
                Global.inforResult.dProcessTime[0] = Global.inforResult.dFullProcessTime / 2.0;
                Global.inforResult.dProcessTime[1] = Global.inforResult.dFullProcessTime / 2.0;

                string judgement = "";
                if (Global.PreConfig_Param.dLimitMark > maskScore1)
                    judgement = "Cam1 Mask Score NG";
                else if (Global.PreConfig_Param.dLimitMark > maskScore2)
                    judgement = "Cam2 Mask Score NG";
                else if (Global.PreConfig_Param.dPreAlignXY < Math.Abs(deltaX) || Global.PreConfig_Param.dPreAlignXY < Math.Abs(deltaY))
                    judgement = "XY Align NG";
                else if (Global.PreConfig_Param.dPreAlignTh < Math.Abs(deltaAngle))
                    judgement = "T Align NG";
                else
                    judgement = "OK";

                Global.inforResult.strJudg = judgement;
                Global.inforResult.TargetX = targetX;
                Global.inforResult.TargetY = targetY;
                Global.inforResult.TargetT = targetAngle;
                Global.inforResult.nRetry = Global.inforPLC.InPmcCurrentRetry;

                // 출력값 설정
                x = targetX;
                y = targetY;
                t = targetAngle;

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 읽어온 카메라 이미지에서 Fiducial 위치를 찾아준다.
        /// </summary>
        /// <param name="camInfo">Camera 번호</param>
        /// <param name="pixelX">찾은 FiducialMark X Pixel값</param>
        /// <param name="pixelY">찾은 FiducialMark Y Pixel값</param>
        /// <param name="score">찾은 FiducialMark Score</param>
        /// <returns></returns>
        /// 
        public bool FindFiducialMark(CamInfo camInfo, out double pixelX, out double pixelY, out double score, out double scale)
        {

            ModelParam modelParam = new ModelParam();

            Stopwatch stopwatch = new Stopwatch();
            int n = (int)camInfo;

            stopwatch.Restart();
            Global.inforResult.dGray[n] = Global.clsAlgorithm[n].MeanGrayValue();


            bool bRslt;

            modelParam.nNumLevels = Convert.ToInt32(Global.recipeGlass_Shm[n].nNumLevels);
            modelParam.dAngleMin = Convert.ToDouble(Global.recipeGlass_Shm[n].dAngleMin);
            modelParam.dAngleMax = Convert.ToDouble(Global.recipeGlass_Shm[n].dAngleMax);
            modelParam.dMinScore = Convert.ToDouble(Global.recipeGlass_Shm[n].dMinScore);
            modelParam.dMaxOverlap = Convert.ToDouble(Global.recipeGlass_Shm[n].dMaxOverlap);
            modelParam.strSubPixel = Global.recipeGlass_Shm[n].strSubPixel;
            modelParam.dGreedNess = Global.recipeGlass_Shm[n].dGreedNess;

            bRslt = Global.clsAlgorithm[n].FindShmModel(modelParam, Global.hModelInfo[n, (int)Matching_Type.GLASS_SHAPE], out ModelRslt rsltGlass);
            pixelX = rsltGlass.dX;
            pixelY = rsltGlass.dY;
            scale = rsltGlass.dScale;
            score = rsltGlass.dScore;

            ClsAlgorithm.OverlayCross(Global.formHDisplay[n].HWindow, rsltGlass.dX, rsltGlass.dY, rsltGlass.dAngle, "green");

            Global.clsAlgorithm[n].Find_DirectionMark(out ModelRslt rsltDir);
            ClsAlgorithm.OverlayCross(Global.formHDisplay[n].HWindow, rsltDir.dX, rsltDir.dY, rsltDir.dAngle, "red");

            return true;
        }

        /// <summary>
        /// 읽어온 카메라 이미지에서 Mask Hole 위치를 찾아준다.
        /// </summary>
        /// <param name="camInfo">Camera 번호</param>
        /// <param name="pixelX">찾은 Mask Hole X Pixel값</param>
        /// <param name="pixelY">찾은 Mask Hole Y Pixel값</param>
        /// <param name="score">찾은 Mask Hole Score</param>
        /// <returns></returns>
        public bool FindMaskHole(CamInfo camInfo, out double pixelX, out double pixelY, out double score, out double scale, Matching_Type matchingType = Matching_Type.MASK_SHAPE)
        {
            ModelParam modelParam = new ModelParam();

            Stopwatch stopwatch = new Stopwatch();
            int n = (int)camInfo;

            stopwatch.Restart();
            Global.inforResult.dGray[n] = Global.clsAlgorithm[n].MeanGrayValue();

            if (matchingType == Matching_Type.MARK_GRAY)
            {
                modelParam.nNumLevels = Convert.ToInt32(Global.recipeMask_Ncc[n].nNumLevels);
                modelParam.dAngleMin = Convert.ToDouble(Global.recipeMask_Ncc[n].dAngleMin);
                modelParam.dAngleMax = Convert.ToDouble(Global.recipeMask_Ncc[n].dAngleMax);
                modelParam.dMinScore = Convert.ToDouble(Global.recipeMask_Ncc[n].dMinScore);
                modelParam.dMaxOverlap = Convert.ToDouble(Global.recipeMask_Ncc[n].dMaxOverlap);
                modelParam.strSubPixel = Global.recipeMask_Ncc[n].strSubPixel;
            }
            else if (matchingType == Matching_Type.MASK_SHAPE)
            {
                modelParam.nNumLevels = Convert.ToInt32(Global.recipeMask_Shm[n].nNumLevels);
                modelParam.dAngleMin = Convert.ToDouble(Global.recipeMask_Shm[n].dAngleMin);
                modelParam.dAngleMax = Convert.ToDouble(Global.recipeMask_Shm[n].dAngleMax);
                modelParam.dMinScore = Convert.ToDouble(Global.recipeMask_Shm[n].dMinScore);
                modelParam.dMaxOverlap = Convert.ToDouble(Global.recipeMask_Shm[n].dMaxOverlap);
                modelParam.strSubPixel = Global.recipeMask_Shm[n].strSubPixel;
                modelParam.dGreedNess = Global.recipeMask_Shm[n].dGreedNess;
            }
            else
            { 
            }
            
            bool bRslt = false;
            ModelRslt rsltMark;
            if (matchingType == Matching_Type.MARK_GRAY)
                bRslt = Global.clsAlgorithm[n].NCC_FindModel(modelParam, Global.hModelInfo[n, (int)Matching_Type.MARK_GRAY], out rsltMark);
            else if (matchingType == Matching_Type.MASK_SHAPE)
                bRslt = Global.clsAlgorithm[n].FindShmModel(modelParam, Global.hModelInfo[n, (int)Matching_Type.MASK_SHAPE], out rsltMark);
            else
            {
                rsltMark = null;
                bRslt = false;
            }

            if (rsltMark != null)
            {
                pixelX = rsltMark.dX;
                pixelY = rsltMark.dY;
                scale = rsltMark.dScale;
                score = rsltMark.dScore;
                ClsAlgorithm.OverlayCross(Global.formHDisplay[n].HWindow, rsltMark.dX, rsltMark.dY, rsltMark.dAngle, "cyan");
            }
            else
            {
                pixelX = 0;
                pixelY = 0;
                scale = 0;
                score = 0;
            }

            return true;
        }

        /// <summary>
        /// 9 Point 이동하며 찍은 카메라이미지 좌표를 기준으로 PixelResolutionX,Y와 Tilt를 구한다.
        /// 9 Point Motor 이동 조건:
        ///  offsetMove = 1000 이라면
        ///  Point 1 = -1000,-1000
        ///  Point 2 = 0,-1000
        ///  Point 3 = 1000,-1000
        ///  Point 4 = -1000,0
        ///  Point 5 = 0,0
        ///  Point 6 = 1000,0
        ///  Point 7 = -1000,1000
        ///  Point 8 = 0,1000
        ///  Point 9 = 1000,1000
        ///  로 구동하여 얻어진 pixelPoints[9]를 적용할 것
        /// </summary>
        /// <param name="camInfo">Camera 번호</param>
        /// <param name="offsetMove">9개 좌표의 이동 간격</param>
        /// <param name="pixelPoints">Camera Pixel 좌표</param>
        /// <returns></returns>
        public bool CalibrationCamera(CamInfo camInfo, double offsetMove, PointF[] pixelPoints)
        {
            try
            {
                int camIdx = (int)camInfo;

                // ClsAlgorithm의 9 Point Calibration 알고리즘 호출
                bool result = Global.clsAlgorithm[camIdx].Calculate9PointCalibration(
                    offsetMove, pixelPoints,
                    out double pixelResolutionX,
                    out double pixelResolutionY,
                    out double tilt);

                if (!result)
                    return false;

                // 결과를 Global.Calibration_Param에 저장
                Global.Calibration_Param.CameraPixelResolutionX[camIdx] = pixelResolutionX;
                Global.Calibration_Param.CameraPixelResolutionY[camIdx] = pixelResolutionY;
                Global.Calibration_Param.CameraTilt[camIdx] = tilt;

                // JSON 파일로 저장
                Global.Calibration_Param = Global.IsLoad_List(false, Global.Calibration_Param);

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// UVW Stage Center(0,0) 대비 Camera의 Center 위치를 구하여 적용한다.
        /// pixelPoints 는 3점 이상이어야 한다.
        /// </summary>
        /// <param name="camInfo">Camera 번호</param>
        /// <param name="pixelPoints">호 Pixel 점들</param>
        /// <returns></returns>
        public bool CalibrationUvwStage(CamInfo camInfo, List<PointF> pixelPoints)
        {
            try
            {
                if (pixelPoints == null || pixelPoints.Count < 3)
                    return false;

                int camIdx = (int)camInfo;
                var calParam = Global.Calibration_Param;

                // 카메라 Width/Height 가져오기
                int cameraWidth = Global.dicClsCam[camInfo].Width;
                int cameraHeight = Global.dicClsCam[camInfo].Height;
                double cameraCenterX = cameraWidth / 2.0;
                double cameraCenterY = cameraHeight / 2.0;

                // Calibration 파라미터
                double pixelResolutionX = calParam.CameraPixelResolutionX[camIdx];
                double pixelResolutionY = calParam.CameraPixelResolutionY[camIdx];
                double tilt = calParam.CameraTilt[camIdx];
                double tiltRad = tilt * Math.PI / 180.0;
                double cos = Math.Cos(tiltRad);
                double sin = Math.Sin(tiltRad);

                // 1. pixelPoints를 PixelResolution과 Tilt를 사용하여 실제 스케일 값으로 변환
                List<PointF> realPoints = new List<PointF>();
                foreach (var pixelPoint in pixelPoints)
                {
                    // 카메라 중심 기준 픽셀 오프셋
                    double deltaPixelX = pixelPoint.X - cameraCenterX;
                    double deltaPixelY = pixelPoint.Y - cameraCenterY;

                    // PixelResolution 적용
                    double deltaRealX = deltaPixelX * pixelResolutionX;
                    double deltaRealY = deltaPixelY * pixelResolutionY;

                    // Tilt 회전 적용
                    double rotatedX = deltaRealX * cos - deltaRealY * sin;
                    double rotatedY = deltaRealX * sin + deltaRealY * cos;

                    realPoints.Add(new PointF((float)rotatedX, (float)rotatedY));
                }

                // 2. FitCircle을 사용하여 원의 중심과 반지름 계산
                ClsAlgorithm.FitCircle(realPoints, out PointF circleCenter, out double radius);

                // 3. 원의 중심이 UVW Stage Center(0,0) 기준이므로,
                //    카메라 중심에서 원의 중심까지의 오프셋이 Camera Center 위치가 됨
                //    (원의 중심이 0,0이 되려면 카메라가 -circleCenter 위치에 있어야 함)
                calParam.CameraCoordinateOffsetX[camIdx] = -circleCenter.X;
                calParam.CameraCoordinateOffsetY[camIdx] = -circleCenter.Y;

                // JSON 파일로 저장
                Global.Calibration_Param = Global.IsLoad_List(false, Global.Calibration_Param);

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Mask Hole의 설계적 치수가 Stage와 완벽하게 Align 되어있다는 가정하에 Calibration
        /// </summary>
        /// <param name="camInfo">Camera 번호</param>
        /// <param name="pixelPoint">Mask Hole 읽은 값</param>
        /// <returns></returns>
        public bool CalibrationUvwStage(CamInfo camInfo, PointF pixelPoint)
        {
            int camIdx = (int)camInfo;
            var calParam = Global.Calibration_Param;

            int cameraWidth = Global.dicClsCam[camInfo].Width;
            int cameraHeight = Global.dicClsCam[camInfo].Height;
            double cameraCenterX = cameraWidth / 2.0;
            double cameraCenterY = cameraHeight / 2.0;


            // Calibration 파라미터
            double pixelResolutionX = calParam.CameraPixelResolutionX[camIdx];
            double pixelResolutionY = calParam.CameraPixelResolutionY[camIdx];
            double tilt = calParam.CameraTilt[camIdx];
            double tiltRad = tilt * Math.PI / 180.0;
            double cos = Math.Cos(tiltRad);
            double sin = Math.Sin(tiltRad);

            PointF realPoint;
            // 카메라 중심 기준 픽셀 오프셋
            double deltaPixelX = pixelPoint.X - cameraCenterX;
            double deltaPixelY = pixelPoint.Y - cameraCenterY;

            // PixelResolution 적용
            double deltaRealX = deltaPixelX * pixelResolutionX;
            double deltaRealY = deltaPixelY * pixelResolutionY;

            // Tilt 회전 적용
            double rotatedX = deltaRealX * cos - deltaRealY * sin;
            double rotatedY = deltaRealX * sin + deltaRealY * cos;

            realPoint = new PointF((float)rotatedX, (float)rotatedY);

            if (camInfo == CamInfo.CAM_1)
            {
                calParam.CameraCoordinateOffsetX[camIdx] = (calParam.MaskHoleDistanceX / 2.0) + realPoint.X;
                calParam.CameraCoordinateOffsetY[camIdx] = -(calParam.MaskHoleDistanceY / 2.0) + realPoint.Y;
            }
            else //CAM_2
            {
                calParam.CameraCoordinateOffsetX[camIdx] = -(calParam.MaskHoleDistanceX / 2.0) + realPoint.X;
                calParam.CameraCoordinateOffsetY[camIdx] = (calParam.MaskHoleDistanceY / 2.0) + realPoint.Y;
            }

            return true;
        }

        /// <summary>
        /// Camera1과 Camera2의 Fiducial Mark 사이의 실제 하드웨어상 직선 거리를 계산한다.
        /// </summary>
        /// <param name="cam1PixelPoint">Camera1에서 찾은 Fiducial Mark 픽셀 좌표</param>
        /// <param name="cam2PixelPoint">Camera2에서 찾은 Fiducial Mark 픽셀 좌표</param>
        /// <returns>계산된 거리 (um), 실패 시 -1</returns>
        public double GetFiducialMarkDistance(PointF cam1PixelPoint, PointF cam2PixelPoint)
        {
            try
            {
                // 각 카메라의 픽셀 좌표를 실제 좌표로 변환
                double cam1RealX, cam1RealY;
                double cam2RealX, cam2RealY;

                if (!PixelPosToRealPos(CamInfo.CAM_1, cam1PixelPoint.X, cam1PixelPoint.Y, out cam1RealX, out cam1RealY))
                    return -1;

                if (!PixelPosToRealPos(CamInfo.CAM_2, cam2PixelPoint.X, cam2PixelPoint.Y, out cam2RealX, out cam2RealY))
                    return -1;

                // 두 Fiducial Mark 사이의 직선 거리 계산
                double deltaX = cam2RealX - cam1RealX;
                double deltaY = cam2RealY - cam1RealY;
                double distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

                // 결과 저장
                Global.Calibration_Param.MaskHoleDistanceC = distance;
                Global.Calibration_Param = Global.IsLoad_List(false, Global.Calibration_Param);

                return distance;
            }
            catch
            {
                return -1;
            }
        }

        /// <summary>
        /// Camera Pixel 좌표를 Hardware 실제 좌표로 변환한다.
        /// </summary>
        /// <param name="camInfo">Camera 번호 (CAM_1, CAM_2)</param>
        /// <param name="camPosX">Camera X Pixel 위치</param>
        /// <param name="camPosY">Camera Y Pixel 위치</param>
        /// <param name="realPosX">변환된 실제 X 좌표</param>
        /// <param name="realPosY">변환된 실제 Y 좌표</param>
        /// <returns>성공 여부</returns>
        public bool PixelPosToRealPos(CamInfo camInfo, double camPosX, double camPosY, out double realPosX, out double realPosY)
        {
            realPosX = 0;
            realPosY = 0;

            try
            {
                int camIdx = (int)camInfo;
                var calParam = Global.Calibration_Param;

                // 카메라 Width/Height 가져오기
                int cameraWidth = Global.dicClsCam[camInfo].Width;
                int cameraHeight = Global.dicClsCam[camInfo].Height;

                // Calibration 파라미터
                double calOffsetX = calParam.CameraCoordinateOffsetX[camIdx];
                double calOffsetY = calParam.CameraCoordinateOffsetY[camIdx];
                double calTilt = calParam.CameraTilt[camIdx];
                double pixelResolutionX = calParam.CameraPixelResolutionX[camIdx];
                double pixelResolutionY = calParam.CameraPixelResolutionY[camIdx];

                // GetRealPos 호출
                return Global.clsAlgorithm[camIdx].GetRealPos(
                    cameraWidth, cameraHeight,
                    calOffsetX, calOffsetY, calTilt,
                    pixelResolutionX, pixelResolutionY,
                    camPosX, camPosY,
                    out realPosX, out realPosY);
            }
            catch
            {
                return false;
            }
        }

    }
}
